using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AdoPetsBKD.Domain.Entities.Mascotas;

namespace AdoPetsBKD.Infrastructure.Data.Configurations.Mascotas;

public class SolicitudAdopcionConfiguration : IEntityTypeConfiguration<SolicitudAdopcion>
{
    public void Configure(EntityTypeBuilder<SolicitudAdopcion> builder)
    {
        builder.ToTable("SolicitudesAdopcion");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Direccion)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(s => s.IngresosMensuales)
            .HasPrecision(18, 2);

        builder.Property(s => s.MotivoAdopcion)
            .HasMaxLength(1000);

        builder.Property(s => s.MotivoRechazo)
            .HasMaxLength(1000);

        // Índices
        builder.HasIndex(s => new { s.MascotaId, s.Estado })
            .HasDatabaseName("IX_Adopcion_MascotaId_Estado");

        builder.HasIndex(s => new { s.UsuarioId, s.FechaSolicitud })
            .HasDatabaseName("IX_Adopcion_UsuarioId_Fecha");

        builder.HasIndex(s => s.Estado)
            .HasDatabaseName("IX_Adopcion_Estado");

        // Relaciones
        builder.HasOne(s => s.Usuario)
            .WithMany()
            .HasForeignKey(s => s.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Mascota)
            .WithMany(m => m.SolicitudesAdopcion)
            .HasForeignKey(s => s.MascotaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.Adjuntos)
            .WithOne(a => a.Solicitud)
            .HasForeignKey(a => a.SolicitudId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Logs)
            .WithOne(l => l.Solicitud)
            .HasForeignKey(l => l.SolicitudId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
