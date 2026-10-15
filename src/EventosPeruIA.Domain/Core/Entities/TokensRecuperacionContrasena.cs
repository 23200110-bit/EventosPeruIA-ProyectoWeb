namespace EventosPeruIA.Domain.Core.Entities;
public partial class TokensRecuperacionContrasena
{
 public Guid TokenId {get;set;}
 public int UsuarioId {get;set;}
 public string TokenHash {get;set;} = null!;
 public DateTime FechaCreacion {get;set;}
 public DateTime FechaExpiracion {get;set;}
 public bool Utilizado {get;set;}
 public DateTime? FechaUso {get;set;}
}
