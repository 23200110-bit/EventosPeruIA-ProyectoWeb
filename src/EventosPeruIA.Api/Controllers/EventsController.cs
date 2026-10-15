using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventosPeruIA.Domain.Infrastructure.Data;

namespace EventosPeruIA.Api.Controllers
{
	[ApiController]
	[Route("api/events")]
	public class EventsController : ControllerBase
	{
		private readonly EventosPeruDbContext _context;

		public EventsController(EventosPeruDbContext context)
		{
			_context = context;
		}

		// GET: /api/events?categoriaId=1&ubicacion=Lima&fecha=2026-11-20
		// US-006: Catálogo de eventos con filtros combinados
		[HttpGet]
		public async Task<IActionResult> Listar(
			[FromQuery] int? categoriaId,
			[FromQuery] string? ubicacion,
			[FromQuery] DateTime? fecha)
		{
			var query = _context.Eventos
				.Include(e => e.Categoria)
				.Where(e => e.Estado == "Publicado")
				.AsQueryable();

			if (categoriaId.HasValue)
			{
				query = query.Where(e => e.CategoriaId == categoriaId.Value);
			}

			if (!string.IsNullOrWhiteSpace(ubicacion))
			{
				query = query.Where(e => e.Direccion.Contains(ubicacion));
			}

			if (fecha.HasValue)
			{
				var fechaFiltro = fecha.Value.Date;
				query = query.Where(e => e.FechaInicio.Date == fechaFiltro);
			}

			var eventos = await query.Select(e => new
			{
				e.EventoId,
				e.Nombre,
				e.Descripcion,
				e.FechaInicio,
				e.FechaFin,
				e.Direccion,
				e.Precio,
				e.Aforo,
				e.Estado,
				Categoria = e.Categoria != null ? e.Categoria.Nombre : null
			}).ToListAsync();

			return Ok(eventos);
		}

		// GET: /api/events/5
		// US-006: Detalle del evento con cálculo en tiempo real de cupos disponibles
		[HttpGet("{id}")]
		public async Task<IActionResult> ObtenerDetalle(int id)
		{
			var evento = await _context.Eventos
				.Include(e => e.Categoria)
				.FirstOrDefaultAsync(e => e.EventoId == id);

			if (evento == null)
			{
				return NotFound(new { success = false, message = $"El evento con ID {id} no fue encontrado." });
			}

			// Regla RN-02 / US-006: Cálculo exacto de entradas vendidas activas
			var ticketsVendidos = await _context.Entradas
				.CountAsync(t => t.EventoId == id && (t.Estado == "Valida" || t.Estado == "Utilizada"));

			var cuposDisponibles = Math.Max(0, evento.Aforo - ticketsVendidos);

			var detalle = new
			{
				evento.EventoId,
				evento.Nombre,
				evento.Descripcion,
				evento.FechaInicio,
				evento.FechaFin,
				evento.Direccion,
				evento.Precio,
				evento.Aforo,
				evento.Estado,
				Categoria = evento.Categoria != null ? evento.Categoria.Nombre : null,
				TicketsVendidos = ticketsVendidos,
				CuposDisponibles = cuposDisponibles,
				Agotado = cuposDisponibles == 0
			};

			return Ok(detalle);
		}
	}
}