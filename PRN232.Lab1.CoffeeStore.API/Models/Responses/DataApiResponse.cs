namespace PRN232.Lab1.CoffeeStore.API.Models.Responses;

public class DataApiResponse<T> : BaseApiResponse
{
    public T Data { get; set; } = default!;
}