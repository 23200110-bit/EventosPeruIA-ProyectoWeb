using System;
using System.Collections.Generic;

namespace EventosPeruIA.Domain.Core.Entities;

public partial class Usuarios
{
    public int UsuarioId { get; set; }

    public string Nombres { get; set; } = null!;

    public string Apellidos { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string ContrasenaHash { get; set; } = null!;

    public bool EsAdministrador { get; set; }

    public string Estado { get; set; } = null!;

    public string? CiudadUbicacion { get; set; }

    public decimal? Latitud { get; set; }

    public decimal? Longitud { get; set; }

    public DateTime FechaRegistro { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public virtual ICollection<AccionesModeracion> AccionesModeracion { get; set; } = new List<AccionesModeracion>();

    public virtual ICollection<CheckIns> CheckIns { get; set; } = new List<CheckIns>();

    public virtual ICollection<Compras> Compras { get; set; } = new List<Compras>();

    public virtual ICollection<ConversacionesChatbot> ConversacionesChatbot { get; set; } = new List<ConversacionesChatbot>();

    public virtual ICollection<Entradas> Entradas { get; set; } = new List<Entradas>();

    public virtual ICollection<Eventos> Eventos { get; set; } = new List<Eventos>();

    public virtual ICollection<EventosCancelacion> EventosCancelacion { get; set; } = new List<EventosCancelacion>();

    public virtual ICollection<Notificaciones> Notificaciones { get; set; } = new List<Notificaciones>();

    public virtual ICollection<Resenas> Resenas { get; set; } = new List<Resenas>();

    public virtual ICollection<TokensRecuperacionContrasena> TokensRecuperacionContrasena { get; set; } = new List<TokensRecuperacionContrasena>();

    public virtual ICollection<UsuarioEventoInteraccion> UsuarioEventoInteraccion { get; set; } = new List<UsuarioEventoInteraccion>();

    public virtual ICollection<Categorias> Categoria { get; set; } = new List<Categorias>();
}
