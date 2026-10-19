using System;
using System.Collections.Generic;

namespace EventosPeruIA.CORE.Core.Entities;

public partial class Compras
{
    public int CompraId { get; set; }

    public int CompradorId { get; set; }

    public int EventoId { get; set; }

    public int CantidadEntradas { get; set; }

    public decimal MontoTotal { get; set; }

    public string? CodigoTransaccion { get; set; }

    public string Estado { get; set; } = null!;

    public DateTime FechaCompra { get; set; }

    public virtual Usuarios Comprador { get; set; } = null!;

    public virtual ICollection<Entradas> Entradas { get; set; } = new List<Entradas>();

    public virtual Eventos Evento { get; set; } = null!;
}
