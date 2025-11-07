namespace AdoPetsBKD.Application.Interfaces.Services;

/// <summary>
/// Interfaz para validación de tokens de Firebase
/// </summary>
public interface IFirebaseAuthService
{
    /// <summary>
    /// Valida un ID Token de Firebase y retorna el UID y email del usuario
    /// </summary>
    /// <param name="idToken">Token de Firebase ID</param>
    /// <returns>Tupla con (UID, Email, DisplayName)</returns>
    Task<(string Uid, string Email, string? DisplayName)> ValidateFirebaseTokenAsync(string idToken);
}
