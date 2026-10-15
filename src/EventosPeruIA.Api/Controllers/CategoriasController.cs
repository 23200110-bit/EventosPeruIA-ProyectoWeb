using EventosPeruIA.Domain.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
namespace EventosPeruIA.Api.Controllers;
[ApiController,Route("api/categorias")]
public sealed class CategoriasController(EventosPeruDbContext db):ControllerBase
{
 [HttpGet] public async Task<IActionResult> Get(CancellationToken ct)=>Ok(await db.Categorias.AsNoTracking().Where(x=>x.Activo).Select(x=>new{x.CategoriaId,x.Nombre}).ToListAsync(ct));
}
