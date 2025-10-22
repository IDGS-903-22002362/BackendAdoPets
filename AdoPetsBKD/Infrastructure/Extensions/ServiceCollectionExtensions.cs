using AdoPetsBKD.Application.Interfaces.Repositories;
using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Infrastructure.Repositories;
using AdoPetsBKD.Infrastructure.Services;

namespace AdoPetsBKD.Infrastructure.Extensions;

/// <summary>
/// Extensiones para configurar la inyección de dependencias
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IRolRepository, RolRepository>();

        // Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddSingleton<ITokenService, TokenService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        
        // Servicios de Clínica
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<ISolicitudCitaDigitalService, SolicitudCitaDigitalService>();
        services.AddScoped<IPagoService, PagoService>();

        return services;
    }
}
