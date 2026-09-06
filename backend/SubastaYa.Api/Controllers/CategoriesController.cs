using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Api.Data;

namespace SubastaYa.Api.Controllers;

[ApiController]
[Route("api/v1/categories")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _db;

    public CategoriesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var categorias = await _db.Categorias
            .Select(c => new { c.Id, c.Nombre, c.UrlIcono })
            .ToListAsync();
        return Ok(categorias);
    }
}
