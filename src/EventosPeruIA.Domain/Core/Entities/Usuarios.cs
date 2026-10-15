namespace EventosPeruIA.Domain.Core.Entities;
public partial class Usuarios
{
 public int UsuarioId {get;set;}
 public string Nombres {get;set;} = null!;
 public string Apellidos {get;set;} = null!;
 public string Correo {get;set;} = null!;
 public string ContrasenaHash {get;set;} = null!;
 public bool EsAdministrador {get;set;}
 public string Estado {get;set;} = "Activo";
 public string? CiudadUbicacion {get;set;}
 public decimal? Latitud {get;set;}
 public decimal? Longitud {get;set;}
 public DateTime FechaRegistro {get;set;}
 public DateTime? FechaActualizacion {get;set;}
}
