using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using PRN232.Lab1.CoffeeStore.API.Mappers;
using PRN232.Lab1.CoffeeStore.Services.Interfaces.Services;
using PRN232.Lab1.CoffeeStore.Services.Models.Requests;

namespace PRN232.Lab1.CoffeeStore.API.Controllers.Menus.v1;

[ApiVersion(1)]
[ApiController]
[Route("api/v{v:apiVersion}/menus")]
public class MenusController : ControllerBase
{
    private readonly IMenuService _menuService;

    public MenusController(IMenuService menuService)
    {
        _menuService = menuService;
    }

    /// <summary>
    /// Get all menus with optional filtering and pagination
    /// </summary>
    /// <param name="request">Query parameters for filtering and pagination</param>
    /// <returns>Paginated list of active menus</returns>
    [MapToApiVersion(1)]
    [HttpGet("")]
    public async Task<IActionResult> GetMenusAsync([FromQuery] GetMenusRequest request)
    {
        var serviceResponse = await _menuService.GetMenus(request);
        
        return Ok(serviceResponse.ToDataApiResponse(Request, Response));
    }
    
    /// <summary>
    /// Get a specific menu by its ID with associated products
    /// </summary>
    /// <param name="menuId">The unique identifier of the menu</param>
    /// <param name="request">Base service request parameters</param>
    /// <returns>Menu details with associated active products</returns>
    [MapToApiVersion(1)]
    [HttpGet("{menuId}")]
    public async Task<IActionResult> GetMenuByIdAsync([FromRoute] Guid menuId, [FromQuery] BaseServiceRequest request)
    {
        var serviceResponse = await _menuService.GetMenuById(menuId);
        if (!serviceResponse.Success)
        {
            return NotFound(serviceResponse.ToDataApiResponse());
        }
        
        return Ok(serviceResponse.ToDataApiResponse());
    }

    /// <summary>
    /// Create a new menu
    /// </summary>
    /// <param name="request">Menu creation details including name and description</param>
    /// <returns>Created menu ID or validation errors</returns>
    [MapToApiVersion(1)]
    [HttpPost("")]
    public async Task<IActionResult> CreateMenuAsync([FromBody] CreateMenuRequest request)
    {
        var serviceResponse = await _menuService.CreateMenu(request);

        if (!serviceResponse.Success)
        {
            return BadRequest(serviceResponse.ToDataApiResponse());
        }

        return Created($"/api/v1/menus/{serviceResponse.ToDataApiResponse().Data}", null);
    }

    /// <summary>
    /// Update an existing menu's information
    /// </summary>
    /// <param name="menuId">The unique identifier of the menu to update</param>
    /// <param name="request">Updated menu details</param>
    /// <returns>Success status or validation errors</returns>
    [MapToApiVersion(1)]
    [HttpPut("{menuId}")]
    public async Task<IActionResult> UpdateMenuAsync(Guid menuId, [FromBody] UpdateMenuRequest request)
    {
        var serviceResponse = await _menuService.UpdateMenu(menuId, request);
        
        if (!serviceResponse.Success)
        {
            return BadRequest(serviceResponse.ToBaseApiResponse());
        }
        
        return NoContent();
    }

    /// <summary>
    /// Soft delete a menu by setting IsActive to false
    /// </summary>
    /// <param name="menuId">The unique identifier of the menu to delete</param>
    /// <returns>Success status or not found error</returns>
    [MapToApiVersion(1)]
    [HttpDelete("{menuId}")]
    public async Task<IActionResult> DeleteMenuAsync(Guid menuId)
    {
        var serviceResponse = await _menuService.DeleteMenu(menuId);
        
        if (!serviceResponse.Success)
        {
            return NotFound(serviceResponse.ToBaseApiResponse());
        }
        
        return NoContent();
    }
}
