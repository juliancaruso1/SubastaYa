using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Api.Data;
using SubastaYa.Api.Enums;
using SubastaYa.Api.Hubs;
using SubastaYa.Api.Models;
using SubastaYa.Api.Services;

namespace SubastaYa.Api.Workers;

// Proceso en segundo plano (equivalente a un cron job) que cada N segundos:
// 1) Busca subastas ACTIVA cuya FechaFin ya pasó.
// 2) Si tienen puja ganadora: liquida (debita comprador, acredita vendedor),
//    marca FINALIZADA y audita el evento.
// 3) Si no tienen ninguna puja: pasa a DESIERTA y audita el evento.
// Cada liquidación corre en su propia transacción atómica.
public class AuctionClosingWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<AuctionClosingWorker> _logger;
    private readonly TimeSpan _intervalo;

    public AuctionClosingWorker(IServiceProvider services, ILogger<AuctionClosingWorker> logger, IConfiguration config)
    {
        _services = services;
        _logger = logger;
        _intervalo = TimeSpan.FromSeconds(config.GetValue<int>("Worker:IntervaloSegundos", 10));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcesarSubastasVencidasAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando subastas vencidas en el Worker.");
            }

            await Task.Delay(_intervalo, stoppingToken);
        }
    }

    private async Task ProcesarSubastasVencidasAsync(CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var auditService = scope.ServiceProvider.GetRequiredService<IAuditService>();
        var hub = scope.ServiceProvider.GetRequiredService<IHubContext<AuctionHub>>();

        var vencidas = await db.Subastas
            .Include(s => s.Pujas)
            .Where(s => s.Estado == EstadoSubasta.Activa && s.FechaFin <= DateTime.UtcNow)
            .ToListAsync(ct);

        foreach (var subasta in vencidas)
        {
            await using var transaction = await db.Database.BeginTransactionAsync(ct);
            try
            {
                var pujaGanadora = subasta.Pujas.OrderByDescending(p => p.Monto).FirstOrDefault();

                if (pujaGanadora is null)
                {
                    subasta.Estado = EstadoSubasta.Desierta;
                    subasta.Version++;
                    await db.SaveChangesAsync(ct);
                    await transaction.CommitAsync(ct);

                    await auditService.RegistrarAsync("SUBASTA", subasta.Id, "PASE_A_DESIERTA", null, new { subasta.FechaFin });
                }
                else
                {
                    var billeteraComprador = await db.Billeteras.FirstAsync(b => b.UsuarioId == pujaGanadora.CompradorId, ct);
                    var billeteraVendedor = await db.Billeteras.FirstAsync(b => b.UsuarioId == subasta.VendedorId, ct);

                    // 1. Debitar al comprador: se liquida lo que tenía retenido en garantía.
                    billeteraComprador.SaldoTotal -= pujaGanadora.Monto;
                    billeteraComprador.SaldoRetenido -= pujaGanadora.Monto;
                    billeteraComprador.Version++;

                    // 2. Acreditar al vendedor.
                    billeteraVendedor.SaldoTotal += pujaGanadora.Monto;
                    billeteraVendedor.Version++;

                    db.TransaccionesLedger.Add(new TransaccionLedger
                    {
                        BilleteraId = billeteraComprador.Id, Tipo = TipoTransaccion.Pago,
                        Monto = pujaGanadora.Monto, SubastaId = subasta.Id
                    });
                    db.TransaccionesLedger.Add(new TransaccionLedger
                    {
                        BilleteraId = billeteraVendedor.Id, Tipo = TipoTransaccion.Cobro,
                        Monto = pujaGanadora.Monto, SubastaId = subasta.Id
                    });

                    // 3. Marcar la subasta como finalizada.
                    subasta.Estado = EstadoSubasta.Finalizada;
                    subasta.Version++;

                    await db.SaveChangesAsync(ct);
                    await transaction.CommitAsync(ct);

                    await auditService.RegistrarAsync("SUBASTA", subasta.Id, "CIERRE_WORKER", null,
                        new { Ganador = pujaGanadora.CompradorId, Monto = pujaGanadora.Monto });

                    await hub.Clients.Group(AuctionService.GrupoSubasta(subasta.Id))
                        .SendAsync("SubastaCerrada", new { subasta.Id, GanadorId = pujaGanadora.CompradorId, Monto = pujaGanadora.Monto }, ct);
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                _logger.LogError(ex, "Error liquidando la subasta {SubastaId}, se hizo rollback.", subasta.Id);
            }
        }
    }
}
