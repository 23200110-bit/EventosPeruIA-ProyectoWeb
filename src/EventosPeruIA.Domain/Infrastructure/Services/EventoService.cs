using EventosPeruIA.Domain.Core.DTOs;
using EventosPeruIA.Domain.Core.Entities;
using EventosPeruIA.Domain.Core.Interfaces;

namespace EventosPeruIA.Domain.Core.Services
{
    public class EventoService : IEventoService
    {
        private readonly IEventoRepository _eventoRepository;

        public EventoService(IEventoRepository eventoRepository)
        {
            _eventoRepository = eventoRepository;
        }

        public async Task<EventoDTO?> GetEventoById(int id)
        {
            var evento = await _eventoRepository.GetEventoById(id);
            if (evento == null) return null;

            var dto = new EventoDTO();
            dto.EventoId = evento.EventoId;
            dto.OrganizadorId = evento.OrganizadorId;
            dto.CategoriaId = evento.CategoriaId;
            dto.Nombre = evento.Nombre;
            dto.Descripcion = evento.Descripcion;
            dto.FechaInicio = evento.FechaInicio;
            dto.FechaFin = evento.FechaFin;
            dto.Direccion = evento.Direccion;
            dto.Latitud = evento.Latitud;
            dto.Longitud = evento.Longitud;
            dto.Aforo = evento.Aforo;
            dto.Precio = evento.Precio;
            dto.Estado = evento.Estado;

            return dto;
        }

        public async Task<int> CreateEvento(int userId, EventoCreateDTO eventoCreateDTO)
        {
            var fechaInicio = eventoCreateDTO.FechaInicio.ToUniversalTime();
            var fechaFin = eventoCreateDTO.FechaFin?.ToUniversalTime();

            if (fechaInicio <= DateTime.UtcNow) return 0;
            if (fechaFin.HasValue && fechaFin.Value <= fechaInicio) return 0;
            if (!await _eventoRepository.CategoriaExists(eventoCreateDTO.CategoriaId)) return 0;

            var evento = new Eventos();
            evento.OrganizadorId = userId;
            evento.CategoriaId = eventoCreateDTO.CategoriaId;
            evento.Nombre = eventoCreateDTO.Nombre.Trim();
            evento.Descripcion = eventoCreateDTO.Descripcion.Trim();
            evento.FechaInicio = fechaInicio;
            evento.FechaFin = fechaFin;
            evento.Direccion = eventoCreateDTO.Direccion.Trim();
            evento.Latitud = eventoCreateDTO.Latitud;
            evento.Longitud = eventoCreateDTO.Longitud;
            evento.Aforo = eventoCreateDTO.Aforo;
            evento.Precio = eventoCreateDTO.Precio;
            evento.Estado = eventoCreateDTO.Publicar ? "Publicado" : "Borrador";
            evento.FechaCreacion = DateTime.UtcNow;
            evento.FechaPublicacion = eventoCreateDTO.Publicar ? DateTime.UtcNow : null;

            return await _eventoRepository.CreateEvento(evento);
        }

        public async Task<bool> UpdateEvento(EventoUpdateDTO eventoUpdateDTO)
        {
            var existing = await _eventoRepository.GetEventoById(eventoUpdateDTO.EventoId);
            if (existing == null) return false;
            if (existing.Estado != "Borrador" && existing.Estado != "Publicado") return false;

            var fechaInicio = eventoUpdateDTO.FechaInicio.ToUniversalTime();
            var fechaFin = eventoUpdateDTO.FechaFin?.ToUniversalTime();

            if (fechaInicio <= DateTime.UtcNow) return false;
            if (fechaFin.HasValue && fechaFin.Value <= fechaInicio) return false;
            if (!await _eventoRepository.CategoriaExists(eventoUpdateDTO.CategoriaId)) return false;

            var entradasActivas = await _eventoRepository.CountEntradasActivas(eventoUpdateDTO.EventoId);
            if (eventoUpdateDTO.Aforo < entradasActivas) return false;

            var publicar = eventoUpdateDTO.Publicar && existing.Estado == "Borrador";

            var evento = new Eventos();
            evento.EventoId = eventoUpdateDTO.EventoId;
            evento.CategoriaId = eventoUpdateDTO.CategoriaId;
            evento.Nombre = eventoUpdateDTO.Nombre.Trim();
            evento.Descripcion = eventoUpdateDTO.Descripcion.Trim();
            evento.FechaInicio = fechaInicio;
            evento.FechaFin = fechaFin;
            evento.Direccion = eventoUpdateDTO.Direccion.Trim();
            evento.Latitud = eventoUpdateDTO.Latitud;
            evento.Longitud = eventoUpdateDTO.Longitud;
            evento.Aforo = eventoUpdateDTO.Aforo;
            evento.Precio = eventoUpdateDTO.Precio;
            evento.Estado = publicar ? "Publicado" : existing.Estado;
            evento.FechaPublicacion = publicar ? DateTime.UtcNow : existing.FechaPublicacion;

            return await _eventoRepository.UpdateEvento(evento);
        }

        public async Task<bool> CancelEvento(int id, int userId, EventoCancelDTO eventoCancelDTO)
        {
            var existing = await _eventoRepository.GetEventoById(id);
            if (existing == null) return false;
            if (existing.Estado != "Borrador" && existing.Estado != "Publicado") return false;
            if (existing.FechaInicio <= DateTime.UtcNow) return false;
            if (!await _eventoRepository.MotivoCancelacionExists(eventoCancelDTO.MotivoCancelacionId)) return false;

            return await _eventoRepository.CancelEvento(
                id,
                userId,
                eventoCancelDTO.MotivoCancelacionId,
                eventoCancelDTO.DescripcionAdicional.Trim());
        }
    }
}