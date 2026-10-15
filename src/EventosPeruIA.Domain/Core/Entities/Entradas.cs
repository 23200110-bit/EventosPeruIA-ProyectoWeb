using System;
using System.Collections.Generic;

namespace EventosPeruIA.Domain.Core.Entities;

public partial class Entradas
{
    public Guid EntradaId { get; set; }

    public int CompraId { get; set; }

    public int EventoId { get; set; }

    public int CompradorId { get; set; }

    public decimal PrecioPagado { get; set; }

    public string Estado { get; set; } = null!;

    public string? EstadoReembolso { get; set; }

    public DateTime FechaEmision { get; set; }

    public DateTime? FechaUso { get; set; }

    public DateTime? FechaAnulacion { get; set; }

    public string? CodigoCorto { get; set; }

    public virtual CheckIns? CheckIns { get; set; }

    public virtual Compras Compra { get; set; } = null!;

    public virtual Usuarios Comprador { get; set; } = null!;

    public virtual Eventos Evento { get; set; } = null!;

    public virtual ICollection<Resenas> Resenas { get; set; } = new List<Resenas>();
}
