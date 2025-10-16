using System.ComponentModel.DataAnnotations;

namespace PRN232.Lab1.CoffeeStore.Services.Models.Requests;

public class GetProductsRequest : BaseServiceRequest
{
    public int Page { get; init; } = 0;
    public int PageSize { get; init; } = 10;
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;
}
