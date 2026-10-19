using System;
using System.Collections.Generic;

namespace EventosPeruIA.CORE.Core.Entities;

public partial class Resenas
{
    public int ResenaId { get; set; }

    public Guid EntradaId { get; set; }

    public int EventoId { get; set; }

    public int UsuarioId { get; set; }

    public byte Calificacion { get; set; }

    public string? Comentario { get; set; }

    public string Estado { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public virtual Entradas Entrada { get; set; } = null!;

    public virtual Eventos Evento { get; set; } = null!;

    public virtual Usuarios Usuario { get; set; } = null!;
}
