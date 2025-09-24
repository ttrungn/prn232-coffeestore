namespace PRN232.Lab1.CoffeeStore.Services.Models.Responses;

public class OrderResponse
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = null!;
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public int TotalItems { get; set; }
    public Guid? PaymentId { get; set; }
}
