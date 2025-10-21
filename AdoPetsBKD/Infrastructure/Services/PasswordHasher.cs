using AdoPetsBKD.Application.Interfaces.Services;
using System.Security.Cryptography;
using System.Text;

namespace AdoPetsBKD.Infrastructure.Services;

/// <summary>
/// Implementaci�n del servicio de hashing de contrase�as usando HMACSHA512
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    public void CreatePasswordHash(string password, out string passwordHash, out string passwordSalt)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("La contrase�a no puede estar vac�a", nameof(password));

        using var hmac = new HMACSHA512();
        
        passwordSalt = Convert.ToBase64String(hmac.Key);
        passwordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
    }

    public bool VerifyPassword(string password, string storedHash, string storedSalt)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("La contrase�a no puede estar vac�a", nameof(password));

        if (string.IsNullOrWhiteSpace(storedHash))
            throw new ArgumentException("El hash no puede estar vac�o", nameof(storedHash));

        if (string.IsNullOrWhiteSpace(storedSalt))
            throw new ArgumentException("El salt no puede estar vac�o", nameof(storedSalt));

        var saltBytes = Convert.FromBase64String(storedSalt);
        using var hmac = new HMACSHA512(saltBytes);
        
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        var storedHashBytes = Convert.FromBase64String(storedHash);

        return computedHash.SequenceEqual(storedHashBytes);
    }
}
