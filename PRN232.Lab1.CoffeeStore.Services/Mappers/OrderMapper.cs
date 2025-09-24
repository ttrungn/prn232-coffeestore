using PRN232.Lab1.CoffeeStore.Repositories.Enums;
using PRN232.Lab1.CoffeeStore.Repositories.Models;
using PRN232.Lab1.CoffeeStore.Services.Models.Requests;
using PRN232.Lab1.CoffeeStore.Services.Models.Responses;

namespace PRN232.Lab1.CoffeeStore.Services.Mappers;

public static class OrderMapper
{
    public static Order ToOrder(this CreateOrderRequest request)
    {
        return new Order
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Editing,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static OrderResponse ToOrderResponse(this Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            UserId = order.UserId,
            OrderDate = order.OrderDate,
            Status = order.Status.ToString(),
            TotalAmount = order.OrderDetails.Sum(od => od.Quantity * od.UnitPrice),
            TotalItems = order.OrderDetails.Sum(od => od.Quantity),
            PaymentId = order.PaymentId
        };
    }

    public static OrderDetailsResponse ToOrderDetailsResponse(this Order order)
    {
        return new OrderDetailsResponse
        {
            Id = order.Id,
            UserId = order.UserId,
            OrderDate = order.OrderDate,
            Status = order.Status.ToString(),
            TotalAmount = order.OrderDetails.Sum(od => od.Quantity * od.UnitPrice),
            Payment = order.Payment.ToPaymentResponse(),
            OrderItems = order.OrderDetails.Select(od => od.ToOrderItemResponse()).ToList()
        };
    }

    public static OrderItemResponse ToOrderItemResponse(this OrderDetail orderDetail)
    {
        return new OrderItemResponse
        {
            Id = orderDetail.Id,
            ProductId = orderDetail.ProductId,
            ProductName = orderDetail.Product?.Name ?? "",
            Quantity = orderDetail.Quantity,
            UnitPrice = orderDetail.UnitPrice,
            TotalPrice = orderDetail.Quantity * orderDetail.UnitPrice
        };
    }

    public static void UpdateOrder(this Order order, UpdateOrderRequest request)
    {
        order.UpdatedAt = DateTime.UtcNow;
    }
}
