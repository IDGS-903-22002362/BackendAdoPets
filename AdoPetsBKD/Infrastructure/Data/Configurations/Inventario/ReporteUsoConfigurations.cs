using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AdoPetsBKD.Domain.Entities.Inventario;

namespace AdoPetsBKD.Infrastructure.Data.Configurations.Inventario;

public class ReporteUsoInsumosConfiguration : IEntityTypeConfiguration<ReporteUsoInsumos>
{
    public void Configure(EntityTypeBuilder<ReporteUsoInsumos> builder)
    {
        builder.ToTable("ReportesUsoInsumos");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Notes)
            .HasMaxLength(2000);

        builder.Property(r => r.ClientUsageId)
            .HasMaxLength(255);

        builder.Property(r => r.RevertReason)
            .HasMaxLength(1000);

        // Índices para idempotencia y consultas
        builder.HasIndex(r => r.CitaId)
            .IsUnique()
            .HasDatabaseName("UX_Report_Appointment");

        builder.HasIndex(r => r.ClientUsageId)
            .IsUnique()
            .HasFilter("[ClientUsageId] IS NOT NULL")
            .HasDatabaseName("UX_Report_ClientUsageId");

        builder.HasIndex(r => new { r.Status, r.SubmittedAt })
            .HasDatabaseName("IX_Report_Status_SubmittedAt");

        // Relaciones
        builder.HasOne(r => r.Cita)
            .WithOne(c => c.ReporteInsumos)
            .HasForeignKey<ReporteUsoInsumos>(r => r.CitaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.Detalles)
            .WithOne(d => d.Reporte)
            .HasForeignKey(d => d.ReporteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ReporteUsoInsumoDetalleConfiguration : IEntityTypeConfiguration<ReporteUsoInsumoDetalle>
{
    public void Configure(EntityTypeBuilder<ReporteUsoInsumoDetalle> builder)
    {
        builder.ToTable("ReportesUsoInsumosDetalles");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.QtySolicitada)
            .HasPrecision(18, 3);

        builder.Property(d => d.QtyAplicada)
            .HasPrecision(18, 3);

        builder.Property(d => d.Notas)
            .HasMaxLength(1000);

        // Índices
        builder.HasIndex(d => d.ReporteId)
            .HasDatabaseName("IX_ReporteDetalle_ReporteId");

        builder.HasIndex(d => d.ItemId)
            .HasDatabaseName("IX_ReporteDetalle_ItemId");

        // Relaciones
        builder.HasOne(d => d.Reporte)
            .WithMany(r => r.Detalles)
            .HasForeignKey(d => d.ReporteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Item)
            .WithMany()
            .HasForeignKey(d => d.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(d => d.Splits)
            .WithOne(s => s.Detalle)
            .HasForeignKey(s => s.DetalleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignorar propiedades computadas
        builder.Ignore(d => d.TotalConsumido);
    }
}

public class ReporteUsoSplitLoteConfiguration : IEntityTypeConfiguration<ReporteUsoSplitLote>
{
    public void Configure(EntityTypeBuilder<ReporteUsoSplitLote> builder)
    {
        builder.ToTable("ReportesUsoSplitLotes");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.QtyConsumida)
            .HasPrecision(18, 3);

        // Índices
        builder.HasIndex(s => new { s.ReporteId, s.DetalleId })
            .HasDatabaseName("IX_Split_ReporteId_DetalleId");

        builder.HasIndex(s => s.BatchId)
            .HasDatabaseName("IX_Split_BatchId");

        // Relaciones
        builder.HasOne(s => s.Reporte)
            .WithMany()
            .HasForeignKey(s => s.ReporteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Detalle)
            .WithMany(d => d.Splits)
            .HasForeignKey(s => s.DetalleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Batch)
            .WithMany(l => l.SplitsUso)
            .HasForeignKey(s => s.BatchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
