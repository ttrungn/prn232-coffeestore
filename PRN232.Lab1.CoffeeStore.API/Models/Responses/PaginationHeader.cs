namespace PRN232.Lab1.CoffeeStore.API.Models.Responses;

public class PaginationHeader
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalResults { get; set; }
    public int TotalCurrentResults { get; set; }
    public string PreviousPageLink { get; set; } = string.Empty;
    public string NextPageLink { get; set; } = string.Empty;
    public string FirstPageLink { get; set; } = string.Empty;
}