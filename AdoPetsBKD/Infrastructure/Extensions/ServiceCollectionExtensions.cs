using AdoPetsBKD.Application.Interfaces.Repositories;
using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Infrastructure.Repositories;
using AdoPetsBKD.Infrastructure.Services;

namespace AdoPetsBKD.Infrastructure.Extensions;

/// <summary>
/// Extensiones para configurar la inyecci�n de dependencias
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IRolRepository, RolRepository>();

        services.AddScoped<IUMascotaRepositoty, MascotaRepository>();

        // Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddSingleton<ITokenService, TokenService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        
        // Servicios de Cl�nica
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<ISolicitudCitaDigitalService, SolicitudCitaDigitalService>();
        services.AddScoped<IPagoService, PagoService>();

        // Servicios de Empleados 
        services.AddScoped<IEmpleadoService, EmpleadoService>();
        services.AddScoped<IEmpleadoRepository, EmpleadoRepository>();

        // Servicios de Mascotas
        services.AddScoped<IUMascotaService, MascotaService>();
        return services;
    }
}
