using AdoPetsBKD.Infrastructure.Configuration;
using AdoPetsBKD.Infrastructure.Data;
using AdoPetsBKD.Infrastructure.Data.Seeders;
using AdoPetsBKD.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

// Hangfire y el servicio de recordatorios
using Hangfire;
using Hangfire.SqlServer;
using AdoPetsBKD.Application.Interfaces.Services;

// --- CORRECCIÓN: Crear wwwroot antes de inicializar la aplicación ---
var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
if (!Directory.Exists(wwwrootPath))
{
    Directory.CreateDirectory(wwwrootPath);
}

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURACIÓN DE SETTINGS ---
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("JWT settings not configured");
var corsSettings = builder.Configuration.GetSection(CorsSettings.SectionName).Get<CorsSettings>()
    ?? throw new InvalidOperationException("CORS settings not configured");

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.Configure<CorsSettings>(builder.Configuration.GetSection(CorsSettings.SectionName));
builder.Services.Configure<AzureBlobSettings>(builder.Configuration.GetSection(AzureBlobSettings.SectionName));
builder.Services.Configure<PayPalSettings>(builder.Configuration.GetSection(PayPalSettings.SectionName));
builder.Services.Configure<FirebaseSettings>(builder.Configuration.GetSection(FirebaseSettings.SectionName));
builder.Services.Configure<PoliciesSettings>(builder.Configuration.GetSection(PoliciesSettings.SectionName));

// --- 2. BASE DE DATOS ---
builder.Services.AddDbContext<AdoPetsDbContext>(options =>
{
    // Asegúrate de tener la ConnectionString "AdoPetsDb" en Azure Portal
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("AdoPetsDb"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null);
            sqlOptions.MigrationsAssembly(typeof(AdoPetsDbContext).Assembly.FullName);
        });

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// --- 3. HANGFIRE ---
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(
        builder.Configuration.GetConnectionString("AdoPetsDb"),
        new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true,
            SchemaName = "Hangfire"
        }));

builder.Services.AddHangfireServer(options =>
{
    options.SchedulePollingInterval = TimeSpan.FromMinutes(1);
    options.ServerName = $"AdoPets-{Environment.MachineName}";
});

// --- 4. GROQ AI (HTTP CLIENT) ---
// Usa la variable de entorno GROQ_API_KEY (configurada en Windows y en Azure).
var groqApiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY");

if (!string.IsNullOrWhiteSpace(groqApiKey))
{
    builder.Services.AddHttpClient("GroqClient", client =>
    {
        client.BaseAddress = new Uri("https://api.groq.com/openai/v1/");
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {groqApiKey}");
    });
}
else
{
    Console.WriteLine("⚠️ Advertencia: la variable de entorno GROQ_API_KEY no está configurada. El cliente de Groq no tendrá autenticación.");
}

// --- 5. SERVICIOS DE APLICACIÓN ---
builder.Services.AddApplicationServices();

// --- 6. CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsSettings.PolicyName, policy =>
    {
        // Usar los orígenes configurados tanto en desarrollo como en producción
        policy
            .WithOrigins(corsSettings.AllowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// --- 7. AUTH ---
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("VetOnly", policy => policy.RequireRole("Veterinario", "Admin"));
    options.AddPolicy("StaffOnly", policy => policy.RequireRole("Admin", "Veterinario", "Recepcionista", "Asistente"));
});

// Controllers
builder.Services.AddControllers();

// --- 8. SWAGGER ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AdoPets API",
        Version = "v1",
        Description = "API para la gestión integral de refugio de animales"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese 'Bearer' [espacio] y luego su token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ================= PIPELINE DE PETICIONES =================

// --- CORRECCIÓN 1: Manejo seguro de archivos estáticos ---
// Verificamos si existe la carpeta uploads, si no, la creamos
var uploadsPath = Path.Combine(wwwrootPath, "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(); // Para wwwroot base
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

// --- CORRECCIÓN 2: Swagger y Hangfire visibles en Producción ---
// Quitamos el "if (IsDevelopment)" para que puedas probar en Azure.

app.UseSwagger(c =>
{
    c.RouteTemplate = "v1/{documentName}/swagger.json";
});

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/v1/v1/swagger.json", "AdoPets API v1");
    app.Logger.LogInformation("Swagger UI inicializado en la raíz.");
    c.RoutePrefix = string.Empty; // Swagger aparecerá en la raíz del sitio
});

// Dashboard de Hangfire
// Nota: En producción, Hangfire bloquea accesos remotos por defecto. 
// Si ves un 403 Forbidden, es normal por seguridad.
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    DashboardTitle = "AdoPets - Jobs Programados",
    DisplayStorageConnectionString = false
});

app.UseCors(corsSettings.PolicyName);

// Solo redirigir a HTTPS en producción
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// --- CORRECCIÓN 3: Migraciones automáticas en Azure ---
// Ejecutamos esto siempre (con try-catch) para asegurar que la BD exista en la nube.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<AdoPetsDbContext>();

        // Solo intentamos conectar/migrar si tenemos conexión.
        // Esto crea la BD en Azure si es la primera vez.
        await dbContext.Database.MigrateAsync();

        // Seed de datos
        await DatabaseSeeder.SeedAllAsync(dbContext);

        app.Logger.LogInformation("✅ Base de Datos migrada y actualizada correctamente.");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "❌ Ocurrió un error al migrar la base de datos.");
        // No lanzamos la excepción (throw) para permitir que la App arranque 
        // y puedas ver al menos el Swagger, aunque la BD falle.
    }
}

// Programar job recurrente de recordatorios
try
{
    RecurringJob.AddOrUpdate<IRecordatorioService>(
        "enviar-recordatorios-citas",
        service => service.EnviarRecordatoriosPendientesAsync(),
        "*/15 * * * *",
        new RecurringJobOptions
        {
            TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)")
        });
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "❌ Error al registrar Jobs de Hangfire (¿Quizás faltó la conexión a BD?)");
}

app.Run();
