using Microsoft.EntityFrameworkCore;
using SubastaYa.Api.Data;
using SubastaYa.Api.DTOs;
using SubastaYa.Api.Enums;
using SubastaYa.Api.Hubs;
using SubastaYa.Api.Models;
using Microsoft.AspNetCore.SignalR;

namespace SubastaYa.Api.Services;

public class AuctionService : IAuctionService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _auditService;
    private readonly IHubContext<AuctionHub> _hub;
    private readonly int _ventanaCriticaSegundos;
    private readonly int _extensionMinutos;

    public AuctionService(AppDbContext db, IAuditService auditService, IHubContext<AuctionHub> hub, IConfiguration config)
    {
        _db = db;
        _auditService = auditService;
        _hub = hub;
        _ventanaCriticaSegundos = config.GetValue<int>("AntiSniping:VentanaCriticaSegundos", 60);
        _extensionMinutos = config.GetValue<int>("AntiSniping:ExtensionMinutos", 2);
    }

    public async Task<List<SubastaResumenResponse>> ListarAsync(string? estado, int? categoriaId, decimal? precioMin, decimal? precioMax, string? orden)
    {
        var query = _db.Subastas
            .Include(s => s.Categoria)
            .Include(s => s.Pujas)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(estado) && Enum.TryParse<EstadoSubasta>(estado, true, out var estadoEnum))
            query = query.Where(s => s.Estado == estadoEnum);

        if (categoriaId.HasValue)
            query = query.Where(s => s.CategoriaId == categoriaId.Value);

        var subastas = await query.ToListAsync();

        var resultado = subastas.Select(s => new SubastaResumenResponse(
            s.Id, s.Titulo, s.UrlImagen, s.Categoria?.Nombre ?? "", s.PujaActual,
            s.Pujas.Count, s.FechaFin, s.Estado
        )).AsEnumerable();

        if (precioMin.HasValue)
            resultado = resultado.Where(s => s.PujaActual >= precioMin.Value);
        if (precioMax.HasValue)
            resultado = resultado.Where(s => s.PujaActual <= precioMax.Value);

        resultado = orden switch
        {
            "tiempo_restante" => resultado.OrderBy(s => s.FechaFin),
            "mayor_puja" => resultado.OrderByDescending(s => s.PujaActual),
            _ => resultado.OrderBy(s => s.FechaFin)
        };

        return resultado.ToList();
    }

    public async Task<SubastaDetalleResponse> ObtenerDetalleAsync(int subastaId)
    {
        var subasta = await CargarSubastaConPujasAsync(subastaId);
        return MapearDetalle(subasta);
    }

    public async Task<SubastaDetalleResponse> CrearAsync(CrearSubastaRequest request)
    {
        if (request.FechaFin <= request.FechaInicio)
            throw new SolicitudInvalidaException("La fecha de finalización debe ser posterior a la de inicio.");

        if (request.PrecioBase <= 0 || request.IncrementoMinimo <= 0)
            throw new SolicitudInvalidaException("El precio base y el incremento mínimo deben ser valores positivos.");

        var vendedorExiste = await _db.Usuarios.AnyAsync(u => u.Id == request.VendedorId);
        if (!vendedorExiste)
            throw new RecursoNoEncontradoException("El vendedor indicado no existe.");

        var categoriaExiste = await _db.Categorias.AnyAsync(c => c.Id == request.CategoriaId);
        if (!categoriaExiste)
            throw new RecursoNoEncontradoException("La categoría indicada no existe.");

        var estadoInicial = request.FechaInicio <= DateTime.UtcNow ? EstadoSubasta.Activa : EstadoSubasta.Programada;

        var subasta = new Subasta
        {
            VendedorId = request.VendedorId,
            CategoriaId = request.CategoriaId,
            Titulo = request.Titulo,
            Descripcion = request.Descripcion,
            UrlImagen = request.UrlImagen,
            PrecioBase = request.PrecioBase,
            IncrementoMinimo = request.IncrementoMinimo,
            FechaInicio = request.FechaInicio,
            FechaFin = request.FechaFin,
            Estado = estadoInicial,
            Version = 0
        };

        _db.Subastas.Add(subasta);
        await _db.SaveChangesAsync();

        var creada = await CargarSubastaConPujasAsync(subasta.Id);
        return MapearDetalle(creada);
    }

    // Núcleo transaccional: valida, libera la retención del líder anterior,
    // congela el saldo del nuevo líder, registra la puja, aplica anti-sniping
    // y deja rastro de auditoría — todo en un único bloque atómico (ACID).
    public async Task<PujaResultadoResponse> RegistrarPujaAsync(int subastaId, CrearPujaRequest request)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var subasta = await _db.Subastas
                .Include(s => s.Pujas)
                .FirstOrDefaultAsync(s => s.Id == subastaId);

            if (subasta is null)
                throw new RecursoNoEncontradoException($"No existe la subasta {subastaId}.");

            if (subasta.Estado != EstadoSubasta.Activa || subasta.FechaInicio > DateTime.UtcNow || subasta.FechaFin <= DateTime.UtcNow)
                throw new SolicitudInvalidaException("La subasta no está activa.");

            // Chequeo temprano de versión: si el cliente ofertó con una foto
            // desactualizada del estado, se lo tratamos como conflicto directo.
            if (request.VersionEsperada != subasta.Version)
                throw new ConflictoConcurrenciaException("La subasta cambió de estado desde que la consultaste. Refrescá e intentá de nuevo.");

            var pujaActual = subasta.Pujas.Count > 0 ? subasta.Pujas.Max(p => p.Monto) : subasta.PrecioBase;
            var montoMinimo = pujaActual + subasta.IncrementoMinimo;

            if (request.Monto < montoMinimo)
                throw new SolicitudInvalidaException($"El monto debe ser al menos {montoMinimo}.");

            var comprador = await _db.Usuarios.Include(u => u.Billetera)
                .FirstOrDefaultAsync(u => u.Id == request.CompradorId);
            if (comprador?.Billetera is null)
                throw new RecursoNoEncontradoException("El comprador o su billetera no existen.");

            var billeteraComprador = comprador.Billetera;
            if (billeteraComprador.SaldoDisponible < request.Monto)
                throw new SaldoInsuficienteException("Saldo disponible insuficiente para cubrir la puja.");

            // 1) Liberar la retención del postor líder anterior (si existía uno distinto).
            var pujaLiderAnterior = subasta.Pujas.OrderByDescending(p => p.Monto).FirstOrDefault();
            if (pujaLiderAnterior is not null && pujaLiderAnterior.CompradorId != request.CompradorId)
            {
                var billeteraAnterior = await _db.Billeteras.FirstAsync(b => b.UsuarioId == pujaLiderAnterior.CompradorId);
                billeteraAnterior.SaldoRetenido -= pujaLiderAnterior.Monto;
                billeteraAnterior.Version++;
                _db.TransaccionesLedger.Add(new TransaccionLedger
                {
                    BilleteraId = billeteraAnterior.Id,
                    Tipo = TipoTransaccion.Liberacion,
                    Monto = pujaLiderAnterior.Monto,
                    SubastaId = subasta.Id
                });
            }

            // 2) Congelar el saldo del nuevo postor líder.
            billeteraComprador.SaldoRetenido += request.Monto;
            billeteraComprador.Version++;
            _db.TransaccionesLedger.Add(new TransaccionLedger
            {
                BilleteraId = billeteraComprador.Id,
                Tipo = TipoTransaccion.Retencion,
                Monto = request.Monto,
                SubastaId = subasta.Id
            });

            // 3) Registrar la puja líder.
            var nuevaPuja = new Puja
            {
                SubastaId = subasta.Id,
                CompradorId = request.CompradorId,
                Monto = request.Monto,
                FechaPuja = DateTime.UtcNow
            };
            _db.Pujas.Add(nuevaPuja);

            // 4) Regla Anti-Sniping: si entra dentro de la ventana crítica, extender el cierre.
            var extendida = false;
            var segundosParaCierre = (subasta.FechaFin - DateTime.UtcNow).TotalSeconds;
            if (segundosParaCierre <= _ventanaCriticaSegundos)
            {
                subasta.FechaFin = subasta.FechaFin.AddMinutes(_extensionMinutos);
                extendida = true;
            }

            subasta.Version++;

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            if (extendida)
            {
                await _auditService.RegistrarAsync("SUBASTA", subasta.Id, "EXTENSION_TIEMPO", request.CompradorId,
                    new { subasta.FechaFin, MinutosExtendidos = _extensionMinutos });
            }

            var actualizada = await CargarSubastaConPujasAsync(subasta.Id);
            var detalle = MapearDetalle(actualizada);

            // Notificación en tiempo real a todos los clientes conectados a la sala.
            await _hub.Clients.Group(GrupoSubasta(subasta.Id)).SendAsync("PujaRegistrada", detalle);

            return new PujaResultadoResponse(extendida, actualizada.FechaFin, detalle);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            await _auditService.RegistrarAsync("SUBASTA", subastaId, "PUJA_RECHAZADA_CONCURRENCIA", request.CompradorId,
                new { request.Monto, Motivo = "DbUpdateConcurrencyException" });
            throw new ConflictoConcurrenciaException("Otra puja se procesó en simultáneo. Refrescá e intentá de nuevo.");
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public static string GrupoSubasta(int subastaId) => $"subasta-{subastaId}";

    private async Task<Subasta> CargarSubastaConPujasAsync(int subastaId)
    {
        var subasta = await _db.Subastas
            .Include(s => s.Categoria)
            .Include(s => s.Pujas.OrderByDescending(p => p.FechaPuja))
            .ThenInclude(p => p.Comprador)
            .FirstOrDefaultAsync(s => s.Id == subastaId);

        if (subasta is null)
            throw new RecursoNoEncontradoException($"No existe la subasta {subastaId}.");

        return subasta;
    }

    private SubastaDetalleResponse MapearDetalle(Subasta subasta)
    {
        var pujaActual = subasta.Pujas.Count > 0 ? subasta.Pujas.Max(p => p.Monto) : subasta.PrecioBase;

        var historial = subasta.Pujas
            .OrderByDescending(p => p.FechaPuja)
            .Select(p => new PujaResponse(p.Id, p.CompradorId, Seudonimizar(p.Comprador?.Nombre ?? "Usuario"), p.Monto, p.FechaPuja))
            .ToList();

        return new SubastaDetalleResponse(
            subasta.Id, subasta.Titulo, subasta.Descripcion, subasta.UrlImagen,
            subasta.Categoria?.Nombre ?? "", subasta.VendedorId,
            subasta.PrecioBase, subasta.IncrementoMinimo, pujaActual,
            pujaActual + subasta.IncrementoMinimo,
            subasta.FechaInicio, subasta.FechaFin, subasta.Estado, subasta.Version,
            historial
        );
    }

    // Anonimiza al postor para la vista pública del historial de ofertas.
    private static string Seudonimizar(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre)) return "Postor";
        var inicial = nombre.Trim()[0];
        return $"{inicial}***";
    }
}
