using System;
using System.Collections.Generic;

namespace EventosPeruIA.CORE.Core.Entities;

public partial class MotivosCancelacion
{
    public int MotivoCancelacionId { get; set; }

    public string Codigo { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public virtual ICollection<EventosCancelacion> EventosCancelacion { get; set; } = new List<EventosCancelacion>();
}
