using Microsoft.EntityFrameworkCore;
using SubastaYa.Api.Data;
using SubastaYa.Api.DTOs;
using SubastaYa.Api.Enums;
using SubastaYa.Api.Models;

namespace SubastaYa.Api.Services;

public class WalletService : IWalletService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _auditService;

    public WalletService(AppDbContext db, IAuditService auditService)
    {
        _db = db;
        _auditService = auditService;
    }

    public async Task<SaldoResponse> ObtenerSaldoAsync(int usuarioId)
    {
        var billetera = await ObtenerBilleteraAsync(usuarioId);
        return new SaldoResponse(billetera.SaldoTotal, billetera.SaldoRetenido, billetera.SaldoDisponible);
    }

    public async Task<SaldoResponse> DepositarAsync(DepositoRequest request)
    {
        if (request.Monto <= 0)
            throw new SolicitudInvalidaException("El monto a depositar debe ser positivo.");

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var billetera = await ObtenerBilleteraAsync(request.UsuarioId);

            billetera.SaldoTotal += request.Monto;
            billetera.Version++;

            _db.TransaccionesLedger.Add(new TransaccionLedger
            {
                BilleteraId = billetera.Id,
                Tipo = TipoTransaccion.Deposito,
                Monto = request.Monto
            });

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            await _auditService.RegistrarAsync("BILLETERA", billetera.Id, "ACREDITACION_MANUAL", request.UsuarioId,
                new { request.Monto });

            return new SaldoResponse(billetera.SaldoTotal, billetera.SaldoRetenido, billetera.SaldoDisponible);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            throw new ConflictoConcurrenciaException("La billetera fue modificada en simultáneo. Intentá de nuevo.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<MovimientoResponse>> ObtenerMovimientosAsync(int usuarioId)
    {
        var billetera = await ObtenerBilleteraAsync(usuarioId);

        return await _db.TransaccionesLedger
            .Where(t => t.BilleteraId == billetera.Id)
            .OrderByDescending(t => t.Fecha)
            .Select(t => new MovimientoResponse(t.Tipo, t.Monto, t.Fecha, t.SubastaId))
            .ToListAsync();
    }

    private async Task<Billetera> ObtenerBilleteraAsync(int usuarioId)
    {
        var billetera = await _db.Billeteras.FirstOrDefaultAsync(b => b.UsuarioId == usuarioId);
        if (billetera is null)
            throw new RecursoNoEncontradoException($"El usuario {usuarioId} no tiene billetera asociada.");
        return billetera;
    }
}
