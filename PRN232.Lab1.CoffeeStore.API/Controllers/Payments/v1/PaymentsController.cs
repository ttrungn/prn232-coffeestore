using Asp.Versioning;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PRN232.Lab1.CoffeeStore.API.Mappers;
using PRN232.Lab1.CoffeeStore.API.Models.Responses;
using PRN232.Lab1.CoffeeStore.Services.Interfaces.Services;
using PRN232.Lab1.CoffeeStore.Services.Models.Requests;
using VNPAY.NET.Models;

namespace PRN232.Lab1.CoffeeStore.API.Controllers.Payments.v1;

[ApiVersion(1)]
[ApiController]
[Route("api/v{v:apiVersion}/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IVnPayService _vnPayService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public PaymentsController(IVnPayService vnPayService, IWebHostEnvironment webHostEnvironment)
    {
        _vnPayService = vnPayService;
        _webHostEnvironment = webHostEnvironment;
    }

    /// <summary>
    /// Generate VNPay payment URL for a pending order
    /// </summary>
    /// <param name="orderId">The unique identifier of the order to create payment for</param>
    /// <param name="request">Base service request parameters</param>
    /// <returns>VNPay payment URL for order checkout or error if order is not pending</returns>
    [HttpGet("url")]
    public async Task<IActionResult> GetPaymentUrl([FromQuery] Guid orderId, [FromQuery] BaseServiceRequest request)
    {
        var serviceResponse = await _vnPayService.GetPaymentUrl(orderId);
        if (!serviceResponse.Success)
        {
            return BadRequest(serviceResponse.ToBaseApiResponse());
        }
        return Ok(serviceResponse.ToDataApiResponse());
    }

    /// <summary>
    /// Payment callback endpoint that displays payment result HTML page with transaction details
    /// </summary>
    /// <returns>HTML page showing payment success/failure status with Vietnamese language support</returns>
    [HttpGet("callback")]
    public IActionResult PaymentCallback()
    {   
        var htmlFilePath = Path.Combine(_webHostEnvironment.ContentRootPath, "Views", "payment-result.html");
        
        if (!System.IO.File.Exists(htmlFilePath))
        {
            return NotFound("Payment result page not found");
        }

        var htmlContent = System.IO.File.ReadAllText(htmlFilePath);
        
        return Content(htmlContent, "text/html");
    }

    /// <summary>
    /// VNPay IPN (Instant Payment Notification) endpoint for processing payment results
    /// </summary>
    /// <returns>Payment processing result after updating order status and creating payment record</returns>
    [HttpGet("ipn-action")]
    public async Task<IActionResult> IpnAction()
    {
        if (!Request.QueryString.HasValue)
            return NotFound(new BaseApiResponse()
            {
                Success = false,
                Message = "Cannot find payment information"
            });
        
        var serviceResponse = await _vnPayService.ProcessIpnAction(Request.Query);
        if (!serviceResponse.Success)
        {
            return BadRequest(serviceResponse.ToBaseApiResponse());
        }

        return Ok(serviceResponse.ToBaseApiResponse());
    }
}