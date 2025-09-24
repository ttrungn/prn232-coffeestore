namespace PRN232.Lab1.CoffeeStore.Services.Models.Responses;

public class TokenResponse
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public int AccessTokenExpiresAt { get; set; }
    public int RefreshTokenExpiresAt { get; set; }
}
