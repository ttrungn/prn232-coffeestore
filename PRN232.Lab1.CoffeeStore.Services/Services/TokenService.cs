using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PRN232.Lab1.CoffeeStore.Repositories.Interfaces;
using PRN232.Lab1.CoffeeStore.Repositories.Models;
using PRN232.Lab1.CoffeeStore.Services.Interfaces.Services;
using PRN232.Lab1.CoffeeStore.Services.Models;
using PRN232.Lab1.CoffeeStore.Services.Models.Requests;
using PRN232.Lab1.CoffeeStore.Services.Models.Responses;

namespace PRN232.Lab1.CoffeeStore.Services.Services;

public class TokenService : ITokenService
{
    private readonly UserManager<User> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtSettings _jwtSettings;

    public TokenService(UserManager<User> userManager, IUnitOfWork unitOfWork, IOptions<JwtSettings> jwtSettings)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<DataServiceResponse<TokenResponse>> GenerateTokens(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new DataServiceResponse<TokenResponse>()
            {
                Success = false,
                Message = "User not found",
                Data = null!
            };
        }
        
        var role = (await _userManager.GetRolesAsync(user))[0];
        var accessToken = GenerateAccessToken(user, role);
        var refreshToken = await GenerateRefreshToken(userId);

        var response = new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            AccessTokenExpiresAt = _jwtSettings.AccessTokenExpirationMinutes * 60,
            RefreshTokenExpiresAt = _jwtSettings.RefreshTokenExpirationDays * 24 * 60 * 60
        };

        return new DataServiceResponse<TokenResponse>()
        {
            Success = true,
            Message = "Tokens generated successfully",
            Data = response
        };
    }

    public async Task<DataServiceResponse<TokenResponse>> RefreshTokens(RefreshTokenRequest request)
    {
        var refreshTokenRepository = _unitOfWork.GetRepository<RefreshToken, Guid>();
        var refreshToken = await refreshTokenRepository.Query()
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

        if (refreshToken == null || !refreshToken.IsActive)
        {
            return new DataServiceResponse<TokenResponse>()
            {
                Success = false,
                Message = "Invalid or expired refresh token",
                Data = null!
            };
        }

        // Revoke the old refresh token
        refreshToken.IsRevoked = true;
        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.ReasonRevoked = "Replaced by new token";

        // Generate new tokens
        var user = await _userManager.FindByIdAsync(refreshToken.UserId);
        if (user == null)
        {
            return new DataServiceResponse<TokenResponse>()
            {
                Success = false,
                Message = "User not found",
                Data = null!
            };
        }
        
        var role = (await _userManager.GetRolesAsync(user))[0];
        var newAccessToken = GenerateAccessToken(refreshToken.User, role);
        var newRefreshToken = await GenerateRefreshToken(refreshToken.UserId);

        // Replace the old token reference
        refreshToken.ReplacedByToken = newRefreshToken.Token;

        await _unitOfWork.SaveChangesAsync();

        var response = new TokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            AccessTokenExpiresAt = _jwtSettings.AccessTokenExpirationMinutes * 60,
            RefreshTokenExpiresAt = _jwtSettings.RefreshTokenExpirationDays * 24 * 60 * 60
        };

        return new DataServiceResponse<TokenResponse>()
        {
            Success = true,
            Message = "Tokens refreshed successfully",
            Data = response
        };
    }

    public async Task<BaseServiceResponse> RevokeToken(RevokeRefreshTokenRequest request, string? revokedBy = null)
    {
        var token = request.RefreshToken;
        var refreshTokenRepository = _unitOfWork.GetRepository<RefreshToken, Guid>();
        var refreshToken = await refreshTokenRepository.Query()
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (refreshToken == null)
        {
            return new BaseServiceResponse()
            {
                Success = false,
                Message = "Token not found"
            };
        }

        if (refreshToken.IsRevoked)
        {
            return new BaseServiceResponse()
            {
                Success = false,
                Message = "Token already revoked"
            };
        }

        refreshToken.IsRevoked = true;
        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.RevokedBy = revokedBy;
        refreshToken.ReasonRevoked = "Manually revoked";
        await refreshTokenRepository.UpdateAsync(refreshToken);
        await _unitOfWork.SaveChangesAsync();

        return new BaseServiceResponse()
        {
            Success = true,
            Message = "Token revoked successfully"
        };
    }

    public async Task<BaseServiceResponse> RevokeAllUserTokens(string userId, string? revokedBy = null)
    {
        var refreshTokenRepository = _unitOfWork.GetRepository<RefreshToken, Guid>();
        var activeTokens = await refreshTokenRepository.Query()
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();

        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedBy = revokedBy;
            token.ReasonRevoked = "Bulk revocation";
        }

        await _unitOfWork.SaveChangesAsync();

        return new BaseServiceResponse()
        {
            Success = true,
            Message = $"Revoked {activeTokens.Count} tokens for user"
        };
    }

    public async Task<DataServiceResponse<bool>> ValidateRefreshToken(string token)
    {
        var refreshTokenRepository = _unitOfWork.GetRepository<RefreshToken, Guid>();
        var refreshToken = await refreshTokenRepository.Query()
            .FirstOrDefaultAsync(rt => rt.Token == token);

        var isValid = refreshToken != null && refreshToken.IsActive;
        return new DataServiceResponse<bool>()
        {
            Success = true,
            Message = isValid ? "Token is valid" : "Token is invalid",
            Data = isValid
        };
    }

    public async Task<BaseServiceResponse> CleanupExpiredTokens()
    {
        var refreshTokenRepository = _unitOfWork.GetRepository<RefreshToken, Guid>();
        var expiredTokens = await refreshTokenRepository.Query()
            .Where(rt => rt.ExpiresAt < DateTime.UtcNow)
            .ToListAsync();

        foreach (var token in expiredTokens)
        {
            // Mark as revoked instead of deleting
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.ReasonRevoked = "Expired - cleaned up";
        }

        await _unitOfWork.SaveChangesAsync();

        return new BaseServiceResponse()
        {
            Success = true,
            Message = $"Cleaned up {expiredTokens.Count} expired tokens"
        };
    }

    private string GenerateAccessToken(User user, string role)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? ""),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(ClaimTypes.Role, role),
            new Claim("firstName", user.FirstName),
            new Claim("lastName", user.LastName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private async Task<RefreshToken> GenerateRefreshToken(string userId)
    {
        var refreshTokenRepository = _unitOfWork.GetRepository<RefreshToken, Guid>();
        
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = GenerateRandomToken(),
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        await refreshTokenRepository.AddAsync(refreshToken);
        await _unitOfWork.SaveChangesAsync();

        return refreshToken;
    }

    private static string GenerateRandomToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var randomBytes = new byte[64];
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
