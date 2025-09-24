using Microsoft.AspNetCore.Identity;
using PRN232.Lab1.CoffeeStore.Repositories.Constants;
using PRN232.Lab1.CoffeeStore.Repositories.Models;
using PRN232.Lab1.CoffeeStore.Services.Interfaces.Services;
using PRN232.Lab1.CoffeeStore.Services.Models.Requests;
using PRN232.Lab1.CoffeeStore.Services.Models.Responses;

namespace PRN232.Lab1.CoffeeStore.Services.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ITokenService _tokenService;

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
    }

    public async Task<DataServiceResponse<TokenResponse>> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return new DataServiceResponse<TokenResponse>()
            {
                Success = false,
                Message = "Invalid email or password",
                Data = null!
            };
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!signInResult.Succeeded)
        {
            return new DataServiceResponse<TokenResponse>()
            {
                Success = false,
                Message = "Invalid email or password",
                Data = null!
            };
        }

        if (!user.EmailConfirmed)
        {
            return new DataServiceResponse<TokenResponse>()
            {
                Success = false,
                Message = "Email not confirmed",
                Data = null!
            };
        }
        
        // Generate tokens using TokenService
        var tokenResponse = await _tokenService.GenerateTokens(user.Id);
        if (!tokenResponse.Success)
        {
            return new DataServiceResponse<TokenResponse>()
            {
                Success = false,
                Message = "Failed to generate tokens",
                Data = null!
            };
        }

        return new DataServiceResponse<TokenResponse>()
        {
            Success = true,
            Message = "Login successful",
            Data = tokenResponse.Data
        };
    }

    public async Task<BaseServiceResponse> Register(RegisterRequest request, string role)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return new BaseServiceResponse()
            {
                Success = false,
                Message = "User with this email already exists"
            };
        }

        // Validate role
        if (string.Equals(role, Roles.Admin, StringComparison.OrdinalIgnoreCase))
        {
            role = Roles.Admin;
        }
        else if (string.Equals(role, Roles.Customer, StringComparison.OrdinalIgnoreCase))
        {
            role = Roles.Customer;
        }
        else
        {
            return new BaseServiceResponse
            {
                Success = false,
                Message = "Invalid role specified"
            };            
        }

        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            return new BaseServiceResponse()
            {
                Success = false,
                Message = $"Registration failed: {errors}"
            };
        }

        // Add role to user
        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            // If role assignment fails, we should clean up the user
            var deleteResult = await _userManager.DeleteAsync(user);
            var roleErrors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            
            if (!deleteResult.Succeeded)
            {
                return new BaseServiceResponse()
                {
                    Success = false,
                    Message = $"Registration failed: Role assignment failed and user cleanup also failed. Please contact support."
                };
            }
            
            return new BaseServiceResponse()
            {
                Success = false,
                Message = $"Failed to assign role: {roleErrors}"
            };
        }

        return new BaseServiceResponse()
        {
            Success = true,
            Message = "Registration successful"
        };
    }
}
