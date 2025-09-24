using Microsoft.EntityFrameworkCore;
using PRN232.Lab1.CoffeeStore.Services.Interfaces.Services;
using PRN232.Lab1.CoffeeStore.Services.Mappers;
using PRN232.Lab1.CoffeeStore.Services.Models.Requests;
using PRN232.Lab1.CoffeeStore.Services.Models.Responses;
using PRN232.Lab1.CoffeeStore.Repositories.Interfaces;
using PRN232.Lab1.CoffeeStore.Repositories.Models;

namespace PRN232.Lab1.CoffeeStore.Services.Services;

public class MenuService : IMenuService
{
    private readonly IUnitOfWork _unitOfWork;

    public MenuService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DataServiceResponse<PaginationResponse<MenuResponse>>> GetMenus(GetMenusRequest request)
    {
        var menuRepository = _unitOfWork.GetRepository<Menu, Guid>();
        var query = menuRepository.Query()
            .Where(m => m.IsActive) // Only select active menus
            .AsNoTracking();
        
        if (!string.IsNullOrWhiteSpace(request.Name))
            query = query.Where(m => m.Name.Contains(request.Name));

        if (request.FromDate.HasValue)
        {
            query = query.Where(m => m.FromDate >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(m => m.ToDate <= request.ToDate.Value);
        }

        var totalMenus = await query.CountAsync();
        
        var menuResponses = await query
            .OrderBy(m => m.Name)
            .Skip(request.Page * request.PageSize)
            .Take(request.PageSize)
            .Select(m => m.ToMenuResponse())
            .ToListAsync();

        var paginationResponse = new PaginationResponse<MenuResponse>()
        {
            TotalResults = totalMenus,
            TotalCurrentResults = menuResponses.Count,
            Page = request.Page,
            Results = menuResponses
        };

        return new DataServiceResponse<PaginationResponse<MenuResponse>>()
        {
            Success = true,
            Message = "Get menus successfully",
            Data = paginationResponse,
        };
    }

    public async Task<DataServiceResponse<MenuDetailsResponse?>> GetMenuById(Guid menuId)
    {
        var menuRepository = _unitOfWork.GetRepository<Menu, Guid>();
        var menu = await menuRepository.Query()
            .Where(m => m.IsActive) // Only select active menus
            .Include(m => m.ProductInMenus.Where(pim => pim.Product!.IsActive)) // Only include active products
                .ThenInclude(pim => pim.Product)
                    .ThenInclude(p => p!.Category)
            .FirstOrDefaultAsync(m => m.Id == menuId);

        if (menu == null)
        {
            return new DataServiceResponse<MenuDetailsResponse?>()
            {
                Success = false,
                Message = $"Menu with id {menuId} not found",
                Data = null,
            };
        }

        return new DataServiceResponse<MenuDetailsResponse?>()
        {
            Success = true,
            Message = "Get menu successfully",
            Data = menu.ToMenuDetailsResponse(),
        };
    }

    public async Task<DataServiceResponse<Guid>> CreateMenu(CreateMenuRequest request)
    {
        var menuRepository = _unitOfWork.GetRepository<Menu, Guid>();
        var productRepository = _unitOfWork.GetRepository<Product, Guid>();
        var productInMenuRepository = _unitOfWork.GetRepository<ProductInMenu, Guid>();

        // Validate date range
        if (request.FromDate >= request.ToDate)
        {
            return new DataServiceResponse<Guid>()
            {
                Success = false,
                Message = "FromDate must be earlier than ToDate",
                Data = Guid.Empty
            };
        }

        // Validate all products exist and are active
        var productIds = request.Products.Select(p => p.ProductId).ToList();
        var existingProducts = await productRepository.Query()
            .Where(p => productIds.Contains(p.Id) && p.IsActive) // Only allow active products
            .Select(p => p.Id)
            .ToListAsync();

        var missingProducts = productIds.Except(existingProducts).ToList();
        if (missingProducts.Any())
        {
            return new DataServiceResponse<Guid>()
            {
                Success = false,
                Message = $"Products not found or inactive: {string.Join(", ", missingProducts)}",
                Data = Guid.Empty
            };
        }

        // Create menu
        var menu = request.ToMenu();
        await menuRepository.AddAsync(menu);

        // Create product-menu relationships
        var productInMenus = request.Products.Select(p => new ProductInMenu
        {
            Id = Guid.NewGuid(),
            ProductId = p.ProductId,
            MenuId = menu.Id,
            Quantity = p.Quantity
        }).ToList();

        foreach (var productInMenu in productInMenus)
        {
            await productInMenuRepository.AddAsync(productInMenu);
        }

        await _unitOfWork.SaveChangesAsync();

        return new DataServiceResponse<Guid>()
        {
            Success = true,
            Message = "Menu created successfully",
            Data = menu.Id
        };
    }

    public async Task<BaseServiceResponse> UpdateMenu(Guid menuId, UpdateMenuRequest request)
    {
        var menuRepository = _unitOfWork.GetRepository<Menu, Guid>();
        var productRepository = _unitOfWork.GetRepository<Product, Guid>();
        var productInMenuRepository = _unitOfWork.GetRepository<ProductInMenu, Guid>();

        var menu = await menuRepository.Query()
            .Include(m => m.ProductInMenus)
            .FirstOrDefaultAsync(m => m.Id == menuId && m.IsActive); // Only select active menus

        if (menu == null)
        {
            return new BaseServiceResponse()
            {
                Success = false,
                Message = $"Menu with id {menuId} not found",
            };
        }

        if (request.FromDate >= request.ToDate)
        {
            return new BaseServiceResponse()
            {
                Success = false,
                Message = "FromDate must be earlier than ToDate",
            };
        }

        var productIds = request.Products.Select(p => p.ProductId).ToList();
        var existingProducts = await productRepository.Query()
            .Where(p => productIds.Contains(p.Id) && p.IsActive) // Only allow active products
            .Select(p => p.Id)
            .ToListAsync();

        var missingProducts = productIds.Except(existingProducts).ToList();
        if (missingProducts.Any())
        {
            return new BaseServiceResponse()
            {
                Success = false,
                Message = $"Products not found or inactive: {string.Join(", ", missingProducts)}",
            };
        }

        menu.UpdateMenu(request);

        var existingProductInMenus = menu.ProductInMenus.ToList();
        foreach (var existingPim in existingProductInMenus)
        {
            await productInMenuRepository.RemoveAsync(existingPim);
        }

        var newProductInMenus = request.Products.Select(p => new ProductInMenu
        {
            Id = Guid.NewGuid(),
            ProductId = p.ProductId,
            MenuId = menuId,
            Quantity = p.Quantity
        }).ToList();

        foreach (var productInMenu in newProductInMenus)
        {
            await productInMenuRepository.AddAsync(productInMenu);
        }

        await menuRepository.UpdateAsync(menu);
        await _unitOfWork.SaveChangesAsync();

        return new BaseServiceResponse()
        {
            Success = true,
            Message = "Menu updated successfully"
        };
    }

    public async Task<BaseServiceResponse> DeleteMenu(Guid menuId)
    {
        var menuRepository = _unitOfWork.GetRepository<Menu, Guid>();

        var menu = await menuRepository.Query()
            .FirstOrDefaultAsync(m => m.Id == menuId && m.IsActive);

        if (menu == null)
        {
            return new BaseServiceResponse()
            {
                Success = false,
                Message = $"Menu with id {menuId} not found"
            };
        }

        // Soft delete by setting IsActive to false
        menu.IsActive = false;
        menu.DeletedAt = DateTime.UtcNow;
        
        await menuRepository.UpdateAsync(menu);
        await _unitOfWork.SaveChangesAsync();

        return new BaseServiceResponse()
        {
            Success = true,
            Message = "Menu deleted successfully"
        };
    }
}
