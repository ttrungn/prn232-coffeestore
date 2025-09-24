namespace PRN232.Lab1.CoffeeStore.Services.Models.Requests;

public class UpdateOrderRequest : BaseServiceRequest
{
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
