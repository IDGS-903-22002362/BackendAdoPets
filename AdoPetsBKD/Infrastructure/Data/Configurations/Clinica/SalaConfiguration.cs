using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AdoPetsBKD.Domain.Entities.Clinica;

namespace AdoPetsBKD.Infrastructure.Data.Configurations.Clinica;

public class SalaConfiguration : IEntityTypeConfiguration<Sala>
{
    public void Configure(EntityTypeBuilder<Sala> builder)
    {
        builder.ToTable("Salas");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Descripcion)
            .HasMaxLength(500);

        // Índice
        builder.HasIndex(s => s.Nombre)
            .IsUnique()
            .HasDatabaseName("UX_Sala_Nombre");

        // Relaciones
        builder.HasMany(s => s.Citas)
            .WithOne(c => c.Sala)
            .HasForeignKey(c => c.SalaId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
