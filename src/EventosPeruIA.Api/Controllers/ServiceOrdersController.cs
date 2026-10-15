using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using EventosPeruIA.Api.DTOs;

namespace EventosPeruIA.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceOrdersController : ControllerBase
    {
        // Almacenamiento en memoria para prototipo; reemplazar por persistencia real si se desea
        private static readonly List<ServiceOrderDto> Orders = new();
        private static int NextId = 1;

        [HttpPost]
        public IActionResult Create([FromBody] ServiceOrderCreateDto model)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            var order = new ServiceOrderDto
            {
                Id = NextId++,
                Customer = model.Customer,
                Description = model.Description,
                AssignedTo = null,
                Status = "pendiente",
                CreatedAt = DateTime.UtcNow
            };
            Orders.Add(order);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }

        [HttpGet]
        public ActionResult<IEnumerable<ServiceOrderDto>> GetAll() => Orders;

        [HttpGet("{id:int}")]
        public ActionResult<ServiceOrderDto> GetById(int id)
        {
            var order = Orders.FirstOrDefault(o => o.Id == id);
            if (order == null) return NotFound(new { error = "Orden no encontrada" });
            return order;
        }

        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] ServiceOrderUpdateDto dto)
        {
            var order = Orders.FirstOrDefault(o => o.Id == id);
            if (order == null) return NotFound(new { error = "Orden no encontrada" });
            if (dto.Description != null) order.Description = dto.Description;
            if (dto.AssignedTo != null) order.AssignedTo = dto.AssignedTo;
            if (dto.Status != null)
            {
                var valid = new[] { "pendiente", "en_curso", "completado" };
                if (!valid.Contains(dto.Status)) return BadRequest(new { error = "Estado inválido" });
                order.Status = dto.Status;
            }
            order.UpdatedAt = DateTime.UtcNow;
            return Ok(order);
        }
    }
}
