using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AdoPetsBKD.Domain.Entities.Security;

namespace AdoPetsBKD.Infrastructure.Data.Configurations.Security;

public class ConsentimientoPoliticaConfiguration : IEntityTypeConfiguration<ConsentimientoPolitica>
{
    public void Configure(EntityTypeBuilder<ConsentimientoPolitica> builder)
    {
        builder.ToTable("ConsentimientosPoliticas");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Version)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Ip)
            .HasMaxLength(45);

        builder.Property(c => c.UserAgent)
            .HasMaxLength(500);

        // Índices
        builder.HasIndex(c => new { c.UsuarioId, c.Version })
            .HasDatabaseName("IX_Consentimiento_UsuarioId_Version");

        // Relaciones
        builder.HasOne(c => c.Usuario)
            .WithMany(u => u.Consentimientos)
            .HasForeignKey(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
