using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using EventosPeruIA.Domain.Core.DTOs;
using EventosPeruIA.Domain.Core.Entities;
using EventosPeruIA.Domain.Core.Interfaces;
using EventosPeruIA.Domain.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

public sealed class MailBox : IRecoverySender
{
    public List<RecuperacionMessage> Messages { get; } = [];
    public Task Send(RecuperacionMessage m, CancellationToken ct) { Messages.Add(m); return Task.CompletedTask; }
}
public sealed class TestClock : TimeProvider
{
    public DateTimeOffset Now = DateTimeOffset.UtcNow;
    public override DateTimeOffset GetUtcNow() => Now;
}
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    readonly SqliteConnection connection = new("Data Source=:memory:");
    public MailBox Mail { get; } = new(); public TestClock Clock { get; } = new();
    public ApiFactory() { connection.Open(); }
    protected override void ConfigureWebHost(IWebHostBuilder b)
    {
        b.UseEnvironment("Development"); b.UseSetting("Jwt:Key", "Synthetic-test-only-signing-key-64-characters-01234567890123456789");
        b.UseSetting("Jwt:Issuer", "EventosPeruIA"); b.UseSetting("Jwt:Audience", "EventosPeruIA.Cliente");
        b.ConfigureServices(s =>
        {
            s.RemoveAll<DbContextOptions<EventosPeruDbContext>>(); s.RemoveAll<EventosPeruDbContext>();
            s.RemoveAll<Microsoft.EntityFrameworkCore.Infrastructure.IDbContextOptionsConfiguration<EventosPeruDbContext>>();
            s.AddDbContext<EventosPeruDbContext>(o => o.UseSqlite(connection));
            s.RemoveAll<IRecoverySender>(); s.AddSingleton<IRecoverySender>(Mail);
            s.RemoveAll<TimeProvider>(); s.AddSingleton<TimeProvider>(Clock);
        });
    }
    public HttpClient Start()
    {
        var c = CreateClient(); using var scope = Services.CreateScope(); var db = scope.ServiceProvider.GetRequiredService<EventosPeruDbContext>();
        db.Database.ExecuteSqlRaw("""
   CREATE TABLE Usuarios (UsuarioId INTEGER PRIMARY KEY AUTOINCREMENT,Nombres TEXT NOT NULL,Apellidos TEXT NOT NULL,Correo TEXT NOT NULL UNIQUE,ContrasenaHash TEXT NOT NULL,EsAdministrador INTEGER NOT NULL,Estado TEXT NOT NULL,CiudadUbicacion TEXT,Latitud TEXT,Longitud TEXT,FechaRegistro TEXT NOT NULL,FechaActualizacion TEXT);
   CREATE TABLE Categorias (CategoriaId INTEGER PRIMARY KEY,Nombre TEXT NOT NULL,Descripcion TEXT,Activo INTEGER NOT NULL);
   CREATE TABLE UsuarioIntereses (UsuarioId INTEGER NOT NULL REFERENCES Usuarios(UsuarioId),CategoriaId INTEGER NOT NULL REFERENCES Categorias(CategoriaId),PRIMARY KEY(UsuarioId,CategoriaId));
   CREATE TABLE TokensRecuperacionContrasena (TokenId TEXT PRIMARY KEY,UsuarioId INTEGER NOT NULL REFERENCES Usuarios(UsuarioId),TokenHash TEXT NOT NULL,FechaCreacion TEXT NOT NULL,FechaExpiracion TEXT NOT NULL,Utilizado INTEGER NOT NULL,FechaUso TEXT);
   INSERT INTO Categorias VALUES(1,'Música',NULL,1),(2,'Inactiva',NULL,0);
   """); return c;
    }
    public void WithDb(Action<EventosPeruDbContext> action) { using var s = Services.CreateScope(); action(s.ServiceProvider.GetRequiredService<EventosPeruDbContext>()); }
    protected override void Dispose(bool disposing) { base.Dispose(disposing); if (disposing) connection.Dispose(); }
}
public class AuthTests
{
    [Fact]
    public async Task Health_and_Swagger_are_available_after_framework_migration()
    {
        using var f = new ApiFactory(); using var c = f.Start();
        Assert.Equal(HttpStatusCode.OK, (await c.GetAsync("/health")).StatusCode);
        var response = await c.GetAsync("/swagger/v1/swagger.json");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var schema = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(schema.GetProperty("paths").TryGetProperty("/api/auth/login", out _));
        Assert.True(schema.GetProperty("paths").TryGetProperty("/api/auth/reset-password", out _));
    }

