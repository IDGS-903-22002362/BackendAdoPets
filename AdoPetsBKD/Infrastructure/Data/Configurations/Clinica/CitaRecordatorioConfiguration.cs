using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AdoPetsBKD.Domain.Entities.Clinica;

namespace AdoPetsBKD.Infrastructure.Data.Configurations.Clinica;

public class CitaRecordatorioConfiguration : IEntityTypeConfiguration<CitaRecordatorio>
{
    public void Configure(EntityTypeBuilder<CitaRecordatorio> builder)
    {
        builder.ToTable("CitasRecordatorios");

        builder.HasKey(r => r.Id);

        // Índice único para evitar duplicados
        builder.HasIndex(r => new { r.CitaId, r.Tipo })
            .IsUnique()
            .HasDatabaseName("UX_CitaRecordatorio_CitaId_Tipo");

        // Relaciones
        builder.HasOne(r => r.Cita)
            .WithMany(c => c.Recordatorios)
            .HasForeignKey(r => r.CitaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignorar propiedades computadas
        builder.Ignore(r => r.WasSent);
    }
}
