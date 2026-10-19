using System;
using System.Collections.Generic;

namespace EventosPeruIA.CORE.Core.Entities;

public partial class Notificaciones
{
    public long NotificacionId { get; set; }

    public int UsuarioId { get; set; }

    public int? EventoId { get; set; }

    public string Tipo { get; set; } = null!;

    public string Mensaje { get; set; } = null!;

    public bool Leida { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaLectura { get; set; }

    public virtual Eventos? Evento { get; set; }

    public virtual Usuarios Usuario { get; set; } = null!;
}
