using SubastaYa.Api.Enums;

namespace SubastaYa.Api.Models;

public class Subasta
{
    public int Id { get; set; }

    public int VendedorId { get; set; }
    public Usuario? Vendedor { get; set; }

    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string UrlImagen { get; set; } = string.Empty;

    public decimal PrecioBase { get; set; }
    public decimal IncrementoMinimo { get; set; }

    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }

    public EstadoSubasta Estado { get; set; } = EstadoSubasta.Programada;

    // Concurrencia optimista (Optimistic Locking): se marca como ConcurrencyToken
    // en AppDbContext. Cada UPDATE exitoso lo incrementa manualmente (Version++).
    // Si dos requests leen el mismo valor y ambas intentan escribir,
    // la segunda falla con DbUpdateConcurrencyException -> se traduce a 409 Conflict.
    public int Version { get; set; } = 0;

    public ICollection<Puja> Pujas { get; set; } = new List<Puja>();

    public decimal PujaActual => Pujas.Count > 0 ? Pujas.Max(p => p.Monto) : PrecioBase;
}
