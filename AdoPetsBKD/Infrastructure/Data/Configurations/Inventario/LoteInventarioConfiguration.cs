using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AdoPetsBKD.Domain.Entities.Inventario;

namespace AdoPetsBKD.Infrastructure.Data.Configurations.Inventario;

public class LoteInventarioConfiguration : IEntityTypeConfiguration<LoteInventario>
{
    public void Configure(EntityTypeBuilder<LoteInventario> builder)
    {
        builder.ToTable("LotesInventario");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Lote)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.QtyDisponible)
            .HasPrecision(18, 3);

        builder.Property(l => l.QtyInicial)
            .HasPrecision(18, 3);

        builder.Property(l => l.Notas)
            .HasMaxLength(1000);

        // Índices críticos para FIFO
        builder.HasIndex(l => new { l.ItemId, l.ExpDate })
            .HasDatabaseName("IX_Lote_ItemId_ExpDate");

        builder.HasIndex(l => new { l.ItemId, l.QtyDisponible })
            .HasDatabaseName("IX_Lote_ItemId_QtyDisponible");

        builder.HasIndex(l => l.ExpDate)
            .HasDatabaseName("IX_Lote_ExpDate");

        // Relaciones
        builder.HasOne(l => l.Item)
            .WithMany(i => i.Lotes)
            .HasForeignKey(l => l.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(l => l.Movimientos)
            .WithOne(m => m.Batch)
            .HasForeignKey(m => m.BatchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(l => l.SplitsUso)
            .WithOne(s => s.Batch)
            .HasForeignKey(s => s.BatchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
