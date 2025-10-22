using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AdoPetsBKD.Domain.Entities.Clinica;

namespace AdoPetsBKD.Infrastructure.Data.Configurations.Clinica;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("Tickets");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.NumeroTicket)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(t => t.NumeroTicket)
            .IsUnique();

        builder.Property(t => t.NombreProcedimiento)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.DescripcionProcedimiento)
            .HasMaxLength(1000);

        builder.Property(t => t.CostoProcedimiento)
            .HasPrecision(18, 2);

        builder.Property(t => t.CostoInsumos)
            .HasPrecision(18, 2);

        builder.Property(t => t.CostoAdicional)
            .HasPrecision(18, 2);

        builder.Property(t => t.Subtotal)
            .HasPrecision(18, 2);

        builder.Property(t => t.Descuento)
            .HasPrecision(18, 2);

        builder.Property(t => t.IVA)
            .HasPrecision(18, 2);

        builder.Property(t => t.Total)
            .HasPrecision(18, 2);

        builder.Property(t => t.Estado)
            .HasConversion<int>();

        // Relaciones
        builder.HasOne(t => t.Cita)
            .WithMany()
            .HasForeignKey(t => t.CitaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Mascota)
            .WithMany()
            .HasForeignKey(t => t.MascotaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Cliente)
            .WithMany()
            .HasForeignKey(t => t.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Veterinario)
            .WithMany()
            .HasForeignKey(t => t.VeterinarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.EntregadoPor)
            .WithMany()
            .HasForeignKey(t => t.EntregadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Pago)
            .WithOne(p => p.Ticket)
            .HasForeignKey<Ticket>(t => t.PagoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Detalles)
            .WithOne(d => d.Ticket)
            .HasForeignKey(d => d.TicketId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
