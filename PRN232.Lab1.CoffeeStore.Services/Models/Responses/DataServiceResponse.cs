namespace PRN232.Lab1.CoffeeStore.Services.Models.Responses;

public class DataServiceResponse<T> : BaseServiceResponse
{
    public T Data { get; set; } = default!;
}