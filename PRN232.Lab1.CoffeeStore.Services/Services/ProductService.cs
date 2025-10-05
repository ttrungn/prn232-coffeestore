using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PRN232.Lab1.CoffeeStore.Repositories.Interfaces;
using PRN232.Lab1.CoffeeStore.Repositories.Models;
using PRN232.Lab1.CoffeeStore.Services.Interfaces;
using PRN232.Lab1.CoffeeStore.Services.Interfaces.Services;
using PRN232.Lab1.CoffeeStore.Services.Mappers;
using PRN232.Lab1.CoffeeStore.Services.Models.Requests;
using PRN232.Lab1.CoffeeStore.Services.Models.Responses;
using System.Linq.Dynamic.Core;

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

    public async Task<DataServiceResponse<PaginationServiceResponse<ProductResponse>>> GetProducts(GetProductsRequest request)
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

        var paginationResponse = new PaginationServiceResponse<ProductResponse>()
        {
            TotalResults = totalProducts,
            TotalCurrentResults = productResponses.Count,
            Page = request.Page,
            PageSize = request.PageSize,
            Results = productResponses
        };
        
        return new DataServiceResponse<PaginationServiceResponse<ProductResponse>>()
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

    public async Task<DataServiceResponse<PaginationServiceResponse<object?>>> GetProducts(ProductQueryRequest request)
    {
        var productRepository = _unitOfWork.GetRepository<Product, Guid>();
        var query = productRepository.Query()
            .Include(p => p.Category)
                .Where(p => p.Category != null && p.Category.IsActive)   

            .AsNoTracking();

        var config = new ParsingConfig { IsCaseSensitive = false };


        var entityProps = typeof(Product).GetProperties()
            .Select(p => p.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);


        if (!string.IsNullOrEmpty(request.Sort))
        {
            var sortFields = request.Sort.Split(',', StringSplitOptions.TrimEntries);

            var invalidSorts = sortFields
                .Select(sf => sf.StartsWith("-") ? sf[1..] : sf)
                .Where(sf => !entityProps.Contains(sf))
                .ToList();
            if (invalidSorts.Any())
            {
                return new DataServiceResponse<PaginationServiceResponse<object?>>
                {
                    Success = false,
                    Message = $"Invalid select fields: {string.Join(", ", invalidSorts)}",
                    Data = new PaginationServiceResponse<object?> { }
                };
            }
        }

        if (!string.IsNullOrEmpty(request.Select))
        {
            var selectFields = request.Select.Split(',', StringSplitOptions.TrimEntries);

            var invalidSelects = selectFields
                .Where(sf => !entityProps.Contains(sf))
                .ToList();

            if (invalidSelects.Any())
            {
                return new DataServiceResponse<PaginationServiceResponse<object?>>
                {
                    Success = false,
                    Message = $"Invalid select fields: {string.Join(", ", invalidSelects)}",
                    Data = new PaginationServiceResponse<object?> { }
                };
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(p => p.Name.Contains(request.Search) ||
                                     p.Description.Contains(request.Search));
        }

        if (!string.IsNullOrWhiteSpace(request.Sort))
        {
            var sortFields = request.Sort.Split(',', StringSplitOptions.TrimEntries);

            var validSorts = sortFields.Select(f =>
            {
                bool desc = f.StartsWith("-");
                string prop = desc ? f[..] : f;
                var realProp = entityProps.FirstOrDefault(p =>
                   string.Equals(p, prop, StringComparison.OrdinalIgnoreCase))
                   ?? throw new Exception($"Invalid sort field: {prop}");

                return desc ? $"{realProp} descending" : $"{realProp} ascending";
            });

            query = query.OrderBy(config, string.Join(",", validSorts));

        }
        else
        {
            query = query.OrderBy(p => p.Name);
        }

        var totalCount = await query.CountAsync();
        var skip = (request.Page - 1) * request.PageSize;


        object data;

        if (!string.IsNullOrEmpty(request.Select))
        {
            var fields = request.Select.Split(',', StringSplitOptions.TrimEntries);
            var selector = "new(" + string.Join(",", fields) + ")";
            data = await query.Skip(skip).Take(request.PageSize)
                .Select(config, selector)
                .ToDynamicListAsync();
        }
        else
        {
            data = await query
                .Skip(skip)
                .Take(request.PageSize)
                .Select(p => new ProductResponse
                {
                    ProductId = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Category = new CategoryResponse
                    {
                        CategoryId = p.CategoryId,
                        Name = p.Name,
                        Description = p.Description,
                    }
                })
                .ToListAsync();
        }

        var result = ((IEnumerable<object>)data).ToList();

        return new DataServiceResponse<PaginationServiceResponse<object?>>
        {
            Success = true,
            Message = "Get Products Successfully",
            Data = new PaginationServiceResponse<object?>
            {
                TotalCurrentResults = result.Count, // number of items in this page
               
                Page = request.Page,
                PageSize = request.PageSize,
                TotalResults = totalCount,
                Results = result!
            }
        };

    }
}