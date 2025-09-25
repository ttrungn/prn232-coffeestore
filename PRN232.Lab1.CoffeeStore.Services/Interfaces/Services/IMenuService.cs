using PRN232.Lab1.CoffeeStore.Services.Models.Requests;
using PRN232.Lab1.CoffeeStore.Services.Models.Responses;

namespace PRN232.Lab1.CoffeeStore.Services.Interfaces.Services;

public interface IMenuService
{
    Task<DataServiceResponse<PaginationServiceResponse<MenuResponse>>> GetMenus(GetMenusRequest request);
    Task<DataServiceResponse<MenuDetailsResponse?>> GetMenuById(Guid menuId);
    Task<DataServiceResponse<Guid>> CreateMenu(CreateMenuRequest request);
    Task<BaseServiceResponse> UpdateMenu(Guid menuId, UpdateMenuRequest request);
    Task<BaseServiceResponse> DeleteMenu(Guid menuId);
}