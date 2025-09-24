using PRN232.Lab1.CoffeeStore.Repositories.Models;

namespace PRN232.Lab1.CoffeeStore.Services.Models.Responses;

public class ProductInMenuResponse
{
    public Guid ProductInMenuId { get; set; }
    public ProductResponse Product { get; set; } = null!;
    public int Quantity { get; set; }
}