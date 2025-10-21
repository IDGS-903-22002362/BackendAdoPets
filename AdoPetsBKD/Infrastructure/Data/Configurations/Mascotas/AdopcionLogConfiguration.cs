using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AdoPetsBKD.Domain.Entities.Mascotas;

namespace AdoPetsBKD.Infrastructure.Data.Configurations.Mascotas;

public class AdopcionLogConfiguration : IEntityTypeConfiguration<AdopcionLog>
{
    public void Configure(EntityTypeBuilder<AdopcionLog> builder)
    {
        builder.ToTable("AdopcionLogs");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Reason)
            .HasMaxLength(1000);

        // Índices
        builder.HasIndex(l => new { l.SolicitudId, l.ChangedAt })
            .HasDatabaseName("IX_AdopLog_SolicitudId_ChangedAt");

        // Relaciones
        builder.HasOne(l => l.Solicitud)
            .WithMany(s => s.Logs)
            .HasForeignKey(l => l.SolicitudId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
