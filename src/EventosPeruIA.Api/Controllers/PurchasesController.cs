using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventosPeruIA.Api.DTOs;
using EventosPeruIA.Domain.Infrastructure.Data;
using EventosPeruIA.Domain.Core.Entities;

namespace EventosPeruIA.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchasesController : ControllerBase
    {
        private readonly EventosPeruDbContext _db;

        public PurchasesController(EventosPeruDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> Purchase([FromBody] PurchaseRequestDto req)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var comprador = await _db.Usuarios.FindAsync(req.CompradorId);
            if (comprador == null) return NotFound(new { error = "Comprador no encontrado" });

            var evento = await _db.Eventos.FindAsync(req.EventoId);
            if (evento == null) return NotFound(new { error = "Evento no encontrado" });

            // Calcular monto
            var total = evento.Precio * req.CantidadEntradas;

            // Crear compra
            var compra = new Compras
            {
                CompradorId = req.CompradorId,
                EventoId = req.EventoId,
                CantidadEntradas = req.CantidadEntradas,
                MontoTotal = total,
                CodigoTransaccion = Guid.NewGuid().ToString("N").Substring(0, 12),
                Estado = "Confirmada",
                FechaCompra = DateTime.UtcNow
            };
            _db.Compras.Add(compra);
            await _db.SaveChangesAsync();

            // Crear entradas asociadas
            for (int i = 0; i < req.CantidadEntradas; i++)
            {
                var entrada = new Entradas
                {
                    CompraId = compra.CompraId,
                    EventoId = req.EventoId,
                    CompradorId = req.CompradorId,
                    PrecioPagado = evento.Precio,
                    Estado = "Valida",
                    FechaEmision = DateTime.UtcNow
                };
                _db.Entradas.Add(entrada);
            }
            await _db.SaveChangesAsync();

            var resp = new PurchaseResponseDto
            {
                CompraId = compra.CompraId,
                EventoId = compra.EventoId,
                CompradorId = compra.CompradorId,
                CantidadEntradas = compra.CantidadEntradas,
                MontoTotal = compra.MontoTotal,
                CodigoTransaccion = compra.CodigoTransaccion ?? string.Empty,
                Estado = compra.Estado,
                FechaCompra = compra.FechaCompra
            };

            return CreatedAtAction(null, resp);
        }

        [HttpGet]
        public async Task<IActionResult> GetPurchases()
        {
            var list = await _db.Compras
                .Include(c => c.Evento)
                .Include(c => c.Comprador)
                .OrderByDescending(c => c.FechaCompra)
                .Select(c => new PurchaseResponseDto
                {
                    CompraId = c.CompraId,
                    EventoId = c.EventoId,
                    CompradorId = c.CompradorId,
                    CantidadEntradas = c.CantidadEntradas,
                    MontoTotal = c.MontoTotal,
                    CodigoTransaccion = c.CodigoTransaccion ?? string.Empty,
                    Estado = c.Estado,
                    FechaCompra = c.FechaCompra
                })
                .ToListAsync();
            return Ok(list);
        }
    }
}
