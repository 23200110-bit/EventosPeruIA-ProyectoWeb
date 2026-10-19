using System;
using System.Collections.Generic;

namespace EventosPeruIA.CORE.Core.Entities;

public partial class Categorias
{
    public int CategoriaId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<Eventos> Eventos { get; set; } = new List<Eventos>();

    public virtual ICollection<Usuarios> Usuario { get; set; } = new List<Usuarios>();
}
