using System;
using System.Collections.Generic;

namespace EventosPeruIA.Domain.Core.Entities;

public partial class ConversacionesChatbot
{
    public int ConversacionId { get; set; }

    public int UsuarioId { get; set; }

    public DateTime FechaInicio { get; set; }

    public virtual ICollection<MensajesChatbot> MensajesChatbot { get; set; } = new List<MensajesChatbot>();

    public virtual Usuarios Usuario { get; set; } = null!;
}
