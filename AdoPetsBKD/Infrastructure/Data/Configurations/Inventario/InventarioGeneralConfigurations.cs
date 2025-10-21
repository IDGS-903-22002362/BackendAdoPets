using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AdoPetsBKD.Domain.Entities.Inventario;

namespace AdoPetsBKD.Infrastructure.Data.Configurations.Inventario;

public class MovimientoInventarioConfiguration : IEntityTypeConfiguration<MovimientoInventario>
{
    public void Configure(EntityTypeBuilder<MovimientoInventario> builder)
    {
        builder.ToTable("MovimientosInventario");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Qty)
            .HasPrecision(18, 3);

        builder.Property(m => m.Reason)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(m => m.Observaciones)
            .HasMaxLength(2000);

        // Índices
        builder.HasIndex(m => new { m.ItemId, m.CreatedAt })
            .HasDatabaseName("IX_Mov_ItemId_CreatedAt");

        builder.HasIndex(m => new { m.BatchId, m.CreatedAt })
            .HasDatabaseName("IX_Mov_BatchId_CreatedAt");

        builder.HasIndex(m => m.RelatedAppointmentId)
            .HasDatabaseName("IX_Mov_AppointmentId");

        // Relaciones
        builder.HasOne(m => m.Item)
            .WithMany(i => i.Movimientos)
            .HasForeignKey(m => m.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Batch)
            .WithMany(l => l.Movimientos)
            .HasForeignKey(m => m.BatchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.RelatedAppointment)
            .WithMany()
            .HasForeignKey(m => m.RelatedAppointmentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class AlertaInventarioConfiguration : IEntityTypeConfiguration<AlertaInventario>
{
    public void Configure(EntityTypeBuilder<AlertaInventario> builder)
    {
        builder.ToTable("AlertasInventario");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.PayloadJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.ResolutionNotes)
            .HasMaxLength(1000);

        // Índices
        builder.HasIndex(a => new { a.ItemId, a.Status, a.Tipo })
            .HasDatabaseName("IX_Alert_ItemId_Status_Tipo");

        builder.HasIndex(a => new { a.Status, a.CreatedAt })
            .HasDatabaseName("IX_Alert_Status_CreatedAt");

        // Relaciones
        builder.HasOne(a => a.Item)
            .WithMany(i => i.Alertas)
            .HasForeignKey(a => a.ItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProveedorConfiguration : IEntityTypeConfiguration<Proveedor>
{
    public void Configure(EntityTypeBuilder<Proveedor> builder)
    {
        builder.ToTable("Proveedores");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nombre)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(p => p.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(p => p.Telefono)
            .HasMaxLength(20);

        builder.Property(p => p.Direccion)
            .HasMaxLength(500);

        builder.Property(p => p.RFC)
            .HasMaxLength(13);

        builder.Property(p => p.Contacto)
            .HasMaxLength(255);

        builder.Property(p => p.Notas)
            .HasMaxLength(2000);

        // Índices
        builder.HasIndex(p => p.Email)
            .HasDatabaseName("IX_Proveedor_Email");

        builder.HasIndex(p => p.Estatus)
            .HasDatabaseName("IX_Proveedor_Estatus");

        // Relaciones
        builder.HasMany(p => p.Compras)
            .WithOne(c => c.Proveedor)
            .HasForeignKey(c => c.ProveedorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class CompraConfiguration : IEntityTypeConfiguration<Compra>
{
    public void Configure(EntityTypeBuilder<Compra> builder)
    {
        builder.ToTable("Compras");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.NumeroFactura)
            .HasMaxLength(100);

        builder.Property(c => c.Total)
            .HasPrecision(18, 2);

        builder.Property(c => c.Notas)
            .HasMaxLength(2000);

        // Índices
        builder.HasIndex(c => new { c.ProveedorId, c.FechaCompra })
            .HasDatabaseName("IX_Compra_ProveedorId_Fecha");

        builder.HasIndex(c => c.Status)
            .HasDatabaseName("IX_Compra_Status");

        // Relaciones
        builder.HasOne(c => c.Proveedor)
            .WithMany(p => p.Compras)
            .HasForeignKey(c => c.ProveedorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Detalles)
            .WithOne(d => d.Compra)
            .HasForeignKey(d => d.CompraId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class DetalleCompraConfiguration : IEntityTypeConfiguration<DetalleCompra>
{
    public void Configure(EntityTypeBuilder<DetalleCompra> builder)
    {
        builder.ToTable("DetallesCompras");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Lote)
            .HasMaxLength(100);

        builder.Property(d => d.Cantidad)
            .HasPrecision(18, 3);

        builder.Property(d => d.PrecioUnitario)
            .HasPrecision(18, 2);

        builder.Property(d => d.Notas)
            .HasMaxLength(1000);

        // Índice
        builder.HasIndex(d => d.CompraId)
            .HasDatabaseName("IX_DetalleCompra_CompraId");

        // Relaciones
        builder.HasOne(d => d.Compra)
            .WithMany(c => c.Detalles)
            .HasForeignKey(d => d.CompraId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Item)
            .WithMany()
            .HasForeignKey(d => d.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ignorar propiedades computadas
        builder.Ignore(d => d.Subtotal);
    }
}
