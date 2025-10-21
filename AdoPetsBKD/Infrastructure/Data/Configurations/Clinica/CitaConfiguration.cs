using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AdoPetsBKD.Domain.Entities.Clinica;
using AdoPetsBKD.Domain.Entities.Inventario;

namespace AdoPetsBKD.Infrastructure.Data.Configurations.Clinica;

public class CitaConfiguration : IEntityTypeConfiguration<Cita>
{
    public void Configure(EntityTypeBuilder<Cita> builder)
    {
        builder.ToTable("Citas");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Notas)
            .HasMaxLength(2000);

        builder.Property(c => c.MotivoConsulta)
            .HasMaxLength(1000);

        builder.Property(c => c.MotivoRechazo)
            .HasMaxLength(1000);

        // Índices críticos para evitar solapamientos
        builder.HasIndex(c => new { c.VeterinarioId, c.StartAt, c.Status })
            .HasDatabaseName("IX_Cita_Vet_Start_Status");

        builder.HasIndex(c => new { c.SalaId, c.StartAt, c.Status })
            .HasDatabaseName("IX_Cita_Sala_Start_Status");

        builder.HasIndex(c => new { c.MascotaId, c.StartAt })
            .HasDatabaseName("IX_Cita_MascotaId_StartAt");

        builder.HasIndex(c => c.Status)
            .HasDatabaseName("IX_Cita_Status");

        // Relaciones
        builder.HasOne(c => c.Mascota)
            .WithMany()
            .HasForeignKey(c => c.MascotaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Propietario)
            .WithMany()
            .HasForeignKey(c => c.PropietarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Veterinario)
            .WithMany()
            .HasForeignKey(c => c.VeterinarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Sala)
            .WithMany(s => s.Citas)
            .HasForeignKey(c => c.SalaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(c => c.Recordatorios)
            .WithOne(r => r.Cita)
            .HasForeignKey(r => r.CitaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.HistorialEstados)
            .WithOne(h => h.Cita)
            .HasForeignKey(h => h.CitaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.ReporteInsumos)
            .WithOne(r => r.Cita)
            .HasForeignKey<ReporteUsoInsumos>(r => r.CitaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
