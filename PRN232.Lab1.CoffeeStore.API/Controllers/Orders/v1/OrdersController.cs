using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232.Lab1.CoffeeStore.API.Mappers;
using PRN232.Lab1.CoffeeStore.Repositories.Constants;
using PRN232.Lab1.CoffeeStore.Repositories.Enums;
using PRN232.Lab1.CoffeeStore.Services.Interfaces.Services;
using PRN232.Lab1.CoffeeStore.Services.Models.Requests;

namespace PRN232.Lab1.CoffeeStore.API.Controllers.Orders.v1;

[ApiVersion(1)]
[ApiController]
[Route("api/v{v:apiVersion}/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }
    
    /// <summary>
    /// Get all orders with optional filtering by user, status, and date range
    /// </summary>
    /// <param name="request">Query parameters for filtering and pagination including UserId, Status, FromDate, ToDate</param>
    /// <returns>Paginated list of orders matching the filter criteria</returns>
    [MapToApiVersion(1)]
    [HttpGet("")]
    public async Task<IActionResult> GetOrdersAsync([FromQuery] GetOrdersRequest request)
    {
        var serviceResponse = await _orderService.GetOrders(request);

        return Ok(serviceResponse.ToDataApiResponse(Request, Response));
    }

    /// <summary>
    /// Get detailed information about a specific order including items and payment details
    /// </summary>
    /// <param name="orderId">The unique identifier of the order</param>
    /// <param name="request">Base service requestV2 parameters</param>
    /// <returns>Complete order details with items, payment information, and totals</returns>
    [MapToApiVersion(1)]
    [HttpGet("{orderId:guid}")]
    public async Task<IActionResult> GetOrderByIdAsync([FromRoute] Guid orderId, [FromQuery] BaseServiceRequest request)
    {
        var serviceResponse = await _orderService.GetOrderById(orderId);
        
        return Ok(serviceResponse.ToDataApiResponse());
    }
    
    /// <summary>
    /// Create a new order with unique products (requires Customer role authentication)
    /// </summary>
    /// <param name="request">Order creation details with list of products and quantities</param>
    /// <returns>Created order ID or validation errors if duplicate products found</returns>
    [Authorize(Roles = Roles.Customer)]
    [MapToApiVersion(1)]
    [HttpPost("")]
    public async Task<IActionResult> CreateOrderAsync([FromBody] CreateOrderRequest request)
    {
        request.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("User not found");
        var serviceResponse = await _orderService.CreateOrder(request);
        
        if (serviceResponse.Success)
        {
            return Created($"/api/v1/orders/{serviceResponse.ToDataApiResponse().Data}", null);
        }
        
        return BadRequest(serviceResponse.ToBaseApiResponse());
    }
    
    /// <summary>
    /// Update an existing order's items (only allowed for orders in Editing status)
    /// </summary>
    /// <param name="orderId">The unique identifier of the order to update</param>
    /// <param name="request">Updated order details with new product list</param>
    /// <returns>Success status or validation errors</returns>
    [Authorize(Roles = Roles.Customer)]
    [MapToApiVersion(1)]
    [HttpPut("{orderId:guid}")]
    public async Task<IActionResult> UpdateOrderAsync([FromRoute] Guid orderId, [FromBody] UpdateOrderRequest request)
    {
        var serviceResponse = await _orderService.UpdateOrder(orderId, request);
        
        if (serviceResponse.Success)
        {
            return Ok(serviceResponse.ToBaseApiResponse());
        }
        
        return BadRequest(serviceResponse.ToBaseApiResponse());
    }

    /// <summary>
    /// Cancel an order by changing its status to Cancelled
    /// </summary>
    /// <param name="orderId">The unique identifier of the order to cancel</param>
    /// <returns>Success status or error if order cannot be cancelled</returns>
    [Authorize(Roles = Roles.Customer)]
    [MapToApiVersion(1)]
    [HttpPatch("{orderId:guid}/cancel")]
    public async Task<IActionResult> CancelOrderAsync([FromRoute] Guid orderId)
    {
        var serviceResponse = await _orderService.UpdateOrderStatus(orderId, OrderStatus.Cancelled);
        
        if (serviceResponse.Success)
        {
            return Ok(serviceResponse.ToBaseApiResponse());
        }
        
        return BadRequest(serviceResponse.ToBaseApiResponse());
    }
    
    /// <summary>
    /// Finalize order editing by changing status from Editing to Pending (ready for payment)
    /// </summary>
    /// <param name="orderId">The unique identifier of the order to finalize</param>
    /// <returns>Success status or error if order is not in editing state</returns>
    [Authorize(Roles = Roles.Customer)]
    [MapToApiVersion(1)]
    [HttpPatch("{orderId:guid}/finalize")]
    public async Task<IActionResult> FinalizeOrderEditingAsync([FromRoute] Guid orderId)
    {
        var serviceResponse = await _orderService.UpdateOrderStatus(orderId, OrderStatus.Pending);
        
        if (serviceResponse.Success)
        {
            return Ok(serviceResponse.ToBaseApiResponse());
        }
        
        return BadRequest(serviceResponse.ToBaseApiResponse());
    }
}
