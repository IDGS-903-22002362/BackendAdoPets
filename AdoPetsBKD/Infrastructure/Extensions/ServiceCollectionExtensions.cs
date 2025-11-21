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
        services.AddScoped<IUMascotaRepositoty, MascotaRepository>();
        services.AddScoped<IEspecialidadRepositoy, EspecialidadRepository>();
        services.AddScoped<IProveedorRepository, ProveedorRepository>();
        services.AddScoped<INotificacionRepository, NotificacionRepository>();
        services.AddScoped<IDispositivoRepository, DispositivoRepository>();
        services.AddScoped<ICitaRepository, CitaRepository>();
        services.AddScoped<ISalaRepository, SalaRepository>();
        services.AddScoped<ISolicitudCitaDigitalRepository, SolicitudCitaDigitalRepository>();

        // Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddSingleton<ITokenService, TokenService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IFirebaseAuthService, FirebaseAuthService>();
        
        // Servicios de Clínica
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<ISolicitudCitaDigitalService, SolicitudCitaDigitalService>();
        services.AddScoped<IPagoService, PagoService>();
        services.AddScoped<IServicioService, ServicioService>();
        services.AddScoped<ISalaService, SalaService>();
    
        // Servicio de PayPal
        services.AddScoped<IPayPalClient, PayPalClient>();

        // Servicios de Empleados 
        services.AddScoped<IEmpleadoService, EmpleadoService>();
        services.AddScoped<IEmpleadoRepository, EmpleadoRepository>();
        services.AddScoped<IEspecialidadService, EspecialidadService>();

        // Servicios de Mascotas
        services.AddScoped<IUMascotaService, MascotaService>();
        
        // Servicio de Mascotas de Usuario
        services.AddScoped<IMascotaUsuarioService, MascotaUsuarioService>();
        
        // Servicio de Proveedores
        services.AddScoped<IProveedorService, ProveedorService>();
       
        // Servicios de Recordatorios y Notificaciones
        services.AddScoped<IRecordatorioService, RecordatorioService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IPushNotificationService, PushNotificationService>();

        // Servicios de Horarios 
        services.AddScoped<IHorarioService, HorarioService>(); 
        services.AddScoped<IHorarioRepositoy, HorarioRepository>();

        return services;
    }
}
