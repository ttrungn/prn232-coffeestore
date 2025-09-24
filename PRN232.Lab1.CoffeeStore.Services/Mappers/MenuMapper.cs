using PRN232.Lab1.CoffeeStore.Services.Models.Requests;
using PRN232.Lab1.CoffeeStore.Services.Models.Responses;
using PRN232.Lab1.CoffeeStore.Repositories.Models;

namespace PRN232.Lab1.CoffeeStore.Services.Mappers;

public static class MenuMapper
{
    public static MenuResponse ToMenuResponse(this Menu menu)
    {
        return new MenuResponse()
        {
            MenuId = menu.Id,
            Name = menu.Name,
            FromDate = menu.FromDate,
            ToDate = menu.ToDate
        };
    }

    public static MenuDetailsResponse ToMenuDetailsResponse(this Menu menu)
    {
        return new MenuDetailsResponse()
        {
            MenuId = menu.Id,
            Name = menu.Name,
            FromDate = menu.FromDate,
            ToDate = menu.ToDate,
            Products = menu.ProductInMenus?.Select(pim => pim.ToProductInMenuResponse()).ToList()!
        };
    }

    public static Menu ToMenu(this CreateMenuRequest request)
    {
        return new Menu()
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            FromDate = request.FromDate!.Value,
            ToDate = request.ToDate!.Value
        };
    }

    public static void UpdateMenu(this Menu menu, UpdateMenuRequest request)
    {
        menu.Name = request.Name;
        menu.FromDate = request.FromDate!.Value;
        menu.ToDate = request.ToDate!.Value;
        menu.UpdatedAt = DateTime.UtcNow;
    }
}