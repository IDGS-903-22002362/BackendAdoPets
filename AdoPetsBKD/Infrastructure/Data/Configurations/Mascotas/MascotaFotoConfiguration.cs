using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AdoPetsBKD.Domain.Entities.Mascotas;

namespace AdoPetsBKD.Infrastructure.Data.Configurations.Mascotas;

public class MascotaFotoConfiguration : IEntityTypeConfiguration<MascotaFoto>
{
    public void Configure(EntityTypeBuilder<MascotaFoto> builder)
    {
        builder.ToTable("MascotasFotos");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.StorageKey)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(f => f.MimeType)
            .IsRequired()
            .HasMaxLength(100);

        // Índices
        builder.HasIndex(f => new { f.MascotaId, f.Orden })
            .HasDatabaseName("IX_MascotaFoto_MascotaId_Orden");

        // Relaciones
        builder.HasOne(f => f.Mascota)
            .WithMany(m => m.Fotos)
            .HasForeignKey(f => f.MascotaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
