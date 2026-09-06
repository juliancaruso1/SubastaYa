using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Api.Data;

namespace SubastaYa.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/v1/users/{id}/bids -> "Mis Compras / Pujas"
    [HttpGet("{id:int}/bids")]
    public async Task<IActionResult> MisPujas(int id)
    {
        var pujas = await _db.Pujas
            .Where(p => p.CompradorId == id)
            .Include(p => p.Subasta)
            .GroupBy(p => p.SubastaId)
            .Select(g => new
            {
                SubastaId = g.Key,
                Titulo = g.First().Subasta!.Titulo,
                MiMejorOferta = g.Max(p => p.Monto),
                PujaActual = g.First().Subasta!.Pujas.Max(x => x.Monto),
                Estado = g.First().Subasta!.Estado,
                Gano = g.First().Subasta!.Estado.ToString() == "Finalizada"
                    && g.First().Subasta!.Pujas.OrderByDescending(x => x.Monto).First().CompradorId == id
            })
            .ToListAsync();

        return Ok(pujas);
    }

    // GET /api/v1/users/{id}/auctions -> "Mis Publicaciones"
    [HttpGet("{id:int}/auctions")]
    public async Task<IActionResult> MisPublicaciones(int id)
    {
        var publicaciones = await _db.Subastas
            .Where(s => s.VendedorId == id)
            .Include(s => s.Pujas)
            .Select(s => new
            {
                s.Id,
                s.Titulo,
                s.Estado,
                CantidadPujas = s.Pujas.Count,
                Recaudacion = s.Estado.ToString() == "Finalizada" ? s.Pujas.Max(p => p.Monto) : (decimal?)null
            })
            .ToListAsync();

        return Ok(publicaciones);
    }
}
