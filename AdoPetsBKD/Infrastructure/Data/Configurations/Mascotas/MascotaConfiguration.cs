using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AdoPetsBKD.Domain.Entities.Mascotas;

namespace AdoPetsBKD.Infrastructure.Data.Configurations.Mascotas;

public class MascotaConfiguration : IEntityTypeConfiguration<Mascota>
{
    public void Configure(EntityTypeBuilder<Mascota> builder)
    {
        builder.ToTable("Mascotas");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.Especie)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(m => m.Raza)
            .HasMaxLength(100);

        builder.Property(m => m.Personalidad)
            .HasMaxLength(500);

        builder.Property(m => m.EstadoSalud)
            .HasMaxLength(500);

        builder.Property(m => m.RequisitoAdopcion)
            .HasMaxLength(1000);

        builder.Property(m => m.Origen)
            .HasMaxLength(255);

        builder.Property(m => m.Notas)
            .HasMaxLength(2000);

        // Índices
        builder.HasIndex(m => m.Estatus)
            .HasDatabaseName("IX_Mascota_Estatus");

        builder.HasIndex(m => new { m.Especie, m.Estatus })
            .HasDatabaseName("IX_Mascota_Especie_Estatus");

        builder.HasIndex(m => m.DeletedAt)
            .HasDatabaseName("IX_Mascota_DeletedAt");

        // Relaciones
        builder.HasMany(m => m.Fotos)
            .WithOne(f => f.Mascota)
            .HasForeignKey(f => f.MascotaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.SolicitudesAdopcion)
            .WithOne(s => s.Mascota)
            .HasForeignKey(s => s.MascotaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ignorar propiedades computadas
        builder.Ignore(m => m.EdadEnMeses);
        builder.Ignore(m => m.IsDeleted);
    }
}
