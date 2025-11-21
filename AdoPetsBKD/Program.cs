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

var builder = WebApplication.CreateBuilder(args);

// Configuración de Settings
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

// Database Context
builder.Services.AddDbContext<AdoPetsDbContext>(options =>
{
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

// Configurar Hangfire 
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

// Application Services (Repositories & Services)
builder.Services.AddApplicationServices();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsSettings.PolicyName, policy =>
    {
        policy.WithOrigins(corsSettings.AllowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Authentication & Authorization (JWT)
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

// OpenAPI/Swagger
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

// En Program.cs o Startup.cs
app.UseStaticFiles(); // Para wwwroot
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads")),
    RequestPath = "/uploads"
});

// Configure HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "v1/{documentName}/swagger.json";
    });

    // Dashboard de Hangfire (después de UseSwagger)
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        DashboardTitle = "AdoPets - Jobs Programados",
        DisplayStorageConnectionString = false
    });

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/v1/v1/swagger.json", "AdoPets API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseCors(corsSettings.PolicyName);

// Solo redirigir a HTTPS en producción para evitar problemas con CORS
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Inicialización de BD en desarrollo
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AdoPetsDbContext>();

    try
    {
        if (await dbContext.Database.CanConnectAsync())
        {
            app.Logger.LogInformation("✅ Conexión a BD exitosa");
            await dbContext.Database.MigrateAsync();

            // Inicializar datos básicos
            await DatabaseSeeder.SeedAllAsync(dbContext);
            app.Logger.LogInformation("✅ Datos iniciales cargados");
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "❌ Error al inicializar BD");
    }
}

// Programar job recurrente de recordatorios (antes de app.Run)
RecurringJob.AddOrUpdate<IRecordatorioService>(
    "enviar-recordatorios-citas",
    service => service.EnviarRecordatoriosPendientesAsync(),
    "*/15 * * * *", // Cada 15 minutos
    new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)")
    });

app.Logger.LogInformation("Job de recordatorios programado: cada 15 minutos");
app.Logger.LogInformation("Dashboard de Hangfire disponible en: /hangfire");

app.Run();
