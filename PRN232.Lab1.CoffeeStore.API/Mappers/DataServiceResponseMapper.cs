using System.Text.Json;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using PRN232.Lab1.CoffeeStore.API.Models.Responses;
using PRN232.Lab1.CoffeeStore.Services.Models.Responses;

namespace PRN232.Lab1.CoffeeStore.API.Mappers;

public static class DataServiceResponseMapper
{
    private static readonly JsonSerializerOptions HeaderJsonOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = false };
    
    public static DataApiResponse<T> ToDataApiResponse<T>(this DataServiceResponse<T> dataServiceResponse, HttpRequest? request = null, HttpResponse? response = null)
    {
        if (typeof(T).IsGenericType && 
            typeof(T).GetGenericTypeDefinition() == typeof(PaginationServiceResponse<>))
        {
            if (response != null && request != null && dataServiceResponse.Data != null)
            {
                dynamic paginationResponse = dataServiceResponse.Data;
                var paginationHeader = new PaginationHeader()
                {
                    Page = paginationResponse.Page,
                    PageSize = paginationResponse.PageSize,
                    TotalResults = paginationResponse.TotalResults,
                    TotalCurrentResults = paginationResponse.TotalCurrentResults,
                    PreviousPageLink = GetPaginationLink(request.GetDisplayUrl(), paginationResponse.Page - 1, paginationResponse.PageSize),
                    NextPageLink = GetPaginationLink(request.GetDisplayUrl(), paginationResponse.Page + 1, paginationResponse.PageSize),
                    FirstPageLink = GetPaginationLink(request.GetDisplayUrl(), 0, paginationResponse.PageSize),
                };
                response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginationHeader, HeaderJsonOptions));
                

                response.Headers.AccessControlExposeHeaders = "X-Pagination";
            }
        }
        
        return new DataApiResponse<T>()
        {
            Success = dataServiceResponse.Success,
            Message = dataServiceResponse.Message,
            Data = dataServiceResponse.Data
        };
    }

    private static string GetPaginationLink(string baseUrl, int page, int pageSize)
    {
        var baseUri = new Uri(baseUrl);
        
        // Copy existing query params
        var query = QueryHelpers.ParseQuery(baseUri.Query)
            .ToDictionary(
                k => k.Key,
                v => v.Value.ToString());
        var dict = query.ToDictionary(kvp => kvp.Key, string? (kvp) => kvp.Value);
        
        // Replace page and pageSize
        if (page < 0)
        {
            page = 0;
        }
        query["page"] = page.ToString();
        query["pageSize"] = pageSize.ToString();

        return QueryHelpers.AddQueryString(baseUri.GetLeftPart(UriPartial.Path), dict);;
    }
}