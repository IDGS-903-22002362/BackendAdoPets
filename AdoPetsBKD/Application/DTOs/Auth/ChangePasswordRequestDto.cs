using System.ComponentModel.DataAnnotations;

namespace AdoPetsBKD.Application.DTOs.Auth;

/// <summary>
/// DTO para cambio de contrase�a
/// </summary>
public class ChangePasswordRequestDto
{
    [Required(ErrorMessage = "La contrase�a actual es requerida")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nueva contrase�a es requerida")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contrase�a debe tener entre 8 y 100 caracteres")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#])[A-Za-z\d@$!%*?&#]{8,}$", 
        ErrorMessage = "La contrase�a debe contener al menos una may�scula, una min�scula, un n�mero y un car�cter especial")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "La confirmaci�n de contrase�a es requerida")]
    [Compare("NewPassword", ErrorMessage = "Las contrase�as no coinciden")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
