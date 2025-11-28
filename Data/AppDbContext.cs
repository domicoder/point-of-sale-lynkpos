using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<CajaEstadoModel> CajaEstados { get; set; }
        public DbSet<Caja> Cajas { get; set; }
        public DbSet<CajaVitacora> CajaVitacoras { get; set; }
        public DbSet<TipoFacturaModel> TiposFacturas { get; set; }
        public DbSet<EstadoFacturaModel> EstadosFacturas { get; set; }
        public DbSet<Factura> Facturas { get; set; }
        public DbSet<FacturaDetalle> FacturasDetalles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Rol>(entity =>
            {
                entity.ToTable("roles");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id);
                entity.Property(x => x.Nombre).IsRequired().HasMaxLength(50);
                entity.Property(x => x.CreadoEn).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(x => x.ActualizadoEn);

                entity.HasData(new Rol {
                    Id=1,
                    Nombre="ADMIN"
                });
            });

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("usuarios");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id);
                entity.Property(x => x.RolId).IsRequired();
                entity.Property(x => x.Nombre).IsRequired().HasMaxLength(150);
                entity.Property(x => x.UsuarioNombre).IsRequired().HasMaxLength(30);
                entity.Property(x => x.Password).IsRequired().HasMaxLength(1000);
                entity.Property(x => x.Activo).IsRequired();
                entity.Property(x => x.Eliminado).IsRequired();
                entity.Property(x => x.CreadoEn).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(x => x.ActualizadoEn);

                entity.HasOne(x => x.Rol)
                      .WithMany(x => x.Usuarios)
                      .HasForeignKey(x => x.RolId)
                      .HasConstraintName("usuarios_fk_rol_id");

                entity.HasIndex(x => x.UsuarioNombre)
                    .HasDatabaseName("udx_usuario_nombre")
                    .IsUnique()
                    .HasFilter("[Eliminado] = 0");

                entity.HasIndex(x => new { x.Eliminado, x.Activo })
                    .HasDatabaseName("idx_usuario_eliminado_activo");

                entity.HasData(new Usuario()
                {
                    Id = new Guid("bfe03e22-65e4-4007-8420-07c1b53c4726"),
                    RolId = 1,
                    Nombre = "Admin",
                    UsuarioNombre = "admin",
                    Password = "9U0zeOGybSi5hk81k/nFzw==.FN5jpe1k2hAMfU0SIg2QuTiwVdhsFdYsC1ykHHAwkzk=",
                    Activo = true,
                    Eliminado = false
                });
            });

            modelBuilder.Entity<CajaEstadoModel>(entity =>
            {
                entity.ToTable("caja_estados");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id);
                entity.Property(x => x.Nombre).IsRequired().HasMaxLength(50);
                entity.Property(x => x.CreadoEn).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasData(new CajaEstadoModel
                {
                    Id = 1,
                    Nombre = "CERRADO"
                });

                entity.HasData(new CajaEstadoModel
                {
                    Id = 2,
                    Nombre = "ABIERTO"
                });
            });

            modelBuilder.Entity<Caja>(entity =>
            {
                entity.ToTable("cajas");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id);
                entity.Property(x => x.EstadoId).IsRequired();
                entity.Property(x => x.Codigo).IsRequired().HasMaxLength(30);
                entity.Property(x => x.Nombre).IsRequired().HasMaxLength(50);
                entity.Property(x => x.CreadoEn).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(x => x.CajaEstado)
                      .WithMany(x => x.Cajas)
                      .HasForeignKey(x => x.EstadoId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("caja_fk_estado_id");

                entity.HasIndex(x => x.Codigo)
                    .IsUnique()
                    .HasDatabaseName("caja_udx_codigo");
            });

            modelBuilder.Entity<CajaVitacora>(entity =>
            {
                entity.ToTable("caja_vitacora");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id);
                entity.Property(x => x.UsuarioId).IsRequired();
                entity.Property(x => x.CajaId).IsRequired();
                entity.Property(x => x.FechaApertura).IsRequired();
                entity.Property(x => x.CreadoEn).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(x => x.Caja)
                      .WithMany(x => x.CajaVitacoras)
                      .HasForeignKey(x => x.CajaId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("caja_vitacora_fk_caja_id");

                entity.HasOne(x => x.Usuario)
                    .WithMany(x => x.CajaVitacoras)
                    .HasForeignKey(v => v.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("caja_vitacora_fk_usuario_id");
            });

            modelBuilder.Entity<TipoFacturaModel>(entity =>
            {
                entity.ToTable("tipos_facturas");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id);
                entity.Property(x => x.Nombre).IsRequired().HasMaxLength(50);
                entity.Property(x => x.CreadoEn).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasData(new TipoFacturaModel
                {
                    Id = 1,
                    Nombre = "DEBITO"
                });

                entity.HasData(new TipoFacturaModel
                {
                    Id = 2,
                    Nombre = "CREDITO"
                });
            });

            modelBuilder.Entity<EstadoFacturaModel>(entity =>
            {
                entity.ToTable("estados_facturas");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id);
                entity.Property(x => x.Nombre).IsRequired().HasMaxLength(50);
                entity.Property(x => x.CreadoEn).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasData(new EstadoFacturaModel
                {
                    Id = 1,
                    Nombre = "EMITIDA"
                });

                entity.HasData(new EstadoFacturaModel
                {
                    Id = 2,
                    Nombre = "CANCELADA"
                });
            });

            modelBuilder.Entity<Factura>(entity =>
            {
                entity.ToTable("facturas");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id);
                entity.Property(x => x.TipoId).IsRequired();
                entity.Property(x => x.EstadoId).IsRequired();
                entity.Property(x => x.UsuarioId).IsRequired();
                entity.Property(x => x.CajaId).IsRequired();
                entity.Property(x => x.FechaEmision).IsRequired();
                entity.Property(x => x.Total)
                    .HasPrecision(12,2)
                    .HasColumnType("decimal(12,2)")
                    .IsRequired();
                entity.Property(x => x.Subtotal)
                    .HasPrecision(12, 2)
                    .HasColumnType("decimal(12,2)")
                    .IsRequired();
                entity.Property(x => x.Impuestos)
                    .HasPrecision(12, 2)
                    .HasColumnType("decimal(12,2)")
                    .IsRequired();
                entity.Property(x => x.CreadoEn).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(x => x.TipoFactura)
                      .WithMany(x => x.Facturas)
                      .HasForeignKey(x => x.TipoId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("facturas_fk_tipo_id");

                entity.HasOne(x => x.EstadoFactura)
                      .WithMany(x => x.Facturas)
                      .HasForeignKey(x => x.EstadoId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("facturas_fk_estado_id");

                entity.HasOne(x => x.Usuario)
                    .WithMany(x => x.Facturas)
                    .HasForeignKey(v => v.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("facturas_fk_usuario_id");

                entity.HasOne(x => x.Caja)
                      .WithMany(x => x.Facturas)
                      .HasForeignKey(x => x.CajaId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("facturas_fk_caja_id");
            });

            modelBuilder.Entity<FacturaDetalle>(entity =>
            {
                entity.ToTable("facturas_detalles");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id);
                entity.Property(x => x.FacturaId).IsRequired();
                entity.Property(x => x.ProductoId).HasMaxLength(36).IsRequired();
                entity.Property(x => x.NombreProducto).HasMaxLength(250).IsRequired();
                entity.Property(x => x.Cantidad).IsRequired();
                entity.Property(x => x.ImpuestoPorcentaje)
                    .HasPrecision(4, 2)
                    .HasColumnType("decimal(4,2)")
                    .IsRequired();
                entity.Property(x => x.PrecioUnitario)
                    .HasPrecision(12, 2)
                    .HasColumnType("decimal(12,2)")
                    .IsRequired();
                entity.Property(x => x.Total)
                    .HasPrecision(12, 2)
                    .HasColumnType("decimal(12,2)")
                    .IsRequired();
                entity.Property(x => x.Subtotal)
                    .HasPrecision(12, 2)
                    .HasColumnType("decimal(12,2)")
                    .IsRequired();
                entity.Property(x => x.Impuestos)
                    .HasPrecision(12, 2)
                    .HasColumnType("decimal(12,2)")
                    .IsRequired();
                entity.Property(x => x.CreadoEn).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(x => x.Factura)
                      .WithOne(x => x.FacturaDetalle)
                      .HasForeignKey<FacturaDetalle>(x => x.FacturaId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("facturas_detalles_fk_factura_id");
            });
        }
    }
}
