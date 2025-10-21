using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AdoPetsBKD.Domain.Entities.Donaciones;

namespace AdoPetsBKD.Infrastructure.Data.Configurations.Donaciones;

public class DonacionConfiguration : IEntityTypeConfiguration<Donacion>
{
    public void Configure(EntityTypeBuilder<Donacion> builder)
    {
        builder.ToTable("Donaciones");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Monto).HasPrecision(18, 2);
        builder.Property(d => d.Moneda).IsRequired().HasMaxLength(3).HasDefaultValue("MXN");
        builder.Property(d => d.PaypalOrderId).IsRequired().HasMaxLength(255);
        builder.Property(d => d.PaypalCaptureId).HasMaxLength(255);
        builder.Property(d => d.PayerEmail).HasMaxLength(255);
        builder.Property(d => d.PayerName).HasMaxLength(255);
        builder.Property(d => d.PaypalPayerId).HasMaxLength(255);
        builder.Property(d => d.Mensaje).HasMaxLength(1000);
        builder.Property(d => d.CancellationReason).HasMaxLength(1000);

        // Índices críticos
        builder.HasIndex(d => d.PaypalCaptureId)
            .IsUnique()
            .HasFilter("[PaypalCaptureId] IS NOT NULL")
            .HasDatabaseName("UX_Donacion_PaypalCaptureId");

        builder.HasIndex(d => new { d.Status, d.CreatedAt })
            .HasDatabaseName("IX_Donacion_Status_CreatedAt");

        builder.HasIndex(d => d.PaypalOrderId)
            .HasDatabaseName("IX_Donacion_PaypalOrderId");

        // Relaciones
        builder.HasOne(d => d.Usuario)
            .WithMany()
            .HasForeignKey(d => d.UsuarioId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class WebhookEventConfiguration : IEntityTypeConfiguration<WebhookEvent>
{
    public void Configure(EntityTypeBuilder<WebhookEvent> builder)
    {
        builder.ToTable("WebhookEvents");
        builder.HasKey(w => w.Id);

        builder.Property(w => w.EventId).IsRequired().HasMaxLength(255);
        builder.Property(w => w.Tipo).IsRequired().HasMaxLength(100);
        builder.Property(w => w.PayloadJson).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(w => w.ErrorMessage).HasMaxLength(2000);

        // Índice único para evitar duplicados
        builder.HasIndex(w => w.EventId)
            .IsUnique()
            .HasDatabaseName("UX_Webhook_EventId");

        builder.HasIndex(w => new { w.Provider, w.Status, w.ReceivedAt })
            .HasDatabaseName("IX_Webhook_Provider_Status_ReceivedAt");
    }
}
