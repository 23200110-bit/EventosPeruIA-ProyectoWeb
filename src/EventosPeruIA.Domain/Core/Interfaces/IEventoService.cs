using EventosPeruIA.CORE.Core.DTOs;

namespace EventosPeruIA.CORE.Core.Interfaces
{
    public interface IEventoService
    {
        Task<bool> CancelEvento(int id, int userId, EventoCancelDTO eventoCancelDTO);
        Task<int> CreateEvento(int userId, EventoCreateDTO eventoCreateDTO);
        Task<EventoDTO?> GetEventoById(int id);
        Task<bool> UpdateEvento(EventoUpdateDTO eventoUpdateDTO);
    }
}