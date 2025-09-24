using PRN232.Lab1.CoffeeStore.API.Models.Responses;
using PRN232.Lab1.CoffeeStore.Services.Models.Responses;

namespace PRN232.Lab1.CoffeeStore.API.Mappers;

public static class DataServiceResponseMapper
{
    public static DataApiResponse<T> ToDataApiResponse<T>(this DataServiceResponse<T> dataServiceResponse)
    {
        return new DataApiResponse<T>()
        {
            Success = dataServiceResponse.Success,
            Message = dataServiceResponse.Message,
            Data = dataServiceResponse.Data
        };
    }
}