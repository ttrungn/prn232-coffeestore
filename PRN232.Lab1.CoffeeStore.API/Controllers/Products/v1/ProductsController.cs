using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using PRN232.Lab1.CoffeeStore.API.Mappers;
using PRN232.Lab1.CoffeeStore.Services.Interfaces.Services;
using PRN232.Lab1.CoffeeStore.Services.Models.Requests;

namespace PRN232.Lab1.CoffeeStore.API.Controllers.Products.v1;

[ApiVersion(1)]
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
    /// Get all products with optional filtering and pagination
    /// </summary>
    /// <param name="requestV2">Query parameters for filtering and pagination including category, price range, and search terms</param>
    /// <returns>Paginated list of active products matching the filter criteria</returns>
    [MapToApiVersion(1)]
    [HttpGet("")]
    public async Task<IActionResult> GetProductsAsync([FromQuery] GetProductsRequest requestV2)
    {
        var serviceResponse = await _productService.GetProducts(requestV2);
        return Ok(serviceResponse.ToDataApiResponse(Request, Response));
    }

    /// <summary>
    /// Get detailed information about a specific product by its ID
    /// </summary>
    /// <param name="productId">The unique identifier of the product</param>
    /// <param name="request">Base service requestV2 parameters</param>
    /// <returns>Complete product details including name, description, price, and category information</returns>
    [MapToApiVersion(1)]
    [HttpGet("{productId}")]
    public async Task<IActionResult> GetProductByIdAsync([FromRoute] Guid productId, [FromQuery] BaseServiceRequest request)
    {
        var serviceResponse = await _productService.GetProductById(productId);
        if (!serviceResponse.Success)
        {
            return NotFound(serviceResponse.ToDataApiResponse());
        }
        
        return Ok(serviceResponse.ToDataApiResponse());
    }

    /// <summary>
    /// Create a new product in the system
    /// </summary>
    /// <param name="request">Product creation details including name, description, price, and category</param>
    /// <returns>Created product ID or validation errors</returns>
    [MapToApiVersion(1)]
    [HttpPost("")]
    public async Task<IActionResult> CreateProductAsync([FromBody] CreateProductRequest request)
    {
        var serviceResponse = await _productService.CreateProduct(request);

        if (!serviceResponse.Success)
        {
            return BadRequest(serviceResponse.ToDataApiResponse());
        }

        return Created($"/api/v1/products/{serviceResponse.ToDataApiResponse().Data}", null);
    }

    /// <summary>
    /// Update an existing product's information
    /// </summary>
    /// <param name="productId">The unique identifier of the product to update</param>
    /// <param name="request">Updated product details including name, description, price, and category</param>
    /// <returns>Success status or validation errors</returns>
    [MapToApiVersion(1)]
    [HttpPut("{productId}")]
    public async Task<IActionResult> UpdateProductAsync(Guid productId, [FromBody] UpdateProductRequest request)
    {
        var serviceResponse = await _productService.UpdateProduct(productId, request);
        
        if (!serviceResponse.Success)
        {
            return BadRequest(serviceResponse.ToBaseApiResponse());
        }
        
        return NoContent();
    }

    /// <summary>
    /// Soft delete a product by setting IsActive to false instead of removing from database
    /// </summary>
    /// <param name="productId">The unique identifier of the product to delete</param>
    /// <returns>Success status or not found error</returns>
    [MapToApiVersion(1)]
    [HttpDelete("{productId}")]
    public async Task<IActionResult> DeleteProductAsync(Guid productId)
    {
        var serviceResponse = await _productService.DeleteProduct(productId);
        
        if (!serviceResponse.Success)
        {
            return NotFound(serviceResponse.ToBaseApiResponse());
        }
        
        return NoContent();
    }
}
