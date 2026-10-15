using System.Net;
using System.Net.Http.Json;
using EventosPeruIA.Domain.Core.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;
public sealed class SqlServerFactAttribute : FactAttribute
{
    public SqlServerFactAttribute() { if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("EVENTOS_SQL_CONNECTION"))) Skip = "Sólo CI: SQL Server desechable creado desde el SQL original."; }
}
public sealed class SqlServerFactory : WebApplicationFactory<Program>
{
    public MailBox Mail { get; } = new();
    protected override void ConfigureWebHost(IWebHostBuilder b)
    {
        b.UseEnvironment("Development"); b.UseSetting("Jwt:Key", "Synthetic-test-only-signing-key-64-characters-01234567890123456789");
        b.ConfigureServices(s => { s.RemoveAll<IRecoverySender>(); s.AddSingleton<IRecoverySender>(Mail); });
    }
}
public class SqlServerTests
{
    [SqlServerFact]
    public async Task Original_SQL_Server_supports_register_login_and_atomic_password_reset()
    {
        using var f = new SqlServerFactory(); using var c = f.CreateClient(); var email = $"ci-{Guid.NewGuid():N}@example.test";
        var registered = await c.PostAsJsonAsync("/api/auth/register", new { nombres = "CI", apellidos = "Prueba", correo = email, contrasena = "Original123!", confirmarContrasena = "Original123!", interesesIds = new[] { 1 } }); Assert.Equal(HttpStatusCode.Created, registered.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await c.PostAsJsonAsync("/api/auth/login", new { correo = email, contrasena = "Original123!" })).StatusCode);
        Assert.Equal(HttpStatusCode.Accepted, (await c.PostAsJsonAsync("/api/auth/forgot-password", new { correo = email })).StatusCode);
        var token = Assert.Single(f.Mail.Messages).Token; var reset = new { correo = email, token, contrasena = "NuevaClave123!", confirmarContrasena = "NuevaClave123!" };
        Assert.Equal(HttpStatusCode.OK, (await c.PostAsJsonAsync("/api/auth/reset-password", reset)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await c.PostAsJsonAsync("/api/auth/reset-password", reset)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await c.PostAsJsonAsync("/api/auth/login", new { correo = email, contrasena = "Original123!" })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await c.PostAsJsonAsync("/api/auth/login", new { correo = email, contrasena = "NuevaClave123!" })).StatusCode);
    }
}
