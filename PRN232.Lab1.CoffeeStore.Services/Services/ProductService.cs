using System.Linq.Dynamic.Core;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PRN232.Lab1.CoffeeStore.Repositories.Interfaces;
using PRN232.Lab1.CoffeeStore.Repositories.Models;
using PRN232.Lab1.CoffeeStore.Services.Interfaces.Services;
using PRN232.Lab1.CoffeeStore.Services.Mappers;
using PRN232.Lab1.CoffeeStore.Services.Models.Requests;
using PRN232.Lab1.CoffeeStore.Services.Models.Responses;

namespace PRN232.Lab1.CoffeeStore.Services.Services;

public class ProductService : IProductService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = false
    };

    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<ProductService> _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(ILogger<ProductService> logger, IUnitOfWork unitOfWork, IMemoryCache memoryCache,
        IDistributedCache distributedCache)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
    }

    public async Task<DataServiceResponse<PaginationServiceResponse<ProductResponse>>> GetProducts(
        GetProductsRequest request)
    {
        var productRepository = _unitOfWork.GetRepository<Product, Guid>();
        var query = productRepository.Query()
            .Where(p => p.IsActive) // Only get active products
            .Include(p => p.Category)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            query = query.Where(p => p.Name.Contains(request.Name));
        }

        var totalProducts = await query.CountAsync();
        var productResponses = await query
            .OrderBy(p => p.Name)
            .Skip(request.Page * request.PageSize)
            .Take(request.PageSize)
            .Select(p => p.ToProductResponse())
            .ToListAsync();

        var paginationResponse = new PaginationServiceResponse<ProductResponse>
        {
            TotalResults = totalProducts,
            TotalCurrentResults = productResponses.Count,
            Page = request.Page,
            PageSize = request.PageSize,
            Results = productResponses
        };

        return new DataServiceResponse<PaginationServiceResponse<ProductResponse>>
        {
            Success = true, Message = "Get products successfully", Data = paginationResponse
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
            return new DataServiceResponse<ProductResponse?>
            {
                Success = false, Message = $"Product with id {productId} not found", Data = null
            };
        }

        return new DataServiceResponse<ProductResponse?>
        {
            Success = true, Message = "Get product successfully", Data = productResponse
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
            return new DataServiceResponse<Guid>
            {
                Success = false, Message = $"Category with id {request.CategoryId} not found", Data = Guid.Empty
            };
        }

        // Create new product
        var product = request.ToProduct();
        await productRepository.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return new DataServiceResponse<Guid>
        {
            Success = true, Message = "Product created successfully", Data = product.Id
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
            return new BaseServiceResponse { Success = false, Message = $"Product with id {productId} not found" };
        }

        // Check if category exists
        var categoryExists = await categoryRepository.Query()
            .AnyAsync(c => c.Id == request.CategoryId);

        if (!categoryExists)
        {
            return new BaseServiceResponse
            {
                Success = false, Message = $"Category with id {request.CategoryId} not found"
            };
        }

        // Update product
        product.UpdateProduct(request);
        await productRepository.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return new BaseServiceResponse { Success = true, Message = "Product updated successfully" };
    }

    public async Task<BaseServiceResponse> DeleteProduct(Guid productId)
    {
        var productRepository = _unitOfWork.GetRepository<Product, Guid>();

        // Check if product exists and is active
        var product = await productRepository.Query()
            .FirstOrDefaultAsync(p => p.Id == productId && p.IsActive);

        if (product == null)
        {
            return new BaseServiceResponse { Success = false, Message = $"Product with id {productId} not found" };
        }

        // Soft delete by setting IsActive to false
        product.IsActive = false;
        product.DeletedAt = DateTime.UtcNow;

        await productRepository.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return new BaseServiceResponse { Success = true, Message = "Product deleted successfully" };
    }

    public async Task<DataServiceResponse<PaginationServiceResponse<object?>>> GetProducts(
        GetProductsRequestV2 requestV2)
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


        if (!string.IsNullOrEmpty(requestV2.Sort))
        {
            var sortFields = requestV2.Sort.Split(',', StringSplitOptions.TrimEntries);

            var invalidSorts = sortFields
                .Select(sf => sf.StartsWith("-") ? sf[1..] : sf)
                .Where(sf => !entityProps.Contains(sf))
                .ToList();
            if (invalidSorts.Count != 0)
            {
                return new DataServiceResponse<PaginationServiceResponse<object?>>
                {
                    Success = false,
                    Message = $"Invalid select fields: {string.Join(", ", invalidSorts)}",
                    Data = new PaginationServiceResponse<object?>()
                };
            }
        }

        if (!string.IsNullOrEmpty(requestV2.Select))
        {
            var selectFields = requestV2.Select.Split(',', StringSplitOptions.TrimEntries);

            var invalidSelects = selectFields
                .Where(sf => !entityProps.Contains(sf))
                .ToList();

            if (invalidSelects.Count != 0)
            {
                return new DataServiceResponse<PaginationServiceResponse<object?>>
                {
                    Success = false,
                    Message = $"Invalid select fields: {string.Join(", ", invalidSelects)}",
                    Data = new PaginationServiceResponse<object?>()
                };
            }
        }

        if (!string.IsNullOrWhiteSpace(requestV2.Search))
        {
            query = query.Where(p => p.Name.Contains(requestV2.Search) ||
                                     p.Description.Contains(requestV2.Search));
        }

        if (!string.IsNullOrWhiteSpace(requestV2.Sort))
        {
            var sortFields = requestV2.Sort.Split(',', StringSplitOptions.TrimEntries);

            var validSorts = sortFields.Select(f =>
            {
                var desc = f.StartsWith("-");
                var prop = desc ? f[1..] : f;
                var realProp = entityProps.FirstOrDefault(p =>
                                   string.Equals(p, prop, StringComparison.OrdinalIgnoreCase))
                               ?? throw new Exception($"Invalid sort field: {prop}");

                return desc ? $"{realProp} descending" : $"{realProp} ascending";
            });

            query = query.OrderBy(config, string.Join(",", validSorts)).ThenBy(p => p.Id);
        }
        else
        {
            query = query.OrderBy(p => p.Name).ThenBy(p => p.Id);
        }

        var totalCount = await query.CountAsync();
        var skip = (requestV2.Page - 1) * requestV2.PageSize;


        object data;

        if (!string.IsNullOrEmpty(requestV2.Select))
        {
            var fields = requestV2.Select.Split(',', StringSplitOptions.TrimEntries);
            var selector = "new(" + string.Join(",", fields) + ")";
            data = await query.Skip(skip).Take(requestV2.PageSize)
                .Select(config, selector)
                .ToDynamicListAsync();
        }
        else
        {
            data = await query
                .Skip(skip)
                .Take(requestV2.PageSize)
                .Select(p => new ProductResponse
                {
                    ProductId = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Category = new CategoryResponse
                    {
                        CategoryId = p.CategoryId, Name = p.Name, Description = p.Description
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

                Page = requestV2.Page,
                PageSize = requestV2.PageSize,
                TotalResults = totalCount,
                Results = result!
            }
        };
    }

    public async Task<DataServiceResponse<ProductResponse?>> GetProductByIdWithInMemoryCache(Guid productId)
    {
        if (_memoryCache.TryGetValue($"product:{productId}", out ProductResponse? cacheValue))
        {
            return new DataServiceResponse<ProductResponse?>
            {
                Success = true, Message = "Get product successfully", Data = cacheValue
            };
        }

        var productRepository = _unitOfWork.GetRepository<Product, Guid>();
        var productResponse = await productRepository.Query()
            .Where(p => p.IsActive)
            .Include(p => p.Category)
            .Where(p => p.Id == productId)
            .Select(p => p.ToProductResponse())
            .FirstOrDefaultAsync();

        if (productResponse == null)
        {
            return new DataServiceResponse<ProductResponse?>
            {
                Success = false, Message = $"Product with id {productId} not found", Data = null
            };
        }

        cacheValue = productResponse;
        var baseTtl = TimeSpan.FromMinutes(1);
        var jitterSec = Random.Shared.Next(0, 30);
        var effectiveTtl = baseTtl + TimeSpan.FromSeconds(jitterSec);
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(effectiveTtl);

        _memoryCache.Set($"product:{productId}", cacheValue, cacheEntryOptions);


        return new DataServiceResponse<ProductResponse?>
        {
            Success = true, Message = "Get product successfully", Data = productResponse
        };
    }


    public async Task<DataServiceResponse<ProductResponse?>> GetProductByIdWithDistributedRedisCache(Guid productId)
    {
        var cacheKey = $"product:{productId}";

        // 1) Try cache first
        var cached = await _distributedCache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cached))
        {
            var fromCache = JsonSerializer.Deserialize<ProductResponse>(cached, JsonOptions);
            if (fromCache != null)
            {
                return new DataServiceResponse<ProductResponse?>
                {
                    Success = true, Message = "Get product successfully (from Redis cache)", Data = fromCache
                };
            }
        }

        // 2) Fallback to DB
        var productRepository = _unitOfWork.GetRepository<Product, Guid>();
        var productResponse = await productRepository.Query()
            .Where(p => p.IsActive)
            .Include(p => p.Category)
            .Where(p => p.Id == productId)
            .Select(p => p.ToProductResponse())
            .FirstOrDefaultAsync();

        if (productResponse == null)
        {
            return new DataServiceResponse<ProductResponse?>
            {
                Success = false, Message = $"Product with id {productId} not found", Data = null
            };
        }

        // 3) Put into cache with TTL (+ jitter to avoid stampedes)
        var baseTtl = TimeSpan.FromMinutes(1);
        var jitterSec = Random.Shared.Next(0, 30); // 0..30s extra
        var effectiveTtl = baseTtl + TimeSpan.FromSeconds(jitterSec);

        var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = effectiveTtl };

        var serialized = JsonSerializer.Serialize(productResponse, JsonOptions);
        await _distributedCache.SetStringAsync(cacheKey, serialized, options);

        return new DataServiceResponse<ProductResponse?>
        {
            Success = true, Message = "Get product successfully", Data = productResponse
        };
    }

    // In-Memory cache version
    public async Task<BaseServiceResponse> UpdateProductWithInMemoryCache(Guid productId, UpdateProductRequest request)
    {
        var productRepository = _unitOfWork.GetRepository<Product, Guid>();
        var categoryRepository = _unitOfWork.GetRepository<Category, Guid>();

        // 1) Tồn tại product?
        var product = await productRepository.Query()
            .FirstOrDefaultAsync(p => p.Id == productId && p.IsActive);

        if (product == null)
        {
            return new BaseServiceResponse { Success = false, Message = $"Product with id {productId} not found" };
        }

        // 2) Tồn tại category?
        var categoryExists = await categoryRepository.Query()
            .AnyAsync(c => c.Id == request.CategoryId);

        if (!categoryExists)
        {
            return new BaseServiceResponse
            {
                Success = false, Message = $"Category with id {request.CategoryId} not found"
            };
        }

        // 3) Cập nhật DB
        product.UpdateProduct(request);
        await productRepository.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        // 4) Write-Around (khuyến nghị với cache-aside): xóa cache để lần GET sau tự nạp lại
        var cacheKey = $"product:{productId}";
        _memoryCache.Remove(cacheKey);

        // Nếu muốn Write-Through thay vì xóa cache (không cần lần GET sau hit DB):
        // var updatedDto = product.ToProductResponse();
        // _memoryCache.Set(cacheKey, updatedDto, new MemoryCacheEntryOptions()
        //     .SetSlidingExpiration(TimeSpan.FromMinutes(5)));

        return new BaseServiceResponse { Success = true, Message = "Product updated successfully" };
    }

    public async Task<BaseServiceResponse> UpdateProductWithDistributedCache(Guid productId,
        UpdateProductRequest request)
    {
        var productRepository = _unitOfWork.GetRepository<Product, Guid>();
        var categoryRepository = _unitOfWork.GetRepository<Category, Guid>();

        var product = await productRepository.Query()
            .FirstOrDefaultAsync(p => p.Id == productId && p.IsActive);

        if (product == null)
        {
            return new BaseServiceResponse { Success = false, Message = $"Product with id {productId} not found" };
        }

        var categoryExists = await categoryRepository.Query()
            .AnyAsync(c => c.Id == request.CategoryId);

        if (!categoryExists)
        {
            return new BaseServiceResponse
            {
                Success = false, Message = $"Category with id {request.CategoryId} not found"
            };
        }

        product.UpdateProduct(request);
        await productRepository.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        var cacheKey = $"product:{productId}";

        // Write-Around (khuyến nghị): xóa cache Redis
        await _distributedCache.RemoveAsync(cacheKey);

        // Nếu muốn Write-Through:
        // var updatedDto = product.ToProductResponse();
        // var json = JsonSerializer.Serialize(updatedDto, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        // var options = new DistributedCacheEntryOptions
        // {
        //     AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        //     // hoặc SlidingExpiration = TimeSpan.FromMinutes(2)
        // };
        // await _distributedCache.SetStringAsync(cacheKey, json, options);

        return new BaseServiceResponse { Success = true, Message = "Product updated successfully" };
    }
}
