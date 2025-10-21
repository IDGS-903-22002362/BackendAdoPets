using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AdoPetsBKD.Domain.Entities.Security;

namespace AdoPetsBKD.Infrastructure.Data.Configurations.Security;

public class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Nombre)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.Descripcion)
            .HasMaxLength(255);

        // Índices
        builder.HasIndex(r => r.Nombre)
            .IsUnique()
            .HasDatabaseName("UX_Rol_Nombre");

        // Datos semilla - Usar GUIDs estáticos
        builder.HasData(
            new Rol { Id = new Guid("11111111-1111-1111-1111-111111111111"), Nombre = Roles.Admin, Descripcion = "Administrador del sistema" },
            new Rol { Id = new Guid("22222222-2222-2222-2222-222222222222"), Nombre = Roles.Veterinario, Descripcion = "Veterinario" },
            new Rol { Id = new Guid("33333333-3333-3333-3333-333333333333"), Nombre = Roles.Recepcionista, Descripcion = "Recepcionista" },
            new Rol { Id = new Guid("44444444-4444-4444-4444-444444444444"), Nombre = Roles.Asistente, Descripcion = "Asistente del refugio" },
            new Rol { Id = new Guid("55555555-5555-5555-5555-555555555555"), Nombre = Roles.Adoptante, Descripcion = "Usuario adoptante" }
        );
    }
}
