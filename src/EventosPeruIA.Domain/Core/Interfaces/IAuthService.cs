using EventosPeruIA.CORE.Core.DTOs;
namespace EventosPeruIA.CORE.Core.Interfaces;
public interface IAuthService
{
    Task<UsuarioResponseDTO> Register(RegisterRequestDTO dto, CancellationToken ct);
    Task<UsuarioResponseDTO> Login(LoginRequestDTO dto, CancellationToken ct);
    Task Forgot(ForgotPasswordRequestDTO dto, CancellationToken ct);
    Task Reset(ResetPasswordRequestDTO dto, CancellationToken ct);
}
public interface IRecoverySender { Task Send(RecuperacionMessage message, CancellationToken ct); }
public sealed class AuthException(int status, string message) : Exception(message) { public int Status { get; } = status; }
