using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Infrastructure.Configuration;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AdoPetsBKD.Infrastructure.Services;

/// <summary>
/// Servicio para validación de tokens de Firebase Authentication
/// </summary>
public class FirebaseAuthService : IFirebaseAuthService
{
    private readonly ILogger<FirebaseAuthService> _logger;
    private readonly FirebaseSettings _firebaseSettings;
    private readonly FirebaseApp? _firebaseApp;

    public FirebaseAuthService(
        ILogger<FirebaseAuthService> logger,
        IOptions<FirebaseSettings> firebaseSettings)
    {
        _logger = logger;
        _firebaseSettings = firebaseSettings.Value;

        try
        {
            // Inicializar Firebase Admin SDK si no está inicializado
            if (FirebaseApp.DefaultInstance == null)
            {
                _firebaseApp = InitializeFirebaseApp();
            }
            else
            {
                _firebaseApp = FirebaseApp.DefaultInstance;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al inicializar Firebase Admin SDK");
        }
    }

    public async Task<(string Uid, string Email, string? DisplayName)> ValidateFirebaseTokenAsync(string idToken)
    {
        try
        {
            if (_firebaseApp == null)
            {
                throw new InvalidOperationException("Firebase no está inicializado correctamente");
            }

            // Verificar el token con Firebase
            FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);

            // Extraer información del usuario
            string uid = decodedToken.Uid;
            string? email = decodedToken.Claims.TryGetValue("email", out var emailObj) 
                ? emailObj?.ToString() 
                : null;
            string? displayName = decodedToken.Claims.TryGetValue("name", out var nameObj) 
                ? nameObj?.ToString() 
                : null;

            if (string.IsNullOrEmpty(email))
            {
                throw new InvalidOperationException("El token de Firebase no contiene un email válido");
            }

            _logger.LogInformation("Token de Firebase validado exitosamente para UID: {Uid}", uid);

            return (uid, email, displayName);
        }
        catch (FirebaseAuthException ex)
        {
            _logger.LogWarning(ex, "Token de Firebase inválido o expirado");
            throw new UnauthorizedAccessException("Token de Firebase inválido o expirado", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar token de Firebase");
            throw;
        }
    }

    private FirebaseApp? InitializeFirebaseApp()
    {
        try
        {
            // Verificar que las credenciales estén configuradas
            if (string.IsNullOrWhiteSpace(_firebaseSettings.ProjectId))
            {
                _logger.LogWarning("Firebase ProjectId no configurado. Firebase Auth estará deshabilitado.");
                return null;
            }

            // Crear credenciales desde los settings
            var credential = GoogleCredential.FromJson($@"{{
                ""type"": ""service_account"",
                ""project_id"": ""{_firebaseSettings.ProjectId}"",
                ""private_key"": ""{_firebaseSettings.PrivateKey.Replace("\\n", "\n")}"",
                ""client_email"": ""{_firebaseSettings.ClientEmail}""
            }}");

            var app = FirebaseApp.Create(new AppOptions
            {
                Credential = credential,
                ProjectId = _firebaseSettings.ProjectId
            });

            _logger.LogInformation("Firebase Admin SDK inicializado correctamente");
            return app;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear credenciales de Firebase");
            return null;
        }
    }
}
