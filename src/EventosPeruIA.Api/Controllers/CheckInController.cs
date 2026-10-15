using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventosPeruIA.Domain.Core.Entities;
using EventosPeruIA.Domain.Core.DTOs;
using EventosPeruIA.Domain.Infrastructure.Data;

namespace EventosPeruIA.Api.Controllers
{
	[ApiController]
	[Route("api/tickets")]
	public class CheckInController : ControllerBase
	{
		private readonly EventosPeruDbContext _context;

		public CheckInController(EventosPeruDbContext context)
		{
			_context = context;
		}

		// POST: /api/tickets/validate-qr (US-012)
		[HttpPost("validate-qr")]
		public async Task<IActionResult> ValidateQr([FromBody] ValidateCheckInRequestDTO request)
		{
			if (string.IsNullOrWhiteSpace(request.CodigoTicket))
			{
				return BadRequest(new { success = false, message = "El código o UUID de la entrada es obligatorio." });
			}

			// 1. Buscar la entrada por UUID (Guid) o por CodigoCorto
			Entradas? entrada = null;

			if (Guid.TryParse(request.CodigoTicket, out Guid ticketGuid))
			{
				entrada = await _context.Entradas
					.FirstOrDefaultAsync(t => t.EntradaId == ticketGuid && t.EventoId == request.EventoId);
			}
			else
			{
				entrada = await _context.Entradas
					.FirstOrDefaultAsync(t => t.CodigoCorto == request.CodigoTicket && t.EventoId == request.EventoId);
			}

			// 2. Si la entrada no existe o no pertenece al evento
			if (entrada == null)
			{
				// Registrar intento fallido en CheckIns para auditoría
				return NotFound(new { success = false, message = "La entrada no existe o no corresponde a este evento." });
			}

			// 3. Si la entrada ya fue utilizada previamente (Regla RN-05 / Anti-fraude)
			if (entrada.Estado == "Utilizada")
			{
				var checkInFallido = new CheckIns
				{
					EntradaId = entrada.EntradaId,
					ValidadoPorId = request.ValidadoPorId > 0 ? request.ValidadoPorId : 1,
					MetodoIngreso = request.MetodoIngreso,
					Resultado = "Rechazado",
					MotivoRechazo = "La entrada ya fue utilizada previamente.",
					FechaHora = DateTime.UtcNow
				};
				_context.CheckIns.Add(checkInFallido);
				await _context.SaveChangesAsync();

				return BadRequest(new { success = false, message = "Acceso denegado: Esta entrada ya fue utilizada." });
			}

			// 4. Si la entrada está Anulada por cancelación del evento
			if (entrada.Estado == "Anulada")
			{
				return BadRequest(new { success = false, message = "Acceso denegado: La entrada se encuentra anulada." });
			}

			// 5. Validación Exitosa: Cambiar estado a 'Utilizada' de forma atómica
			entrada.Estado = "Utilizada";
			entrada.FechaUso = DateTime.UtcNow;

			var checkInExitoso = new CheckIns
			{
				EntradaId = entrada.EntradaId,
				ValidadoPorId = request.ValidadoPorId > 0 ? request.ValidadoPorId : 1,
				MetodoIngreso = request.MetodoIngreso,
				Resultado = "Aprobado",
				MotivoRechazo = null,
				FechaHora = DateTime.UtcNow
			};
			_context.CheckIns.Add(checkInExitoso);

			await _context.SaveChangesAsync();

			return Ok(new
			{
				success = true,
				message = "¡Check-in aprobado! Ingreso autorizado.",
				entradaId = entrada.EntradaId,
				codigoCorto = entrada.CodigoCorto,
				fechaUso = entrada.FechaUso
			});
		}
	}
}