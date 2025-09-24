using PRN232.Lab1.CoffeeStore.Repositories.Enums;

namespace PRN232.Lab1.CoffeeStore.Services.Models.Requests;

public class GetOrdersRequest : BaseServiceRequest
{
    public int Page { get; set; } = 0;
    public int PageSize { get; set; } = 10;
    public string? UserId { get; set; }
    public OrderStatus? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
