namespace SubastaYa.Api.Models;

public class Categoria
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? UrlIcono { get; set; }

    public ICollection<Subasta> Subastas { get; set; } = new List<Subasta>();
}
