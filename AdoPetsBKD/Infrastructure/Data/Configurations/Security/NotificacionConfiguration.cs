using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AdoPetsBKD.Domain.Entities.Security;

namespace AdoPetsBKD.Infrastructure.Data.Configurations.Security;

public class NotificacionConfiguration : IEntityTypeConfiguration<Notificacion>
{
    public void Configure(EntityTypeBuilder<Notificacion> builder)
    {
        builder.ToTable("Notificaciones");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Titulo)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(n => n.Mensaje)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(n => n.DataJson)
            .HasColumnType("nvarchar(max)");

        // Índices
        builder.HasIndex(n => new { n.UsuarioId, n.Fecha })
            .HasDatabaseName("IX_Notif_UsuarioId_Fecha");

        builder.HasIndex(n => new { n.Tipo, n.Fecha })
            .HasDatabaseName("IX_Notif_Tipo_Fecha");

        // Relaciones
        builder.HasOne(n => n.Usuario)
            .WithMany(u => u.Notificaciones)
            .HasForeignKey(n => n.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignorar propiedades computadas
        builder.Ignore(n => n.IsRead);
    }
}
