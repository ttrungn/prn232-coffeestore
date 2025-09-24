namespace PRN232.Lab1.CoffeeStore.API.Models.Responses;

public class BaseApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
}