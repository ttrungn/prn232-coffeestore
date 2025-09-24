using VNPAY.NET.Models;

namespace PRN232.Lab1.CoffeeStore.Services.Models.Responses;

public class OrderDetailsResponse
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = null!;
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public PaymentResponse? Payment { get; set; }
    public List<OrderItemResponse> OrderItems { get; set; } = new List<OrderItemResponse>();
}

public class OrderItemResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
