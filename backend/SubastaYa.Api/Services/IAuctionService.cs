using SubastaYa.Api.DTOs;

namespace SubastaYa.Api.Services;

public interface IAuctionService
{
    Task<List<SubastaResumenResponse>> ListarAsync(string? estado, int? categoriaId, decimal? precioMin, decimal? precioMax, string? orden);
    Task<SubastaDetalleResponse> ObtenerDetalleAsync(int subastaId);
    Task<SubastaDetalleResponse> CrearAsync(CrearSubastaRequest request);
    Task<PujaResultadoResponse> RegistrarPujaAsync(int subastaId, CrearPujaRequest request);
}
