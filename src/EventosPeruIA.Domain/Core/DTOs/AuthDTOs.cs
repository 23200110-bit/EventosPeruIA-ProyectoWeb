using System.ComponentModel.DataAnnotations;
namespace EventosPeruIA.Domain.Core.DTOs;
public sealed class RegisterRequestDTO
{
 [Required, StringLength(100, MinimumLength=1)] public string Nombres {get;set;} = "";
 [Required, StringLength(100, MinimumLength=1)] public string Apellidos {get;set;} = "";
 [Required, EmailAddress, StringLength(150)] public string Correo {get;set;} = "";
 [Required, StringLength(128, MinimumLength=8)] public string Contrasena {get;set;} = "";
 [Required, Compare(nameof(Contrasena))] public string ConfirmarContrasena {get;set;} = "";
 [MaxLength(50)] public int[] InteresesIds {get;set;} = [];
}
public sealed class LoginRequestDTO
{
 [Required, EmailAddress, StringLength(150)] public string Correo {get;set;} = "";
 [Required, StringLength(128)] public string Contrasena {get;set;} = "";
}
public sealed class ForgotPasswordRequestDTO
{
 [Required, EmailAddress, StringLength(150)] public string Correo {get;set;} = "";
}
public sealed class ResetPasswordRequestDTO
{
 [Required, EmailAddress, StringLength(150)] public string Correo {get;set;} = "";
 [Required, StringLength(128, MinimumLength=32)] public string Token {get;set;} = "";
 [Required, StringLength(128, MinimumLength=8)] public string Contrasena {get;set;} = "";
 [Required, Compare(nameof(Contrasena))] public string ConfirmarContrasena {get;set;} = "";
}
public sealed record UsuarioResponseDTO(int UsuarioId,string Nombres,string Apellidos,string Correo,bool EsAdmin);
public sealed record RecuperacionMessage(string Correo,string Token);
