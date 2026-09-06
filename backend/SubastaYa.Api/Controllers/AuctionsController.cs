using Microsoft.AspNetCore.Mvc;
using SubastaYa.Api.DTOs;
using SubastaYa.Api.Services;

namespace SubastaYa.Api.Controllers;

[ApiController]
[Route("api/v1/auctions")]
public class AuctionsController : ControllerBase
{
    private readonly IAuctionService _auctionService;

    public AuctionsController(IAuctionService auctionService)
    {
        _auctionService = auctionService;
    }

    // GET /api/v1/auctions?estado=Activa&categoriaId=1&precioMin=100&precioMax=1000&orden=tiempo_restante
    [HttpGet]
    public async Task<ActionResult<List<SubastaResumenResponse>>> Listar(
        [FromQuery] string? estado, [FromQuery] int? categoriaId,
        [FromQuery] decimal? precioMin, [FromQuery] decimal? precioMax, [FromQuery] string? orden)
    {
        var resultado = await _auctionService.ListarAsync(estado, categoriaId, precioMin, precioMax, orden);
        return Ok(resultado);
    }

    // GET /api/v1/auctions/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<SubastaDetalleResponse>> ObtenerDetalle(int id)
    {
        var detalle = await _auctionService.ObtenerDetalleAsync(id);
        return Ok(detalle);
    }

    // POST /api/v1/auctions
    [HttpPost]
    public async Task<ActionResult<SubastaDetalleResponse>> Crear([FromBody] CrearSubastaRequest request)
    {
        var creada = await _auctionService.CrearAsync(request);
        return CreatedAtAction(nameof(ObtenerDetalle), new { id = creada.Id }, creada);
    }

    // POST /api/v1/auctions/{id}/bids  -> subrecurso "bids" (sustantivo plural), no un verbo como "/bid"
    [HttpPost("{id:int}/bids")]
    public async Task<ActionResult<PujaResultadoResponse>> RegistrarPuja(int id, [FromBody] CrearPujaRequest request)
    {
        var resultado = await _auctionService.RegistrarPujaAsync(id, request);
        return Ok(resultado);
    }
}
