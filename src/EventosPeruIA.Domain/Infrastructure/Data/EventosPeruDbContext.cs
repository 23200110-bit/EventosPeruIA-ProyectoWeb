using Microsoft.EntityFrameworkCore;
using EventosPeruIA.Domain.Core.Entities;
namespace EventosPeruIA.Domain.Infrastructure.Data;
// Modelo del esquema SQL recibido. El script de scaffolding regenera estas clases
// desde SQL Server; no se usa EnsureCreated ni migraciones para crear la base real.
public partial class EventosPeruDbContext(DbContextOptions<EventosPeruDbContext> options) : DbContext(options)
{
 public virtual DbSet<Usuarios> Usuarios {get;set;}
 public virtual DbSet<Categorias> Categorias {get;set;}
 public virtual DbSet<TokensRecuperacionContrasena> TokensRecuperacionContrasena {get;set;}
 protected override void OnModelCreating(ModelBuilder m)
 {
  m.Entity<Usuarios>(e=>{e.ToTable("Usuarios");e.HasKey(x=>x.UsuarioId);e.HasIndex(x=>x.Correo).IsUnique().HasDatabaseName("UQ_Usuarios_Correo");e.Property(x=>x.Nombres).HasMaxLength(100);e.Property(x=>x.Apellidos).HasMaxLength(100);e.Property(x=>x.Correo).HasMaxLength(150);e.Property(x=>x.ContrasenaHash).HasMaxLength(255);e.Property(x=>x.Estado).HasMaxLength(20);e.Property(x=>x.CiudadUbicacion).HasMaxLength(100);e.Property(x=>x.Latitud).HasPrecision(9,6);e.Property(x=>x.Longitud).HasPrecision(9,6);});
  m.Entity<Categorias>(e=>{e.ToTable("Categorias");e.HasKey(x=>x.CategoriaId);e.HasIndex(x=>x.Nombre).IsUnique();e.Property(x=>x.Nombre).HasMaxLength(60);e.Property(x=>x.Descripcion).HasMaxLength(255);});
  m.Entity<TokensRecuperacionContrasena>(e=>{e.ToTable("TokensRecuperacionContrasena");e.HasKey(x=>x.TokenId);e.Property(x=>x.TokenHash).HasMaxLength(255);e.HasOne<Usuarios>().WithMany().HasForeignKey(x=>x.UsuarioId).OnDelete(DeleteBehavior.Cascade);});
 }
}
