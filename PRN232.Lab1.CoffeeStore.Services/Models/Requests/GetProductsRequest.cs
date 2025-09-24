namespace PRN232.Lab1.CoffeeStore.Services.Models.Requests;

public class GetProductsRequest : BaseServiceRequest
{
    public int Page { get; init; } = 0;
    public int PageSize { get; init; } = 10;
    public string Name { get; init; } = string.Empty;
}