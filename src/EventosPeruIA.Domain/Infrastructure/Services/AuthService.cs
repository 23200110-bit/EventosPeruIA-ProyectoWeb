using System.Security.Cryptography;
using System.Text;
using EventosPeruIA.Domain.Core.DTOs;
using EventosPeruIA.Domain.Core.Entities;
using EventosPeruIA.Domain.Core.Interfaces;
using EventosPeruIA.Domain.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace EventosPeruIA.Domain.Infrastructure.Services;
public sealed class AuthService(EventosPeruDbContext db,IPasswordHasher<Usuarios> hasher,IRecoverySender sender,TimeProvider clock):IAuthService
{
 static string Email(string value)=>value.Trim().ToLowerInvariant();
 static UsuarioResponseDTO View(Usuarios u)=>new(u.UsuarioId,u.Nombres,u.Apellidos,u.Correo,u.EsAdministrador);
 static string Hash(string token)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
 DateTime Now()=>clock.GetUtcNow().UtcDateTime;
 public async Task<UsuarioResponseDTO> Register(RegisterRequestDTO dto,CancellationToken ct)
 {
  var correo=Email(dto.Correo);
  if(await db.Usuarios.AnyAsync(x=>x.Correo==correo,ct))throw new AuthException(409,"El correo ya está registrado.");
  var interests=(dto.InteresesIds??[]).Distinct().ToArray();
  if(await db.Categorias.CountAsync(x=>interests.Contains(x.CategoriaId)&&x.Activo,ct)!=interests.Length)throw new AuthException(400,"Hay intereses inexistentes o inactivos.");
  var u=new Usuarios {Nombres=dto.Nombres.Trim(),Apellidos=dto.Apellidos.Trim(),Correo=correo,EsAdministrador=false,Estado="Activo",FechaRegistro=Now()};
  if(u.Nombres.Length==0||u.Apellidos.Length==0)throw new AuthException(400,"Nombre y apellidos son obligatorios.");
  u.ContrasenaHash=hasher.HashPassword(u,dto.Contrasena);
  await using var tx=await db.Database.BeginTransactionAsync(ct);
  try
  {
   db.Usuarios.Add(u);await db.SaveChangesAsync(ct);
   foreach(var id in interests)await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO UsuarioIntereses (UsuarioId,CategoriaId) VALUES ({u.UsuarioId},{id})",ct);
   await tx.CommitAsync(ct);return View(u);
  }
  catch(DbUpdateException ex) when(ex.InnerException is Microsoft.Data.SqlClient.SqlException sql && sql.Number is 2601 or 2627)
  {throw new AuthException(409,"El correo ya está registrado.");}
 }
 public async Task<UsuarioResponseDTO> Login(LoginRequestDTO dto,CancellationToken ct)
 {
  var correo=Email(dto.Correo);var u=await db.Usuarios.SingleOrDefaultAsync(x=>x.Correo==correo,ct);
  if(u is null||u.Estado!="Activo")throw new AuthException(401,"Correo o contraseña incorrectos.");
  var result=hasher.VerifyHashedPassword(u,u.ContrasenaHash,dto.Contrasena);
  if(result==PasswordVerificationResult.Failed)throw new AuthException(401,"Correo o contraseña incorrectos.");
  if(result==PasswordVerificationResult.SuccessRehashNeeded){u.ContrasenaHash=hasher.HashPassword(u,dto.Contrasena);await db.SaveChangesAsync(ct);}
  return View(u);
 }
 public async Task Forgot(ForgotPasswordRequestDTO dto,CancellationToken ct)
 {
  var correo=Email(dto.Correo);var u=await db.Usuarios.SingleOrDefaultAsync(x=>x.Correo==correo&&x.Estado=="Activo",ct);
  if(u is null)return;
  var token=Convert.ToHexString(RandomNumberGenerator.GetBytes(32));var now=Now();
  db.TokensRecuperacionContrasena.Add(new(){TokenId=Guid.NewGuid(),UsuarioId=u.UsuarioId,TokenHash=Hash(token),FechaCreacion=now,FechaExpiracion=now.AddMinutes(30),Utilizado=false});
  await db.SaveChangesAsync(ct);await sender.Send(new(correo,token),ct);
 }
 public async Task Reset(ResetPasswordRequestDTO dto,CancellationToken ct)
 {
  var email=Email(dto.Correo);var hash=Hash(dto.Token);var now=Now();
  var u=await db.Usuarios.SingleOrDefaultAsync(x=>x.Correo==email&&x.Estado=="Activo",ct);
  if(u is null)throw new AuthException(400,"Recuperación inválida o vencida.");
  await using var tx=await db.Database.BeginTransactionAsync(ct);
  // UPDATE condicional atómico: dos solicitudes concurrentes no consumen el mismo token.
  var consumed=await db.TokensRecuperacionContrasena.Where(x=>x.UsuarioId==u.UsuarioId&&x.TokenHash==hash&&!x.Utilizado&&x.FechaExpiracion>now)
    .ExecuteUpdateAsync(s=>s.SetProperty(x=>x.Utilizado,true).SetProperty(x=>x.FechaUso,now),ct);
  if(consumed!=1)throw new AuthException(400,"Recuperación inválida o vencida.");
  u.ContrasenaHash=hasher.HashPassword(u,dto.Contrasena);u.FechaActualizacion=now;
  await db.SaveChangesAsync(ct);
  await db.TokensRecuperacionContrasena.Where(x=>x.UsuarioId==u.UsuarioId&&!x.Utilizado).ExecuteUpdateAsync(s=>s.SetProperty(x=>x.Utilizado,true).SetProperty(x=>x.FechaUso,now),ct);
  await tx.CommitAsync(ct);
 }
}
