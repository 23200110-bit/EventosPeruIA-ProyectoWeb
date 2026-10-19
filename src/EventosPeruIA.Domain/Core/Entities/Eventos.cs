using System;
using System.Collections.Generic;

namespace EventosPeruIA.CORE.Core.Entities;

public partial class Eventos
{
    public int EventoId { get; set; }

    public int OrganizadorId { get; set; }

    public int CategoriaId { get; set; }

    public string Nombre { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public DateTime FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    public string Direccion { get; set; } = null!;

    public decimal? Latitud { get; set; }

    public decimal? Longitud { get; set; }

    public int Aforo { get; set; }

    public decimal Precio { get; set; }

    public string Estado { get; set; } = null!;

    public string EstadoModeracion { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaPublicacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public byte[] VersionFila { get; set; } = null!;

    public virtual Categorias Categoria { get; set; } = null!;

    public virtual ICollection<Compras> Compras { get; set; } = new List<Compras>();

    public virtual ICollection<Entradas> Entradas { get; set; } = new List<Entradas>();

    public virtual EventosCancelacion? EventosCancelacion { get; set; }

    public virtual ICollection<EventosEmbeddings> EventosEmbeddings { get; set; } = new List<EventosEmbeddings>();

    public virtual ICollection<Notificaciones> Notificaciones { get; set; } = new List<Notificaciones>();

    public virtual Usuarios Organizador { get; set; } = null!;

    public virtual ICollection<Resenas> Resenas { get; set; } = new List<Resenas>();

    public virtual ICollection<UsuarioEventoInteraccion> UsuarioEventoInteraccion { get; set; } = new List<UsuarioEventoInteraccion>();
}
