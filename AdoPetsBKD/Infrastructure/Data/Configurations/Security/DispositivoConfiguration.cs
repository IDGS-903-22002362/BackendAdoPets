using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AdoPetsBKD.Domain.Entities.Security;

namespace AdoPetsBKD.Infrastructure.Data.Configurations.Security;

public class DispositivoConfiguration : IEntityTypeConfiguration<Dispositivo>
{
    public void Configure(EntityTypeBuilder<Dispositivo> builder)
    {
        builder.ToTable("Dispositivos");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Token)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(d => d.AppVersion)
            .HasMaxLength(50);

        // Índices
        builder.HasIndex(d => d.Token)
            .IsUnique()
            .HasDatabaseName("UX_Dispositivo_Token");

        builder.HasIndex(d => d.UsuarioId)
            .HasDatabaseName("IX_Dispositivo_UsuarioId");

        // Relaciones
        builder.HasOne(d => d.Usuario)
            .WithMany(u => u.Dispositivos)
            .HasForeignKey(d => d.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
