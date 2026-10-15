using System;
using System.Collections.Generic;
using EventosPeruIA.Domain.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventosPeruIA.Domain.Infrastructure.Data;

public partial class EventosPeruDbContext : DbContext
{
    public EventosPeruDbContext(DbContextOptions<EventosPeruDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AccionesModeracion> AccionesModeracion { get; set; }

    public virtual DbSet<Categorias> Categorias { get; set; }

    public virtual DbSet<CheckIns> CheckIns { get; set; }

    public virtual DbSet<Compras> Compras { get; set; }

    public virtual DbSet<ConversacionesChatbot> ConversacionesChatbot { get; set; }

    public virtual DbSet<Entradas> Entradas { get; set; }

    public virtual DbSet<Eventos> Eventos { get; set; }

    public virtual DbSet<EventosCancelacion> EventosCancelacion { get; set; }

    public virtual DbSet<EventosEmbeddings> EventosEmbeddings { get; set; }

    public virtual DbSet<MensajesChatbot> MensajesChatbot { get; set; }

    public virtual DbSet<MotivosCancelacion> MotivosCancelacion { get; set; }

    public virtual DbSet<Notificaciones> Notificaciones { get; set; }

    public virtual DbSet<Resenas> Resenas { get; set; }

    public virtual DbSet<TokensRecuperacionContrasena> TokensRecuperacionContrasena { get; set; }

    public virtual DbSet<UsuarioEventoInteraccion> UsuarioEventoInteraccion { get; set; }

    public virtual DbSet<Usuarios> Usuarios { get; set; }

    public virtual DbSet<vw_DisponibilidadEventos> vw_DisponibilidadEventos { get; set; }

    public virtual DbSet<vw_VentasPorEvento> vw_VentasPorEvento { get; set; }

    public virtual DbSet<vw_VentasPorFecha> vw_VentasPorFecha { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccionesModeracion>(entity =>
        {
            entity.HasKey(e => e.AccionId);

            entity.Property(e => e.EntidadTipo)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.FechaHora).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Motivo).HasMaxLength(300);
            entity.Property(e => e.TipoAccion)
                .HasMaxLength(30)
                .IsUnicode(false);

            entity.HasOne(d => d.Administrador).WithMany(p => p.AccionesModeracion)
                .HasForeignKey(d => d.AdministradorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AccionesModeracion_Admin");
        });

        modelBuilder.Entity<Categorias>(entity =>
        {
            entity.HasKey(e => e.CategoriaId);

            entity.HasIndex(e => e.Nombre, "UQ_Categorias_Nombre").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Descripcion).HasMaxLength(255);
            entity.Property(e => e.Nombre).HasMaxLength(60);
        });

