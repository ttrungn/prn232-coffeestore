using PRN232.Lab1.CoffeeStore.Services.Models.Requests;
using PRN232.Lab1.CoffeeStore.Services.Models.Responses;

namespace PRN232.Lab1.CoffeeStore.Services.Interfaces.Services;

public interface IProductService
{
    Task<DataServiceResponse<PaginationServiceResponse<ProductResponse>>> GetProducts(GetProductsRequest request);
    Task<DataServiceResponse<ProductResponse?>> GetProductById(Guid productId);
    Task<DataServiceResponse<Guid>> CreateProduct(CreateProductRequest request);
    Task<BaseServiceResponse> UpdateProduct(Guid productId, UpdateProductRequest request);
    Task<BaseServiceResponse> DeleteProduct(Guid productId);

    Task<DataServiceResponse<PaginationServiceResponse<object?>>>
        GetProducts(GetProductsRequestV2 requestV2);

    Task<DataServiceResponse<ProductResponse?>> GetProductByIdWithInMemoryCache(Guid productId);
    Task<BaseServiceResponse> UpdateProductWithInMemoryCache(Guid productId, UpdateProductRequest request);
    Task<DataServiceResponse<ProductResponse?>> GetProductByIdWithDistributedRedisCache(Guid productId);
    Task<BaseServiceResponse> UpdateProductWithDistributedCache(Guid productId, UpdateProductRequest request);
}
