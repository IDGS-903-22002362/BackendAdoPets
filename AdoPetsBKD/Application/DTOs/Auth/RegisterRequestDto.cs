using System.ComponentModel.DataAnnotations;

namespace AdoPetsBKD.Application.DTOs.Auth;

/// <summary>
/// DTO para registro de nuevo usuario
/// </summary>
public class RegisterRequestDto
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido paterno es requerido")]
    [StringLength(100, ErrorMessage = "El apellido paterno no puede exceder 100 caracteres")]
    public string ApellidoPaterno { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "El apellido materno no puede exceder 100 caracteres")]
    public string ApellidoMaterno { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "El formato del email no es v�lido")]
    [StringLength(255, ErrorMessage = "El email no puede exceder 255 caracteres")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "El formato del tel�fono no es v�lido")]
    [StringLength(20, ErrorMessage = "El tel�fono no puede exceder 20 caracteres")]
    public string? Telefono { get; set; }

    [Required(ErrorMessage = "La contrase�a es requerida")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contrase�a debe tener entre 8 y 100 caracteres")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#])[A-Za-z\d@$!%*?&#]{8,}$", 
        ErrorMessage = "La contrase�a debe contener al menos una may�scula, una min�scula, un n�mero y un car�cter especial")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "La confirmaci�n de contrase�a es requerida")]
    [Compare("Password", ErrorMessage = "Las contrase�as no coinciden")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public bool AceptaPoliticas { get; set; } = false;
}
