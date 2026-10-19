using System;
using System.Collections.Generic;

namespace EventosPeruIA.CORE.Core.Entities;

public partial class AccionesModeracion
{
    public long AccionId { get; set; }

    public int AdministradorId { get; set; }

    public string TipoAccion { get; set; } = null!;

    public string EntidadTipo { get; set; } = null!;

    public int EntidadId { get; set; }
    
    public string? Motivo { get; set; }

    public DateTime FechaHora { get; set; }

    public virtual Usuarios Administrador { get; set; } = null!;
}
