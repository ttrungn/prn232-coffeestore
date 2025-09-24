using PRN232.Lab1.CoffeeStore.Services.Models.Requests;
using PRN232.Lab1.CoffeeStore.Services.Models.Responses;

namespace PRN232.Lab1.CoffeeStore.Services.Interfaces.Services;

public interface IProductService
{
    Task<DataServiceResponse<PaginationResponse<ProductResponse>>> GetProducts(GetProductsRequest request);
    Task<DataServiceResponse<ProductResponse?>> GetProductById(Guid productId);
    Task<DataServiceResponse<Guid>> CreateProduct(CreateProductRequest request);
    Task<BaseServiceResponse> UpdateProduct(Guid productId, UpdateProductRequest request);
    Task<BaseServiceResponse> DeleteProduct(Guid productId);
}