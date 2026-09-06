using SubastaYa.Api.Enums;

namespace SubastaYa.Api.DTOs;

public record CrearSubastaRequest(
    int VendedorId,
    string Titulo,
    string Descripcion,
    string UrlImagen,
    int CategoriaId,
    decimal PrecioBase,
    decimal IncrementoMinimo,
    DateTime FechaInicio,
    DateTime FechaFin
);

public record SubastaResumenResponse(
    int Id,
    string Titulo,
    string UrlImagen,
    string Categoria,
    decimal PujaActual,
    int CantidadPujas,
    DateTime FechaFin,
    EstadoSubasta Estado
);

public record SubastaDetalleResponse(
    int Id,
    string Titulo,
    string Descripcion,
    string UrlImagen,
    string Categoria,
    int VendedorId,
    decimal PrecioBase,
    decimal IncrementoMinimo,
    decimal PujaActual,
    decimal ProximaPujaSugerida,
    DateTime FechaInicio,
    DateTime FechaFin,
    EstadoSubasta Estado,
    int Version,
    List<PujaResponse> HistorialOfertas
);

public record PujaResponse(
    int Id,
    int CompradorId,
    string CompradorSeudonimo,
    decimal Monto,
    DateTime FechaPuja
);

public record CrearPujaRequest(
    int CompradorId,
    decimal Monto,
    // El cliente envía la versión que tenía al momento de ofertar,
    // para que el backend detecte conflictos de concurrencia optimista.
    int VersionEsperada
);

public record PujaResultadoResponse(
    bool Extendida,
    DateTime NuevaFechaFin,
    SubastaDetalleResponse Subasta
);
