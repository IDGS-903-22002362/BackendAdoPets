using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AdoPetsBKD.Domain.Entities.Clinica;

namespace AdoPetsBKD.Infrastructure.Data.Configurations.Clinica;

public class CitaHistorialEstadoConfiguration : IEntityTypeConfiguration<CitaHistorialEstado>
{
    public void Configure(EntityTypeBuilder<CitaHistorialEstado> builder)
    {
        builder.ToTable("CitasHistorialEstados");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Notas)
            .HasMaxLength(1000);

        // Índices
        builder.HasIndex(h => new { h.CitaId, h.ChangedAt })
            .HasDatabaseName("IX_CitaHistorial_CitaId_ChangedAt");

        // Relaciones
        builder.HasOne(h => h.Cita)
            .WithMany(c => c.HistorialEstados)
            .HasForeignKey(h => h.CitaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
