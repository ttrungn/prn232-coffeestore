namespace PRN232.Lab1.CoffeeStore.Services.Models.Responses;

public class CategoryResponse
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
}