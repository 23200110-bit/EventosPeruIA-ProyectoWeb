using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EventosPeruIA.Domain.Core.DTOs;
using Microsoft.IdentityModel.Tokens;
namespace EventosPeruIA.Api.Security;
public sealed class JwtTokenGenerator(IConfiguration config, TimeProvider clock)
{
    public object Generate(UsuarioResponseDTO u)
    {
        var expires = clock.GetUtcNow().UtcDateTime.AddMinutes(60);
        var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, u.UsuarioId.ToString()), new Claim(JwtRegisteredClaimNames.Email, u.Correo), new Claim("UsuarioId", u.UsuarioId.ToString()), new Claim("Correo", u.Correo), new Claim("EsAdmin", u.EsAdmin.ToString().ToLowerInvariant()), new Claim(ClaimTypes.Role, u.EsAdmin ? "Admin" : "Usuario"), new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) };
        var jwt = new JwtSecurityToken(config["Jwt:Issuer"], config["Jwt:Audience"], claims, clock.GetUtcNow().UtcDateTime, expires, new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!)), SecurityAlgorithms.HmacSha256));
        return new { token = new JwtSecurityTokenHandler().WriteToken(jwt), expiraEn = expires, usuario = u };
    }
}