    static object Registration(string email = "ana@example.test", int[]? ids = null) => new { nombres = "Ana", apellidos = "Pérez", correo = email, contrasena = "ClaveSegura123!", confirmarContrasena = "ClaveSegura123!", interesesIds = ids ?? [1] };
    static Task<HttpResponseMessage> Login(HttpClient c, string password = "ClaveSegura123!") => c.PostAsJsonAsync("/api/auth/login", new { correo = "ana@example.test", contrasena = password });
    [Fact]
    public async Task Register_hashes_password_assigns_no_admin_and_persists_interests()
    {
        using var f = new ApiFactory(); using var c = f.Start(); var r = await c.PostAsJsonAsync("/api/auth/register", Registration(ids: [1, 1])); Assert.Equal(HttpStatusCode.Created, r.StatusCode);
        f.WithDb(db => { var u = db.Usuarios.Single(); Assert.False(u.EsAdministrador); Assert.NotEqual("ClaveSegura123!", u.ContrasenaHash); Assert.Equal("Activo", u.Estado); });
        Assert.DoesNotContain("contrasenaHash", await r.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.Conflict, (await c.PostAsJsonAsync("/api/auth/register", Registration("ANA@example.test"))).StatusCode);
    }
    [Theory]
    [InlineData(2)]
    [InlineData(999)]
    public async Task Invalid_interest_rejects_without_partial_user(int id)
    {
        using var f = new ApiFactory(); using var c = f.Start(); Assert.Equal(HttpStatusCode.BadRequest, (await c.PostAsJsonAsync("/api/auth/register", Registration(ids: [id]))).StatusCode); f.WithDb(db => Assert.Empty(db.Usuarios));
    }
    [Fact]
    public async Task Password_confirmation_and_email_are_validated()
    {
        using var f = new ApiFactory(); using var c = f.Start(); var r = await c.PostAsJsonAsync("/api/auth/register", new { nombres = "Ana", apellidos = "Pérez", correo = "incorrecto", contrasena = "short", confirmarContrasena = "diferente" }); Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode); f.WithDb(db => Assert.Empty(db.Usuarios));
    }
    [Fact]
    public async Task Login_issues_valid_bearer_and_rejects_wrong_or_blocked()
    {
        using var f = new ApiFactory(); using var c = f.Start(); await c.PostAsJsonAsync("/api/auth/register", Registration()); Assert.Equal(HttpStatusCode.Unauthorized, (await Login(c, "equivocada")).StatusCode);
        var r = await Login(c); Assert.Equal(HttpStatusCode.OK, r.StatusCode); var j = await r.Content.ReadFromJsonAsync<JsonElement>(); c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", j.GetProperty("token").GetString());
        Assert.Equal(HttpStatusCode.OK, (await c.GetAsync("/api/auth/me")).StatusCode); c.DefaultRequestHeaders.Authorization = new("Bearer", "invalido"); Assert.Equal(HttpStatusCode.Unauthorized, (await c.GetAsync("/api/auth/me")).StatusCode);
        f.WithDb(db => { db.Usuarios.Single().Estado = "Bloqueado"; db.SaveChanges(); }); Assert.Equal(HttpStatusCode.Unauthorized, (await Login(c)).StatusCode);
    }
    [Fact]
    public async Task Recovery_is_private_single_use_and_changes_the_password()
    {
        using var f = new ApiFactory(); using var c = f.Start(); await c.PostAsJsonAsync("/api/auth/register", Registration());
        var a = await c.PostAsJsonAsync("/api/auth/forgot-password", new { correo = "ana@example.test" }); var b = await c.PostAsJsonAsync("/api/auth/forgot-password", new { correo = "unknown@example.test" }); Assert.Equal(HttpStatusCode.Accepted, a.StatusCode); Assert.Equal(await a.Content.ReadAsStringAsync(), await b.Content.ReadAsStringAsync()); var token = Assert.Single(f.Mail.Messages).Token;
        f.WithDb(db => Assert.NotEqual(token, db.TokensRecuperacionContrasena.Single().TokenHash));
        var reset = new { correo = "ana@example.test", token, contrasena = "NuevaClave456!", confirmarContrasena = "NuevaClave456!" }; Assert.Equal(HttpStatusCode.OK, (await c.PostAsJsonAsync("/api/auth/reset-password", reset)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await c.PostAsJsonAsync("/api/auth/reset-password", reset)).StatusCode); Assert.Equal(HttpStatusCode.Unauthorized, (await Login(c)).StatusCode); Assert.Equal(HttpStatusCode.OK, (await Login(c, "NuevaClave456!")).StatusCode);
    }
    [Fact]
    public async Task Expired_and_wrong_user_tokens_cannot_change_password()
    {
        using var f = new ApiFactory(); using var c = f.Start(); await c.PostAsJsonAsync("/api/auth/register", Registration()); await c.PostAsJsonAsync("/api/auth/forgot-password", new { correo = "ana@example.test" }); var token = f.Mail.Messages.Single().Token;
        var reset = new { correo = "another@example.test", token, contrasena = "NuevaClave456!", confirmarContrasena = "NuevaClave456!" }; Assert.Equal(HttpStatusCode.BadRequest, (await c.PostAsJsonAsync("/api/auth/reset-password", reset)).StatusCode);
        f.Clock.Now = f.Clock.Now.AddMinutes(31); Assert.Equal(HttpStatusCode.BadRequest, (await c.PostAsJsonAsync("/api/auth/reset-password", new { correo = "ana@example.test", token, contrasena = "NuevaClave456!", confirmarContrasena = "NuevaClave456!" })).StatusCode); Assert.Equal(HttpStatusCode.OK, (await Login(c)).StatusCode);
    }
}
