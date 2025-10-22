using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AdoPetsBKD.Domain.Entities.Clinica;

namespace AdoPetsBKD.Infrastructure.Data.Configurations.Clinica;

public class SolicitudCitaDigitalConfiguration : IEntityTypeConfiguration<SolicitudCitaDigital>
{
    public void Configure(EntityTypeBuilder<SolicitudCitaDigital> builder)
    {
        builder.ToTable("SolicitudesCitasDigitales");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.NumeroSolicitud)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(s => s.NumeroSolicitud)
            .IsUnique();

        builder.Property(s => s.NombreMascota)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.EspecieMascota)
            .HasMaxLength(50);

        builder.Property(s => s.RazaMascota)
            .HasMaxLength(100);

        builder.Property(s => s.DescripcionServicio)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(s => s.CostoEstimado)
            .HasPrecision(18, 2);

        builder.Property(s => s.MontoAnticipo)
            .HasPrecision(18, 2);

        builder.Property(s => s.Estado)
            .HasConversion<int>();

        builder.HasIndex(s => s.Estado);
        builder.HasIndex(s => s.FechaSolicitud);
        builder.HasIndex(s => s.FechaHoraSolicitada);

        // Relaciones
        builder.HasOne(s => s.Solicitante)
            .WithMany()
            .HasForeignKey(s => s.SolicitanteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Mascota)
            .WithMany()
            .HasForeignKey(s => s.MascotaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Servicio)
            .WithMany()
            .HasForeignKey(s => s.ServicioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.VeterinarioPreferido)
            .WithMany()
            .HasForeignKey(s => s.VeterinarioPreferidoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.SalaPreferida)
            .WithMany()
            .HasForeignKey(s => s.SalaPreferidaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.RevisadoPor)
            .WithMany()
            .HasForeignKey(s => s.RevisadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.PagoAnticipo)
            .WithMany()
            .HasForeignKey(s => s.PagoAnticipoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Cita)
            .WithMany()
            .HasForeignKey(s => s.CitaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
