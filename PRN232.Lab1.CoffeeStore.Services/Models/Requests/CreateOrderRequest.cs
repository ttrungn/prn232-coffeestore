using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PRN232.Lab1.CoffeeStore.Services.Models.Requests;

public class CreateOrderRequest : BaseServiceRequest
{
    [JsonIgnore] 
    public string? UserId { get; set; }
    [MinLength(1, ErrorMessage = "Order must contain at least 1 item")]
    private List<OrderItemRequest> _orderItems = [];

    public List<OrderItemRequest> OrderItems
    {
        get => _orderItems;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            var dupes = value
                .GroupBy(i => i.ProductId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (dupes.Count > 0)
                throw new ArgumentException(
                    $"Duplicate product(s) in OrderItems: {string.Join(", ", dupes)}",
                    nameof(OrderItems));

            _orderItems = value;
        }
    }
}

public class OrderItemRequest
{
    [Required]
    public Guid ProductId { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }
}
