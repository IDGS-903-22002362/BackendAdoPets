using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AdoPetsBKD.Domain.Entities.Clinica;

namespace AdoPetsBKD.Infrastructure.Data.Configurations.Clinica;

public class TicketDetalleConfiguration : IEntityTypeConfiguration<TicketDetalle>
{
    public void Configure(EntityTypeBuilder<TicketDetalle> builder)
    {
        builder.ToTable("TicketDetalles");

        builder.HasKey(td => td.Id);

        builder.Property(td => td.Descripcion)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(td => td.Cantidad)
            .HasPrecision(18, 4);

        builder.Property(td => td.Unidad)
            .HasMaxLength(50);

        builder.Property(td => td.PrecioUnitario)
            .HasPrecision(18, 2);

        builder.Property(td => td.Subtotal)
            .HasPrecision(18, 2);

        builder.Property(td => td.Tipo)
            .HasConversion<int>();

        // Relaciones
        builder.HasOne(td => td.Ticket)
            .WithMany(t => t.Detalles)
            .HasForeignKey(td => td.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(td => td.ItemInventario)
            .WithMany()
            .HasForeignKey(td => td.ItemInventarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
