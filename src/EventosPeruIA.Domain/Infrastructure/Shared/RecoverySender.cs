using System.Net;
using System.Net.Mail;
using EventosPeruIA.CORE.Core.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using EventosPeruIA.CORE.Core.Interfaces;
namespace EventosPeruIA.Api.Services;
public sealed class RecoverySender(IHostEnvironment env, IConfiguration config) : IRecoverySender
{
    public async Task Send(RecuperacionMessage message, CancellationToken ct)
    {
        var origin = config["Recovery:PublicUrl"] ?? "http://localhost:5080";
        var link = $"{origin.TrimEnd('/')}/?correo={Uri.EscapeDataString(message.Correo)}&token={Uri.EscapeDataString(message.Token)}";
        var body = $"Solicitaste restablecer tu contraseña de EventosPeruIA. Enlace válido por 30 minutos y un solo uso:\n{link}\nSi no lo solicitaste, ignora este mensaje.";
        if (env.IsDevelopment())
        {
            var dir = Path.Combine(env.ContentRootPath, "App_Data", "Outbox"); Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, Guid.NewGuid() + ".txt");
            await File.WriteAllTextAsync(path, $"Para: {message.Correo}\n{body}", ct);
            if (!OperatingSystem.IsWindows()) File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite);
            return;
        }
        if (string.IsNullOrWhiteSpace(config["Smtp:Host"])) throw new InvalidOperationException("Configura SMTP antes de usar recuperación en producción.");
        using var smtp = new SmtpClient(config["Smtp:Host"], config.GetValue("Smtp:Port", 587)) { EnableSsl = true, Credentials = new NetworkCredential(config["Smtp:User"], config["Smtp:Password"]) };
        using var mail = new MailMessage(config["Smtp:From"]!, message.Correo, "Recuperar acceso — EventosPeruIA", body);
        await smtp.SendMailAsync(mail, ct);
    }
}
