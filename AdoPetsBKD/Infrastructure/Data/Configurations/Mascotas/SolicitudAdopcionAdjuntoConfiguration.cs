using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AdoPetsBKD.Domain.Entities.Mascotas;

namespace AdoPetsBKD.Infrastructure.Data.Configurations.Mascotas;

public class SolicitudAdopcionAdjuntoConfiguration : IEntityTypeConfiguration<SolicitudAdopcionAdjunto>
{
    public void Configure(EntityTypeBuilder<SolicitudAdopcionAdjunto> builder)
    {
        builder.ToTable("SolicitudesAdopcionAdjuntos");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.StorageKey)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.MimeType)
            .IsRequired()
            .HasMaxLength(100);

        // Índices
        builder.HasIndex(a => new { a.SolicitudId, a.UploadedAt })
            .HasDatabaseName("IX_Adjunto_SolicitudId_UploadedAt");

        // Relaciones
        builder.HasOne(a => a.Solicitud)
            .WithMany(s => s.Adjuntos)
            .HasForeignKey(a => a.SolicitudId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
