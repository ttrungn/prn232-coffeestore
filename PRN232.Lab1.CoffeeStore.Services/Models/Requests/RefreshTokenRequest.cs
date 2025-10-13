using System.ComponentModel.DataAnnotations;

namespace PRN232.Lab1.CoffeeStore.Services.Models.Requests;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = null!;
}
