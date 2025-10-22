using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AdoPetsBKD.Domain.Entities.Clinica;

namespace AdoPetsBKD.Infrastructure.Data.Configurations.Clinica;

public class PagoConfiguration : IEntityTypeConfiguration<Pago>
{
    public void Configure(EntityTypeBuilder<Pago> builder)
    {
        builder.ToTable("Pagos");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.NumeroPago)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(p => p.NumeroPago)
            .IsUnique();

        builder.Property(p => p.Monto)
            .HasPrecision(18, 2);

        builder.Property(p => p.Moneda)
            .HasMaxLength(10)
            .HasDefaultValue("MXN");

        builder.Property(p => p.MontoTotal)
            .HasPrecision(18, 2);

        builder.Property(p => p.MontoRestante)
            .HasPrecision(18, 2);

        builder.Property(p => p.Tipo)
            .HasConversion<int>();

        builder.Property(p => p.Metodo)
            .HasConversion<int>();

        builder.Property(p => p.Estado)
            .HasConversion<int>();

        builder.Property(p => p.PayPalOrderId)
            .HasMaxLength(100);

        builder.HasIndex(p => p.PayPalOrderId);

        builder.Property(p => p.PayPalCaptureId)
            .HasMaxLength(100);

        builder.HasIndex(p => p.PayPalCaptureId);

        builder.Property(p => p.PayPalPayerId)
            .HasMaxLength(100);

        builder.Property(p => p.PayPalPayerEmail)
            .HasMaxLength(200);

        builder.Property(p => p.PayPalPayerName)
            .HasMaxLength(200);

        builder.Property(p => p.Concepto)
            .HasMaxLength(500);

        builder.Property(p => p.Referencia)
            .HasMaxLength(100);

        // Relaciones
        builder.HasOne(p => p.Usuario)
            .WithMany()
            .HasForeignKey(p => p.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Cita)
            .WithMany()
            .HasForeignKey(p => p.CitaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.PagoPrincipal)
            .WithMany(p => p.PagosComplementarios)
            .HasForeignKey(p => p.PagoPrincipalId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
