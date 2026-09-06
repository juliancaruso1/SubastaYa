using SubastaYa.Api.Enums;

namespace SubastaYa.Api.Models;

public class TransaccionLedger
{
    public int Id { get; set; }

    public int BilleteraId { get; set; }
    public Billetera? Billetera { get; set; }

    public TipoTransaccion Tipo { get; set; }
    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    // Trazabilidad opcional hacia la subasta que originó el movimiento
    public int? SubastaId { get; set; }
}
