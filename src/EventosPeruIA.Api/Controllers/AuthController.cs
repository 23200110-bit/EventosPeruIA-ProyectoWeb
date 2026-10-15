using System.Security.Claims;
using EventosPeruIA.Domain.Core.DTOs;
using EventosPeruIA.Domain.Core.Interfaces;
using EventosPeruIA.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
namespace EventosPeruIA.Api.Controllers;
[ApiController,Route("api/auth"),EnableRateLimiting("auth")]
public sealed class AuthController(IAuthService auth,JwtTokenGenerator jwt):ControllerBase
{
 [HttpPost("register")] public async Task<IActionResult> Register(RegisterRequestDTO dto,CancellationToken ct)=>StatusCode(201,new {mensaje="Cuenta creada correctamente.",usuario=await auth.Register(dto,ct)});
 [HttpPost("login")] public async Task<IActionResult> Login(LoginRequestDTO dto,CancellationToken ct)=>Ok(jwt.Generate(await auth.Login(dto,ct)));
 [HttpPost("forgot-password")] public async Task<IActionResult> Forgot(ForgotPasswordRequestDTO dto,CancellationToken ct){await auth.Forgot(dto,ct);return Accepted(new {mensaje="Si el correo está registrado y activo, recibirás instrucciones para recuperar el acceso."});}
 [HttpPost("reset-password")] public async Task<IActionResult> Reset(ResetPasswordRequestDTO dto,CancellationToken ct){await auth.Reset(dto,ct);return Ok(new {mensaje="Contraseña actualizada. Ingresa con tu nueva contraseña."});}
 [Authorize,HttpGet("me")] public IActionResult Me()=>Ok(new {usuarioId=User.FindFirstValue("UsuarioId"),correo=User.FindFirstValue("Correo"),esAdmin=User.FindFirstValue("EsAdmin")});
}
