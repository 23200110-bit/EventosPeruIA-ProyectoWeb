using System;
using System.Collections.Generic;

namespace EventosPeruIA.Domain.Core.Entities;

public partial class MensajesChatbot
{
    public long MensajeId { get; set; }

    public int ConversacionId { get; set; }

    public string Emisor { get; set; } = null!;

    public string Contenido { get; set; } = null!;

    public DateTime FechaHora { get; set; }

    public virtual ConversacionesChatbot Conversacion { get; set; } = null!;
}
