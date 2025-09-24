using System.ComponentModel.DataAnnotations;

namespace PRN232.Lab1.CoffeeStore.Services.Models.Requests;

public class LoginRequest : BaseServiceRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
    
    [Required]
    public string Password { get; set; } = null!;
    
    public bool RememberMe { get; set; } = false;
}
