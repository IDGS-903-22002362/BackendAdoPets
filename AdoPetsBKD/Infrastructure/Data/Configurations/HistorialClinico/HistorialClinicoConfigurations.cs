using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AdoPetsBKD.Domain.Entities.HistorialClinico;

namespace AdoPetsBKD.Infrastructure.Data.Configurations.HistorialClinico;

public class ValoracionConfiguration : IEntityTypeConfiguration<Valoracion>
{
    public void Configure(EntityTypeBuilder<Valoracion> builder)
    {
        builder.ToTable("Valoraciones");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Peso).HasPrecision(5, 2);
        builder.Property(v => v.Temperatura).HasPrecision(4, 1);
        builder.Property(v => v.CondicionCorporal).HasMaxLength(100);
        builder.Property(v => v.Observaciones).HasMaxLength(2000);

        builder.HasIndex(v => new { v.MascotaId, v.TakenAt })
            .HasDatabaseName("IX_Valoracion_MascotaId_TakenAt");

        builder.HasOne(v => v.Cita).WithMany().HasForeignKey(v => v.CitaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(v => v.Mascota).WithMany().HasForeignKey(v => v.MascotaId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ExpedienteConfiguration : IEntityTypeConfiguration<Expediente>
{
    public void Configure(EntityTypeBuilder<Expediente> builder)
    {
        builder.ToTable("Expedientes");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.MotivoConsulta).HasMaxLength(1000);
        builder.Property(e => e.Anamnesis).HasMaxLength(2000);
        builder.Property(e => e.Diagnostico).IsRequired().HasMaxLength(2000);
        builder.Property(e => e.Tratamiento).HasMaxLength(2000);
        builder.Property(e => e.Notas).HasMaxLength(2000);
        builder.Property(e => e.Pronostico).HasMaxLength(500);

        builder.HasIndex(e => new { e.MascotaId, e.Fecha })
            .HasDatabaseName("IX_Expediente_MascotaId_Fecha");

        builder.HasOne(e => e.Mascota).WithMany().HasForeignKey(e => e.MascotaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Cita).WithMany().HasForeignKey(e => e.CitaId).OnDelete(DeleteBehavior.SetNull);
        builder.HasMany(e => e.Adjuntos).WithMany();
    }
}

public class VacunacionConfiguration : IEntityTypeConfiguration<Vacunacion>
{
    public void Configure(EntityTypeBuilder<Vacunacion> builder)
    {
        builder.ToTable("Vacunaciones");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.VaccineName).IsRequired().HasMaxLength(255);
        builder.Property(v => v.Dose).HasMaxLength(100);
        builder.Property(v => v.Lot).HasMaxLength(100);
        builder.Property(v => v.Notes).HasMaxLength(1000);
        builder.Property(v => v.ReaccionAdversa).HasMaxLength(1000);

        builder.HasIndex(v => new { v.MascotaId, v.AppliedAt })
            .HasDatabaseName("IX_Vacuna_MascotaId_AppliedAt");

        builder.HasOne(v => v.Mascota).WithMany().HasForeignKey(v => v.MascotaId).OnDelete(DeleteBehavior.Restrict);
        builder.Ignore(v => v.RequiereRefuerzo);
    }
}

public class DesparasitacionConfiguration : IEntityTypeConfiguration<Desparasitacion>
{
    public void Configure(EntityTypeBuilder<Desparasitacion> builder)
    {
        builder.ToTable("Desparasitaciones");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Product).IsRequired().HasMaxLength(255);
        builder.Property(d => d.Dose).HasMaxLength(100);
        builder.Property(d => d.Notes).HasMaxLength(1000);

        builder.HasIndex(d => new { d.MascotaId, d.AppliedAt })
            .HasDatabaseName("IX_Desp_MascotaId_AppliedAt");

        builder.HasOne(d => d.Mascota).WithMany().HasForeignKey(d => d.MascotaId).OnDelete(DeleteBehavior.Restrict);
        builder.Ignore(d => d.RequiereSiguiente);
    }
}

public class CirugiaConfiguration : IEntityTypeConfiguration<Cirugia>
{
    public void Configure(EntityTypeBuilder<Cirugia> builder)
    {
        builder.ToTable("Cirugias");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Tipo).IsRequired().HasMaxLength(255);
        builder.Property(c => c.Descripcion).HasMaxLength(2000);
        builder.Property(c => c.Anesthesia).HasMaxLength(255);
        builder.Property(c => c.Notes).HasMaxLength(2000);
        builder.Property(c => c.Medicacion).HasMaxLength(1000);
        builder.Property(c => c.CuidadosPostoperatorios).HasMaxLength(2000);

        builder.HasIndex(c => new { c.MascotaId, c.PerformedAt })
            .HasDatabaseName("IX_Cirugia_MascotaId_PerformedAt");

        builder.HasOne(c => c.Mascota).WithMany().HasForeignKey(c => c.MascotaId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class AdjuntoMedicoConfiguration : IEntityTypeConfiguration<AdjuntoMedico>
{
    public void Configure(EntityTypeBuilder<AdjuntoMedico> builder)
    {
        builder.ToTable("AdjuntosMedicos");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.StorageKey).IsRequired().HasMaxLength(500);
        builder.Property(a => a.FileName).IsRequired().HasMaxLength(255);
        builder.Property(a => a.MimeType).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Descripcion).HasMaxLength(500);

        builder.HasIndex(a => new { a.MascotaId, a.UploadedAt })
            .HasDatabaseName("IX_AdjMed_MascotaId_UploadedAt");

        builder.HasIndex(a => new { a.EntryType, a.EntryId })
            .HasDatabaseName("IX_AdjMed_EntryType_EntryId");

        builder.HasOne(a => a.Mascota).WithMany().HasForeignKey(a => a.MascotaId).OnDelete(DeleteBehavior.Restrict);
    }
}
