using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AdoPetsBKD.Domain.Entities.Security;

namespace AdoPetsBKD.Infrastructure.Data.Configurations.Security;

public class PreferenciaNotificacionConfiguration : IEntityTypeConfiguration<PreferenciaNotificacion>
{
    public void Configure(EntityTypeBuilder<PreferenciaNotificacion> builder)
    {
        builder.ToTable("PreferenciasNotificaciones");

        builder.HasKey(p => p.Id);

        // Índice único para evitar duplicados
        builder.HasIndex(p => new { p.UsuarioId, p.Canal, p.Categoria })
            .IsUnique()
            .HasDatabaseName("UX_PreferenciaNotif_Usuario_Canal_Categoria");

        // Relaciones
        builder.HasOne(p => p.Usuario)
            .WithMany()
            .HasForeignKey(p => p.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
