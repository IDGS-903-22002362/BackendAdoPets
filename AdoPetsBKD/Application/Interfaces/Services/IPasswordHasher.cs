namespace AdoPetsBKD.Application.Interfaces.Services;

/// <summary>
/// Interfaz para el servicio de hashing de contraseñas
/// </summary>
public interface IPasswordHasher
{
    void CreatePasswordHash(string password, out string passwordHash, out string passwordSalt);
    bool VerifyPassword(string password, string storedHash, string storedSalt);
}
