namespace SubastaYa.Api.Models;

public class AuditoriaLog
{
    public int Id { get; set; }

    // Ej: "SUBASTA", "BILLETERA", "SISTEMA"
    public string Entidad { get; set; } = string.Empty;

    // Id del registro afectado
    public int EntidadId { get; set; }

    // Ej: "EXTENSION_TIEMPO", "CIERRE_WORKER", "PUJA_RECHAZADA", "ACREDITACION_MANUAL"
    public string Accion { get; set; } = string.Empty;

    // Opcional: null si la acción la disparó el Worker en segundo plano
    public int? UsuarioId { get; set; }

    // Payload libre con los detalles del cambio (JSON serializado)
    public string DetalleJson { get; set; } = "{}";

    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}
