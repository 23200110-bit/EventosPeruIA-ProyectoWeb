using System;
using System.Collections.Generic;

namespace EventosPeruIA.Domain.Core.Entities;

public partial class vw_VentasPorEvento
{
    public int EventoId { get; set; }

    public int OrganizadorId { get; set; }

    public string NombreEvento { get; set; } = null!;

    public int? EntradasVendidas { get; set; }

    public decimal? IngresosConfirmados { get; set; }

    public int? EntradasAnuladas { get; set; }

    public decimal? MontoPendienteReembolso { get; set; }
}
