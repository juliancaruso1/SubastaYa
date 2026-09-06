namespace SubastaYa.Api.Services;

public interface IAuditService
{
    Task RegistrarAsync(string entidad, int entidadId, string accion, int? usuarioId, object detalle);
}
