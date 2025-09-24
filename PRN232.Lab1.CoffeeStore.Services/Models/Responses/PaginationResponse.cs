namespace PRN232.Lab1.CoffeeStore.Services.Models.Responses;

public class PaginationResponse<T>
{
    public int TotalResults { get; set; }
    public int Page { get; set; }
    public int TotalCurrentResults { get; set; }
    public List<T> Results { get; set; } = [];
}