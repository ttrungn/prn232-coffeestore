using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PRN232.Lab1.CoffeeStore.Services.Interfaces;
using PRN232.Lab1.CoffeeStore.Services.Interfaces.Services;
using PRN232.Lab1.CoffeeStore.Services.Mappers;
using PRN232.Lab1.CoffeeStore.Services.Models.Requests;
using PRN232.Lab1.CoffeeStore.Services.Models.Responses;
using PRN232.Lab1.CoffeeStore.Repositories.Interfaces;
using PRN232.Lab1.CoffeeStore.Repositories.Models;

namespace PRN232.Lab1.CoffeeStore.Services.Services;

public class ProductService : IProductService
{
    private readonly ILogger<ProductService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(ILogger<ProductService> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<DataServiceResponse<PaginationResponse<ProductResponse>>> GetProducts(GetProductsRequest request)
    {
        var productRepository = _unitOfWork.GetRepository<Product, Guid>();
        var query = productRepository.Query()
            .Where(p => p.IsActive) // Only get active products
            .Include(p => p.Category)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Name))
            query = query.Where(p => p.Name.Contains(request.Name));
        
        var totalProducts = await query.CountAsync();
        var productResponses = await query
            .OrderBy(p => p.Name)
            .Skip(request.Page * request.PageSize)
            .Take(request.PageSize)
            .Select(p => p.ToProductResponse())
            .ToListAsync();

        var paginationResponse = new PaginationResponse<ProductResponse>()
        {
            TotalResults = totalProducts,
            TotalCurrentResults = productResponses.Count,
            Page = request.Page,
            Results = productResponses
        };
        
        return new DataServiceResponse<PaginationResponse<ProductResponse>>()
        {
            Success = true,
            Message = "Get products successfully",
            Data = paginationResponse,
        };
    }

    public async Task<DataServiceResponse<ProductResponse?>> GetProductById(Guid productId)
    {
        var productRepository = _unitOfWork.GetRepository<Product, Guid>();
        var productResponse = await productRepository.Query()
            .Where(p => p.IsActive) // Only get active products
            .Include(p => p.Category)
            .Where(p => p.Id == productId)
            .Select(p => p.ToProductResponse())
            .FirstOrDefaultAsync();

        if (productResponse == null)
        {
            return new DataServiceResponse<ProductResponse?>()
            {
                Success = false,
                Message = $"Product with id {productId} not found",
                Data = null,
            };
        }
        
        return new DataServiceResponse<ProductResponse?>()
        {
            Success = true,
            Message = "Get product successfully",
            Data = productResponse,
        };
    }

    public async Task<DataServiceResponse<Guid>> CreateProduct(CreateProductRequest request)
    {
        var productRepository = _unitOfWork.GetRepository<Product, Guid>();
        var categoryRepository = _unitOfWork.GetRepository<Category, Guid>();

        // Check if category exists
        var categoryExists = await categoryRepository.Query()
            .AnyAsync(c => c.Id == request.CategoryId);

        if (!categoryExists)
        {
            return new DataServiceResponse<Guid>()
            {
                Success = false,
                Message = $"Category with id {request.CategoryId} not found",
                Data = Guid.Empty
            };
        }

        // Create new product
        var product = request.ToProduct();
        await productRepository.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return new DataServiceResponse<Guid>()
        {
            Success = true,
            Message = "Product created successfully",
            Data = product.Id
        };
    }

    public async Task<BaseServiceResponse> UpdateProduct(Guid productId, UpdateProductRequest request)
    {
        var productRepository = _unitOfWork.GetRepository<Product, Guid>();
        var categoryRepository = _unitOfWork.GetRepository<Category, Guid>();

        // Check if product exists and is active
        var product = await productRepository.Query()
            .FirstOrDefaultAsync(p => p.Id == productId && p.IsActive);

        if (product == null)
        {
            return new BaseServiceResponse()
            {
                Success = false,
                Message = $"Product with id {productId} not found"
            };
        }

        // Check if category exists
        var categoryExists = await categoryRepository.Query()
            .AnyAsync(c => c.Id == request.CategoryId);

        if (!categoryExists)
        {
            return new BaseServiceResponse()
            {
                Success = false,
                Message = $"Category with id {request.CategoryId} not found"
            };
        }

        // Update product
        product.UpdateProduct(request);
        await productRepository.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return new BaseServiceResponse()
        {
            Success = true,
            Message = "Product updated successfully"
        };
    }

    public async Task<BaseServiceResponse> DeleteProduct(Guid productId)
    {
        var productRepository = _unitOfWork.GetRepository<Product, Guid>();

        // Check if product exists and is active
        var product = await productRepository.Query()
            .FirstOrDefaultAsync(p => p.Id == productId && p.IsActive);

        if (product == null)
        {
            return new BaseServiceResponse()
            {
                Success = false,
                Message = $"Product with id {productId} not found"
            };
        }

        // Soft delete by setting IsActive to false
        product.IsActive = false;
        product.DeletedAt = DateTime.UtcNow;
        
        await productRepository.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return new BaseServiceResponse()
        {
            Success = true,
            Message = "Product deleted successfully"
        };
    }
}