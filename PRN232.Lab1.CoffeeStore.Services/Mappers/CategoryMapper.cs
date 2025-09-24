using PRN232.Lab1.CoffeeStore.Services.Models.Responses;
using PRN232.Lab1.CoffeeStore.Repositories.Models;

namespace PRN232.Lab1.CoffeeStore.Services.Mappers;

public static class CategoryMapper
{
    public static CategoryResponse ToCategoryResponse(this Category category)
    {
        return new CategoryResponse()
        {
            CategoryId = category.Id,
            Name = category.Name,
            Description = category.Description
        };
    }
}