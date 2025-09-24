using PRN232.Lab1.CoffeeStore.Services.Models.Requests;
using PRN232.Lab1.CoffeeStore.Services.Models.Responses;

namespace PRN232.Lab1.CoffeeStore.Services.Interfaces.Services;

public interface IAuthService
{
    Task<DataServiceResponse<TokenResponse>> Login(LoginRequest request);
    Task<BaseServiceResponse> Register(RegisterRequest request, string role);
}
