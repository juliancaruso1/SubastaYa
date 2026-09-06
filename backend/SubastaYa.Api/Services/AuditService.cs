using System.Text.Json;
using SubastaYa.Api.Data;
using SubastaYa.Api.Models;

namespace SubastaYa.Api.Services;

// Registra eventos de forma inmutable (append-only) en la tabla de auditoría.
// No expone update/delete: una vez escrito, un registro de auditoría no se modifica.
public class AuditService : IAuditService
{
    private readonly AppDbContext _db;

    public AuditService(AppDbContext db)
    {
        _db = db;
    }

    public async Task RegistrarAsync(string entidad, int entidadId, string accion, int? usuarioId, object detalle)
    {
        var log = new AuditoriaLog
        {
            Entidad = entidad,
            EntidadId = entidadId,
            Accion = accion,
            UsuarioId = usuarioId,
            DetalleJson = JsonSerializer.Serialize(detalle),
            Fecha = DateTime.UtcNow
        };

        _db.AuditoriaLogs.Add(log);
        await _db.SaveChangesAsync();
    }
}
