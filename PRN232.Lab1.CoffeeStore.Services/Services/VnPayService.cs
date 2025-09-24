using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PRN232.Lab1.CoffeeStore.Repositories.Enums;
using PRN232.Lab1.CoffeeStore.Repositories.Interfaces;
using PRN232.Lab1.CoffeeStore.Repositories.Models;
using PRN232.Lab1.CoffeeStore.Services.Interfaces.Services;
using PRN232.Lab1.CoffeeStore.Services.Models.Responses;
using PRN232.Lab1.CoffeeStore.Services.Utils;
using VNPAY.NET;
using VNPAY.NET.Enums;
using VNPAY.NET.Models;

namespace PRN232.Lab1.CoffeeStore.Services.Services;

public class VnPayService : IVnPayService
{
    private readonly ILogger<VnPayService> _logger;
    private readonly IVnpay _vnpay;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IOrderService _orderService;
    private readonly IUnitOfWork _unitOfWork;

    public VnPayService(
        ILogger<VnPayService> logger,
        IVnpay vnpay, 
        IConfiguration configuration, 
        IHttpContextAccessor httpContextAccessor, 
        IOrderService orderService,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _vnpay = vnpay;
        _httpContextAccessor = httpContextAccessor;
        _orderService = orderService;
        _unitOfWork = unitOfWork;
        
        _vnpay.Initialize(
            configuration["Vnpay:TmnCode"]!,
            configuration["Vnpay:HashSecret"]!, 
            configuration["Vnpay:BaseUrl"]!, 
            configuration["Vnpay:ReturnUrl"]!);
    }
    
    public async Task<DataServiceResponse<string>> GetPaymentUrl(Guid orderId)
    {
        _logger.LogInformation("GetPaymentUrl called with orderId: {OrderId}", orderId);
        var serviceResponse = await _orderService.GetOrderById(orderId);
        if (!serviceResponse.Success || serviceResponse.Data!.Status != nameof(OrderStatus.Pending))
        {
            return new DataServiceResponse<string>()
            {
                Success = false,
                Message = "Order not found or not in pending status",
                Data = string.Empty,
            };
        }
        
        var totalAmount = serviceResponse.Data!.OrderItems.Sum(od => od.Quantity * od.UnitPrice);
        var request = new PaymentRequest
        {
            PaymentId = DateTime.Now.Ticks,
            Money = (double) totalAmount,
            Description = orderId.ToString(),
            IpAddress = NetworkUtils.GetIpAddress(_httpContextAccessor.HttpContext!),
            BankCode = BankCode.ANY,
            CreatedDate = DateTime.Now,
            Currency = Currency.VND,
            Language = DisplayLanguage.Vietnamese
        };

        var paymentUrl = _vnpay.GetPaymentUrl(request);
        _logger.LogInformation("Payment URL generated for order {OrderId}: {PaymentUrl}", orderId, paymentUrl);
        
        return new DataServiceResponse<string>()
        {
            Success = true,
            Message = "Successfully get payment url",
            Data = paymentUrl
        };
    }

    public async Task<BaseServiceResponse> ProcessIpnAction(IQueryCollection queryParams)
    {
        var paymentResult = _vnpay.GetPaymentResult(queryParams);
        
        if (!paymentResult.IsSuccess)
        {
            _logger.LogWarning("Payment failed with result: {PaymentResult}", paymentResult);
            return new BaseServiceResponse()
            {
                Success = false,
                Message = paymentResult.PaymentResponse.Description
            };
        }
        var orderId = paymentResult.Description;
        _logger.LogInformation("Processing payment for orderId: {OrderId}", orderId);

        var orderRepository = _unitOfWork.GetRepository<Order, Guid>();
        var paymentRepository = _unitOfWork.GetRepository<Payment, Guid>();

        var order = await orderRepository.GetByIdAsync(Guid.Parse(orderId));
        if (order == null)
        {
            _logger.LogError("Order entity not found for orderId: {OrderId}", orderId);
            return new BaseServiceResponse()
            {
                Success = false,
                Message = $"Order with id {orderId} not found"
            };
        }

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.Parse(orderId),
            Amount = decimal.Parse(queryParams["vnp_Amount"]!) / 100,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = PaymentMethod.VnPay
        };

        await paymentRepository.AddAsync(payment);

        order.PaymentId = payment.Id;
        order.Status = OrderStatus.Completed;
        order.UpdatedAt = DateTime.UtcNow;

        await orderRepository.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Payment processed successfully for orderId: {OrderId}, paymentId: {PaymentId}", orderId, payment.Id);

        return new BaseServiceResponse()
        {
            Success = true,
            Message = $"Successfully process IPN action for orderId: {orderId}"
        };
    }
}