        modelBuilder.Entity<CheckIns>(entity =>
        {
            entity.HasKey(e => e.CheckInId);

            entity.HasIndex(e => e.EntradaId, "UX_CheckIns_EntradaAprobada")
                .IsUnique()
                .HasFilter("([Resultado]='Aprobado')");

            entity.Property(e => e.FechaHora).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.MetodoIngreso)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.MotivoRechazo).HasMaxLength(200);
            entity.Property(e => e.Resultado)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Entrada).WithOne(p => p.CheckIns)
                .HasForeignKey<CheckIns>(d => d.EntradaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CheckIns_Entrada");

            entity.HasOne(d => d.ValidadoPor).WithMany(p => p.CheckIns)
                .HasForeignKey(d => d.ValidadoPorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CheckIns_Validador");
        });

        modelBuilder.Entity<Compras>(entity =>
        {
            entity.HasKey(e => e.CompraId);

            entity.Property(e => e.CodigoTransaccion)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Confirmada");
            entity.Property(e => e.FechaCompra).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.MontoTotal).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Comprador).WithMany(p => p.Compras)
                .HasForeignKey(d => d.CompradorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Compras_Comprador");

            entity.HasOne(d => d.Evento).WithMany(p => p.Compras)
                .HasForeignKey(d => d.EventoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Compras_Evento");
        });

        modelBuilder.Entity<ConversacionesChatbot>(entity =>
        {
            entity.HasKey(e => e.ConversacionId);

            entity.Property(e => e.FechaInicio).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Usuario).WithMany(p => p.ConversacionesChatbot)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Conversaciones_Usuario");
        });

        modelBuilder.Entity<Entradas>(entity =>
        {
            entity.HasKey(e => e.EntradaId);

            entity.HasIndex(e => new { e.EventoId, e.Estado }, "IX_Entradas_EventoId_Estado");

            entity.Property(e => e.EntradaId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CodigoCorto)
                .HasMaxLength(12)
                .IsUnicode(false)
                .HasComputedColumnSql("('TCK-'+upper(left(CONVERT([varchar](36),[EntradaId]),(8))))", true);
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Valida");
            entity.Property(e => e.EstadoReembolso)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.FechaEmision).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.PrecioPagado).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Compra).WithMany(p => p.Entradas)
                .HasForeignKey(d => d.CompraId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Entradas_Compra");

            entity.HasOne(d => d.Comprador).WithMany(p => p.Entradas)
                .HasForeignKey(d => d.CompradorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Entradas_Comprador");

            entity.HasOne(d => d.Evento).WithMany(p => p.Entradas)
                .HasForeignKey(d => d.EventoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Entradas_Evento");
        });

        modelBuilder.Entity<Eventos>(entity =>
        {
            entity.HasKey(e => e.EventoId);

            entity.HasIndex(e => e.CategoriaId, "IX_Eventos_CategoriaId");

            entity.HasIndex(e => new { e.Estado, e.FechaInicio }, "IX_Eventos_Estado_FechaInicio");

            entity.Property(e => e.Direccion).HasMaxLength(250);
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Borrador");
            entity.Property(e => e.EstadoModeracion)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Aprobado");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Latitud).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.Longitud).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.Nombre).HasMaxLength(150);
            entity.Property(e => e.Precio).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.VersionFila)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Categoria).WithMany(p => p.Eventos)
                .HasForeignKey(d => d.CategoriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Eventos_Categoria");

            entity.HasOne(d => d.Organizador).WithMany(p => p.Eventos)
                .HasForeignKey(d => d.OrganizadorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Eventos_Organizador");
        });

        modelBuilder.Entity<EventosCancelacion>(entity =>
        {
            entity.HasKey(e => e.EventoCancelacionId);

            entity.HasIndex(e => e.EventoId, "UQ_EventosCancelacion_Evento").IsUnique();

            entity.Property(e => e.DescripcionAdicional).HasMaxLength(500);
            entity.Property(e => e.FechaHora).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.CanceladoPor).WithMany(p => p.EventosCancelacion)
                .HasForeignKey(d => d.CanceladoPorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EventosCancelacion_Usuario");

            entity.HasOne(d => d.Evento).WithOne(p => p.EventosCancelacion)
                .HasForeignKey<EventosCancelacion>(d => d.EventoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EventosCancelacion_Evento");

            entity.HasOne(d => d.MotivoCancelacion).WithMany(p => p.EventosCancelacion)
                .HasForeignKey(d => d.MotivoCancelacionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EventosCancelacion_Motivo");
        });

        modelBuilder.Entity<EventosEmbeddings>(entity =>
        {
            entity.HasKey(e => e.EmbeddingId);

            entity.Property(e => e.FechaGeneracion).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.ModeloUsado)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Version).HasDefaultValue(1);
            entity.Property(e => e.Vigente).HasDefaultValue(true);

            entity.HasOne(d => d.Evento).WithMany(p => p.EventosEmbeddings)
                .HasForeignKey(d => d.EventoId)
                .HasConstraintName("FK_EventosEmbeddings_Evento");
        });

        modelBuilder.Entity<MensajesChatbot>(entity =>
        {
            entity.HasKey(e => e.MensajeId);

            entity.Property(e => e.Emisor)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.FechaHora).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Conversacion).WithMany(p => p.MensajesChatbot)
                .HasForeignKey(d => d.ConversacionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Mensajes_Conversacion");
        });

        modelBuilder.Entity<MotivosCancelacion>(entity =>
        {
            entity.HasKey(e => e.MotivoCancelacionId);

            entity.HasIndex(e => e.Codigo, "UQ_MotivosCancelacion_Codigo").IsUnique();

            entity.Property(e => e.Codigo)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.Descripcion).HasMaxLength(200);
        });

        modelBuilder.Entity<Notificaciones>(entity =>
        {
            entity.HasKey(e => e.NotificacionId);

            entity.HasIndex(e => new { e.UsuarioId, e.Leida }, "IX_Notificaciones_UsuarioId_Leida");

            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Mensaje).HasMaxLength(500);
            entity.Property(e => e.Tipo)
                .HasMaxLength(30)
                .IsUnicode(false);

            entity.HasOne(d => d.Evento).WithMany(p => p.Notificaciones)
                .HasForeignKey(d => d.EventoId)
                .HasConstraintName("FK_Notificaciones_Evento");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Notificaciones)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notificaciones_Usuario");
        });

        modelBuilder.Entity<Resenas>(entity =>
        {
            entity.HasKey(e => e.ResenaId);

            entity.HasIndex(e => new { e.UsuarioId, e.EventoId }, "UQ_Resenas_UsuarioEvento").IsUnique();

            entity.Property(e => e.Comentario).HasMaxLength(1000);
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Activa");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Entrada).WithMany(p => p.Resenas)
                .HasForeignKey(d => d.EntradaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Resenas_Entrada");

            entity.HasOne(d => d.Evento).WithMany(p => p.Resenas)
                .HasForeignKey(d => d.EventoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Resenas_Evento");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Resenas)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Resenas_Usuario");
        });

        modelBuilder.Entity<TokensRecuperacionContrasena>(entity =>
        {
            entity.HasKey(e => e.TokenId).HasName("PK_TokensRecuperacion");

            entity.Property(e => e.TokenId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.TokenHash).HasMaxLength(255);

            entity.HasOne(d => d.Usuario).WithMany(p => p.TokensRecuperacionContrasena)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("FK_Tokens_Usuario");
        });

        modelBuilder.Entity<UsuarioEventoInteraccion>(entity =>
        {
            entity.HasKey(e => e.InteraccionId);

            entity.HasIndex(e => new { e.UsuarioId, e.EventoId }, "IX_UsuarioEventoInteraccion_UsuarioId");

            entity.Property(e => e.FechaHora).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.TipoInteraccion)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Evento).WithMany(p => p.UsuarioEventoInteraccion)
                .HasForeignKey(d => d.EventoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Interaccion_Evento");

            entity.HasOne(d => d.Usuario).WithMany(p => p.UsuarioEventoInteraccion)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Interaccion_Usuario");
        });

        modelBuilder.Entity<Usuarios>(entity =>
        {
            entity.HasKey(e => e.UsuarioId);

            entity.HasIndex(e => e.Correo, "UQ_Usuarios_Correo").IsUnique();

            entity.Property(e => e.Apellidos).HasMaxLength(100);
            entity.Property(e => e.CiudadUbicacion).HasMaxLength(100);
            entity.Property(e => e.ContrasenaHash).HasMaxLength(255);
            entity.Property(e => e.Correo).HasMaxLength(150);
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Activo");
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Latitud).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.Longitud).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.Nombres).HasMaxLength(100);

            entity.HasMany(d => d.Categoria).WithMany(p => p.Usuario)
                .UsingEntity<Dictionary<string, object>>(
                    "UsuarioIntereses",
                    r => r.HasOne<Categorias>().WithMany()
                        .HasForeignKey("CategoriaId")
                        .HasConstraintName("FK_UsuarioIntereses_Categoria"),
                    l => l.HasOne<Usuarios>().WithMany()
                        .HasForeignKey("UsuarioId")
                        .HasConstraintName("FK_UsuarioIntereses_Usuario"),
                    j =>
                    {
                        j.HasKey("UsuarioId", "CategoriaId");
                    });
        });

        modelBuilder.Entity<vw_DisponibilidadEventos>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_DisponibilidadEventos");

            entity.Property(e => e.Nombre).HasMaxLength(150);
        });

        modelBuilder.Entity<vw_VentasPorEvento>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_VentasPorEvento");

            entity.Property(e => e.IngresosConfirmados).HasColumnType("decimal(38, 2)");
            entity.Property(e => e.MontoPendienteReembolso).HasColumnType("decimal(38, 2)");
            entity.Property(e => e.NombreEvento).HasMaxLength(150);
        });

        modelBuilder.Entity<vw_VentasPorFecha>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_VentasPorFecha");

            entity.Property(e => e.IngresosEseDia).HasColumnType("decimal(38, 2)");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
