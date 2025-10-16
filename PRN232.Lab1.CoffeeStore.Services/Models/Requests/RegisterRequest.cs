using System.ComponentModel.DataAnnotations;

namespace PRN232.Lab1.CoffeeStore.Services.Models.Requests;

public class RegisterRequest : BaseServiceRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
    
    [Required]
    [StringLength(50)]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username chỉ được chứa ký tự và số")]
    public string Username { get; set; }
    
    [Required]
    [MinLength(6)]
    public string Password { get; set; } = null!;
    
    [Required]
    [Compare("Password")]
    public string ConfirmPassword { get; set; } = null!;
    
    [Required]
    public string FirstName { get; set; } = null!;
    
    [Required]
    public string LastName { get; set; } = null!;
    
    public string? PhoneNumber { get; set; }
}
