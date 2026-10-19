using System;
using System.Collections.Generic;

namespace EventosPeruIA.CORE.Core.Entities;

public partial class CheckIns
{
    public int CheckInId { get; set; }

    public Guid EntradaId { get; set; }

    public int ValidadoPorId { get; set; }

    public string MetodoIngreso { get; set; } = null!;

    public string Resultado { get; set; } = null!;

    public string? MotivoRechazo { get; set; }

    public DateTime FechaHora { get; set; }

    public virtual Entradas Entrada { get; set; } = null!;

    public virtual Usuarios ValidadoPor { get; set; } = null!;
}
