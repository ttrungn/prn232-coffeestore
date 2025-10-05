using PRN232.Lab1.CoffeeStore.Services.Models.Requests;
using PRN232.Lab1.CoffeeStore.Services.Models.Responses;
using PRN232.Lab1.CoffeeStore.Repositories.Models;

namespace PRN232.Lab1.CoffeeStore.Services.Mappers;

public static class ProductMapper
{
    public static ProductResponse ToProductResponse(this Product product)
    {
        return new ProductResponse()
        {
            ProductId = product.Id,
            Name = product.Name,
            Price = product.Price,
            Description = product.Description,
            Category = product.Category!.ToCategoryResponse()
        };
    }

    public static Product ToProduct(this CreateProductRequest request)
    {
        return new Product()
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Price = request.Price,
            Description = request.Description,
            CategoryId = request.CategoryId,
            IsActive = true
        };
    }

    public static void UpdateProduct(this Product product, UpdateProductRequest request)
    {
        product.Name = request.Name;
        product.Price = request.Price;
        product.Description = request.Description;
        product.CategoryId = request.CategoryId;
    }
}
