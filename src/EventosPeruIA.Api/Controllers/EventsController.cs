using System.Security.Claims;
using EventosPeruIA.Domain.Core.DTOs;
using EventosPeruIA.Domain.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventosPeruIA.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EventsController : ControllerBase
    {
        private readonly IEventoService _eventoService;

        public EventsController(IEventoService eventoService)
        {
            _eventoService = eventoService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvento([FromBody] EventoCreateDTO evento)
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var eventoId = await _eventoService.CreateEvento(userId, evento);
            if (eventoId == 0)
                return BadRequest("No se pudo crear el evento. Verifica la categoría y que las fechas sean futuras y coherentes.");

            return Created($"/api/events/{eventoId}", new { eventoId });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvento([FromBody] EventoUpdateDTO evento, int id)
        {
            if (id != evento.EventoId)
                return BadRequest();

            var userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var existing = await _eventoService.GetEventoById(id);
            if (existing == null) return NotFound();
            if (existing.OrganizadorId != userId) return Forbid();

            var result = await _eventoService.UpdateEvento(evento);
            if (!result)
                return BadRequest("No se pudo actualizar el evento. Verifica su estado, la categoría, las fechas y que el aforo no sea menor a las entradas vendidas.");

            return NoContent();
        }

        [HttpPatch("{id}/cancel")]
        public async Task<IActionResult> CancelEvento([FromBody] EventoCancelDTO evento, int id)
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var existing = await _eventoService.GetEventoById(id);
            if (existing == null) return NotFound();
            if (existing.OrganizadorId != userId) return Forbid();

            var result = await _eventoService.CancelEvento(id, userId, evento);
            if (!result)
                return BadRequest("No se pudo cancelar el evento. Verifica que esté en Borrador o Publicado, que no haya iniciado y que el motivo sea válido.");

            return NoContent();
        }

        private int GetUserId()
        {
            var claim = User.FindFirst("UsuarioId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null) return 0;
            return int.TryParse(claim.Value, out var id) ? id : 0;
        }
    }
}