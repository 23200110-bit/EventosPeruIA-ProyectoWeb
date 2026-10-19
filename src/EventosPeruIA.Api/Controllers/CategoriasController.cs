using EventosPeruIA.CORE.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventosPeruIA.Api.Controllers;

[ApiController]
[Route("api/categorias")]
public sealed class CategoriasController(EventosPeruDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var categorias = await db.Categorias
            .AsNoTracking()
            .Where(x => x.Activo)
            .Select(x => new { x.CategoriaId, x.Nombre })
            .ToListAsync(ct);

        return Ok(categorias);
    }
}