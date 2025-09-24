using PRN232.Lab1.CoffeeStore.Repositories.Models;
using PRN232.Lab1.CoffeeStore.Services.Models.Responses;

namespace PRN232.Lab1.CoffeeStore.Services.Mappers;

public static class ProductInMenuMapper
{
    public static ProductInMenuResponse ToProductInMenuResponse(this ProductInMenu productInMenu)
    {
        return new ProductInMenuResponse()
        {
            ProductInMenuId = productInMenu.Id,
            Product = productInMenu.Product!.ToProductResponse(),
            Quantity = productInMenu.Quantity
        };
    }
}