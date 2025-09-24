using Microsoft.AspNetCore.Identity;

namespace PRN232.Lab1.CoffeeStore.Repositories.Models;

public class User : IdentityUser
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public ICollection<RefreshToken> RefreshTokens { get; set; } = null!;
}