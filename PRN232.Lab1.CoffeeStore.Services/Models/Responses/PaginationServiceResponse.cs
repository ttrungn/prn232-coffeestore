namespace PRN232.Lab1.CoffeeStore.Services.Models.Responses;

public class PaginationServiceResponse<T>
{
    public int TotalResults { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCurrentResults { get; set; }
    public List<T> Results { get; set; } = [];
}