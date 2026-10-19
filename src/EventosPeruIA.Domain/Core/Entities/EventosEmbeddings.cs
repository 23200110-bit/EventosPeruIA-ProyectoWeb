using System;
using System.Collections.Generic;

namespace EventosPeruIA.CORE.Core.Entities;

public partial class EventosEmbeddings
{
    public int EmbeddingId { get; set; }

    public int EventoId { get; set; }

    public string ContenidoTexto { get; set; } = null!;

    public byte[] VectorEmbedding { get; set; } = null!;

    public string? EmbeddingJson { get; set; }

    public string ModeloUsado { get; set; } = null!;

    public int Version { get; set; }

    public bool Vigente { get; set; }

    public DateTime FechaGeneracion { get; set; }

    public virtual Eventos Evento { get; set; } = null!;
}
