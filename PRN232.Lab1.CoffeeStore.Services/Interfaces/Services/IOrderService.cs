using PRN232.Lab1.CoffeeStore.Repositories.Enums;
using PRN232.Lab1.CoffeeStore.Services.Models.Requests;
using PRN232.Lab1.CoffeeStore.Services.Models.Responses;

namespace PRN232.Lab1.CoffeeStore.Services.Interfaces.Services;

public interface IOrderService
{
    Task<DataServiceResponse<PaginationResponse<OrderResponse>>> GetOrders(GetOrdersRequest request);
    Task<DataServiceResponse<OrderDetailsResponse?>> GetOrderById(Guid orderId);
    Task<DataServiceResponse<Guid>> CreateOrder(CreateOrderRequest request);
    Task<BaseServiceResponse> UpdateOrder(Guid orderId, UpdateOrderRequest request);
    Task<BaseServiceResponse> UpdateOrderStatus(Guid orderId, OrderStatus status);
}
