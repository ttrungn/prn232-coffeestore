using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using PRN232.Lab1.CoffeeStore.API.Mappers;
using PRN232.Lab1.CoffeeStore.Services.Interfaces.Services;
using PRN232.Lab1.CoffeeStore.Services.Models.Requests;

namespace PRN232.Lab1.CoffeeStore.API.Controllers.Products.v2;

[ApiVersion(2)]
[ApiController]
[Route("api/v{v:apiVersion}/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    
    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }
    
    /// <summary>
    /// Retrieves a list of products based on the specified query parameters.
    /// </summary>
    /// <param name="requestV2">The product query parameters.</param>
    /// <returns>A list of products matching the query criteria.</returns>
    [MapToApiVersion(2)]
    [HttpGet("")]
    public async Task<IActionResult> GetProductsAsync([FromQuery] GetProductsRequestV2 requestV2)
    {
        var serviceResponse = await _productService.GetProducts(requestV2);
        if (!serviceResponse.Success)
        {
            return BadRequest(serviceResponse.ToBaseApiResponse());
        }
        return Ok(serviceResponse.ToDataApiResponse(Request, Response));
    }
}
