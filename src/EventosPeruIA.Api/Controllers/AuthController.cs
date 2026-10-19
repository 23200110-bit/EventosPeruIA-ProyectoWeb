using System.Security.Claims;
using EventosPeruIA.CORE.Core.DTOs;
using EventosPeruIA.CORE.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventosPeruIA.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService auth) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDTO dto, CancellationToken ct)
    {
        var result = await auth.Register(dto, ct);
        return StatusCode(201, new { mensaje = "Cuenta creada correctamente.", usuario = result });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDTO dto, CancellationToken ct)
    {
        var token = await auth.Login(dto, ct);
        return Ok(new { token });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> Forgot([FromBody] ForgotPasswordRequestDTO dto, CancellationToken ct)
    {
        await auth.Forgot(dto, ct);
        return Accepted(new { mensaje = "Si el correo está registrado y activo, recibirás instrucciones para recuperar el acceso." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> Reset([FromBody] ResetPasswordRequestDTO dto, CancellationToken ct)
    {
        await auth.Reset(dto, ct);
        return Ok(new { mensaje = "Contraseña actualizada. Ingresa con tu nueva contraseña." });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            usuarioId = User.FindFirstValue("UsuarioId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier),
            correo = User.FindFirstValue("Correo") ?? User.FindFirstValue(ClaimTypes.Email),
            esAdmin = User.FindFirstValue("EsAdmin")
        });
    }
}