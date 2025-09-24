using Microsoft.AspNetCore.Http;
using PRN232.Lab1.CoffeeStore.Services.Models.Responses;
using VNPAY.NET.Models;

namespace PRN232.Lab1.CoffeeStore.Services.Interfaces.Services;

public interface IVnPayService
{
    Task<DataServiceResponse<string>> GetPaymentUrl(Guid orderId);
    Task<BaseServiceResponse> ProcessIpnAction(IQueryCollection queryParams);
}