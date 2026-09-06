namespace SubastaYa.Api.Models;

public class Billetera
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public decimal SaldoTotal { get; set; }
    public decimal SaldoRetenido { get; set; }

    // Propiedad calculada, no se persiste como columna propia:
    // SaldoDisponible = SaldoTotal - SaldoRetenido
    public decimal SaldoDisponible => SaldoTotal - SaldoRetenido;

    // Concurrencia optimista para evitar carreras al retener/liberar saldo
    // cuando llegan pujas simultáneas sobre distintas subastas del mismo usuario.
    public int Version { get; set; } = 0;

    public ICollection<TransaccionLedger> Movimientos { get; set; } = new List<TransaccionLedger>();
}
