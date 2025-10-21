using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AdoPetsBKD.Domain.Entities.Inventario;

namespace AdoPetsBKD.Infrastructure.Data.Configurations.Inventario;

public class ItemInventarioConfiguration : IEntityTypeConfiguration<ItemInventario>
{
    public void Configure(EntityTypeBuilder<ItemInventario> builder)
    {
        builder.ToTable("ItemsInventario");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Nombre)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(i => i.Unidad)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(i => i.MinQty)
            .HasPrecision(18, 3);

        builder.Property(i => i.Descripcion)
            .HasMaxLength(1000);

        builder.Property(i => i.Notas)
            .HasMaxLength(2000);

        // Índices
        builder.HasIndex(i => i.Nombre)
            .IsUnique()
            .HasDatabaseName("UX_ItemInventario_Nombre");

        builder.HasIndex(i => i.Activo)
            .HasDatabaseName("IX_ItemInventario_Activo");

        builder.HasIndex(i => i.Categoria)
            .HasDatabaseName("IX_ItemInventario_Categoria");

        // Relaciones
        builder.HasMany(i => i.Lotes)
            .WithOne(l => l.Item)
            .HasForeignKey(l => l.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.Movimientos)
            .WithOne(m => m.Item)
            .HasForeignKey(m => m.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.Alertas)
            .WithOne(a => a.Item)
            .HasForeignKey(a => a.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignorar propiedades computadas
        builder.Ignore(i => i.StockTotal);
    }
}
