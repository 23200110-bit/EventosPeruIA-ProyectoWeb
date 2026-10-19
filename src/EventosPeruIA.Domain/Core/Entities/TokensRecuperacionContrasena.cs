using System;
using System.Collections.Generic;

namespace EventosPeruIA.CORE.Core.Entities;

public partial class TokensRecuperacionContrasena
{
    public Guid TokenId { get; set; }

    public int UsuarioId { get; set; }

    public string TokenHash { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public DateTime FechaExpiracion { get; set; }

    public bool Utilizado { get; set; }

    public DateTime? FechaUso { get; set; }

    public virtual Usuarios Usuario { get; set; } = null!;
}
