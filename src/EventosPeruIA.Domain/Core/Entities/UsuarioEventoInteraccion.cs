using System;
using System.Collections.Generic;

namespace EventosPeruIA.Domain.Core.Entities;

public partial class UsuarioEventoInteraccion
{
    public long InteraccionId { get; set; }

    public int UsuarioId { get; set; }

    public int EventoId { get; set; }

    public string TipoInteraccion { get; set; } = null!;

    public DateTime FechaHora { get; set; }

    public virtual Eventos Evento { get; set; } = null!;

    public virtual Usuarios Usuario { get; set; } = null!;
}
