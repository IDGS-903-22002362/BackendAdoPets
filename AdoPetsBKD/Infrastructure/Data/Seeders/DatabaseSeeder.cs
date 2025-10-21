using AdoPetsBKD.Domain.Entities.Security;
using AdoPetsBKD.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdoPetsBKD.Infrastructure.Data.Seeders;

/// <summary>
/// Seeder para inicializar datos básicos en la base de datos
/// </summary>
public static class DatabaseSeeder
{
    /// <summary>
    /// Inicializa los roles del sistema
    /// </summary>
    public static async Task SeedRolesAsync(AdoPetsDbContext context)
    {
        if (await context.Roles.AnyAsync())
        {
            return; // Ya existen roles
        }

        var roles = new List<Rol>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Nombre = Roles.Admin,
                Descripcion = "Administrador del sistema con acceso completo"
            },
            new()
            {
                Id = Guid.NewGuid(),
                Nombre = Roles.Veterinario,
                Descripcion = "Veterinario encargado de la atención médica"
            },
            new()
            {
                Id = Guid.NewGuid(),
                Nombre = Roles.Recepcionista,
                Descripcion = "Recepcionista para gestión de citas y mascotas"
            },
            new()
            {
                Id = Guid.NewGuid(),
                Nombre = Roles.Asistente,
                Descripcion = "Asistente para soporte operativo"
            },
            new()
            {
                Id = Guid.NewGuid(),
                Nombre = Roles.Adoptante,
                Descripcion = "Usuario que puede adoptar mascotas"
            }
        };

        await context.Roles.AddRangeAsync(roles);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Inicializa un usuario administrador por defecto
    /// </summary>
    public static async Task SeedAdminUserAsync(AdoPetsDbContext context)
    {
        if (await context.Usuarios.AnyAsync(u => u.Email == "admin@adopets.com"))
        {
            return; // Ya existe el admin
        }

        var adminRol = await context.Roles.FirstOrDefaultAsync(r => r.Nombre == Roles.Admin);
        if (adminRol == null)
        {
            throw new InvalidOperationException("El rol Admin debe existir antes de crear el usuario administrador");
        }

        // Contraseña: Admin123!
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nombre = "Administrador",
            ApellidoPaterno = "Sistema",
            ApellidoMaterno = "",
            Email = "admin@adopets.com",
            Telefono = "5550000000",
            // Hash y Salt generados para la contraseña "Admin123!"
            PasswordHash = "vR8aqEfGkBz5ORhGJwqFPvNKqXqX0HqPJqL8RyXqE5Y=",
            PasswordSalt = "base64salt==",
            Estatus = EstatusUsuario.Activo,
            AceptoPoliticasVersion = "1.0.0",
            AceptoPoliticasAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        usuario.UsuarioRoles.Add(new UsuarioRol
        {
            UsuarioId = usuario.Id,
            RolId = adminRol.Id
        });

        await context.Usuarios.AddAsync(usuario);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Ejecuta todos los seeders
    /// </summary>
    public static async Task SeedAllAsync(AdoPetsDbContext context)
    {
        await SeedRolesAsync(context);
        await SeedAdminUserAsync(context);
    }
}
