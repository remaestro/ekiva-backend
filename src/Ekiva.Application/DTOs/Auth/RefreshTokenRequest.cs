using System.ComponentModel.DataAnnotations;

namespace Ekiva.Application.DTOs.Auth;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
