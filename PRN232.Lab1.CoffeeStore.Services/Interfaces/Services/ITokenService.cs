using PRN232.Lab1.CoffeeStore.Services.Models.Requests;
using PRN232.Lab1.CoffeeStore.Services.Models.Responses;

namespace PRN232.Lab1.CoffeeStore.Services.Interfaces.Services;

public interface ITokenService
{
    Task<DataServiceResponse<TokenResponse>> GenerateTokens(string userId);
    Task<DataServiceResponse<TokenResponse>> RefreshTokens(RefreshTokenRequest request);
    Task<BaseServiceResponse> RevokeToken(RevokeRefreshTokenRequest request, string? revokedBy = null);
    Task<BaseServiceResponse> RevokeAllUserTokens(string userId, string? revokedBy = null);
    Task<DataServiceResponse<bool>> ValidateRefreshToken(string token);
    Task<BaseServiceResponse> CleanupExpiredTokens();
}
