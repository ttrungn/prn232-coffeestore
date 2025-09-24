using Microsoft.EntityFrameworkCore;
using PRN232.Lab1.CoffeeStore.Repositories.Enums;
using PRN232.Lab1.CoffeeStore.Services.Interfaces.Services;
using PRN232.Lab1.CoffeeStore.Services.Mappers;
using PRN232.Lab1.CoffeeStore.Services.Models.Requests;
using PRN232.Lab1.CoffeeStore.Services.Models.Responses;
using PRN232.Lab1.CoffeeStore.Repositories.Interfaces;
using PRN232.Lab1.CoffeeStore.Repositories.Models;

namespace PRN232.Lab1.CoffeeStore.Services.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DataServiceResponse<PaginationResponse<OrderResponse>>> GetOrders(GetOrdersRequest request)
    {
        var orderRepository = _unitOfWork.GetRepository<Order, Guid>();
        var query = orderRepository.Query()
            .Where(o => o.IsActive) // Only get active orders
            .Include(o => o.OrderDetails)
            .AsNoTracking();

        // Filter by UserId if provided
        if (!string.IsNullOrWhiteSpace(request.UserId))
            query = query.Where(o => o.UserId == request.UserId);

        // Filter by Status if provided
        if (request.Status != null)
            query = query.Where(o => o.Status == request.Status);

        // Filter by date range
        if (request.FromDate.HasValue)
            query = query.Where(o => o.OrderDate >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(o => o.OrderDate <= request.ToDate.Value);

        var totalOrders = await query.CountAsync();

        var orderResponses = await query
            .OrderByDescending(o => o.OrderDate)
            .Skip(request.Page * request.PageSize)
            .Take(request.PageSize)
            .Select(o => o.ToOrderResponse())
            .ToListAsync();

        var paginationResponse = new PaginationResponse<OrderResponse>()
        {
            TotalResults = totalOrders,
            TotalCurrentResults = orderResponses.Count,
            Page = request.Page,
            Results = orderResponses
        };

        return new DataServiceResponse<PaginationResponse<OrderResponse>>()
        {
            Success = true,
            Message = "Get orders successfully",
            Data = paginationResponse,
        };
    }

    public async Task<DataServiceResponse<OrderDetailsResponse?>> GetOrderById(Guid orderId)
    {
        var orderRepository = _unitOfWork.GetRepository<Order, Guid>();
        var order = await orderRepository.Query()
            .Where(o => o.IsActive) // Only get active orders
            .Include(o => o.OrderDetails.Where(od => od.IsActive)) // Only include active order details
                .ThenInclude(od => od.Product)
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            return new DataServiceResponse<OrderDetailsResponse?>()
            {
                Success = false,
                Message = $"Order with id {orderId} not found",
                Data = null,
            };
        }

        return new DataServiceResponse<OrderDetailsResponse?>()
        {
            Success = true,
            Message = "Get order successfully",
            Data = order.ToOrderDetailsResponse(),
        };
    }

    public async Task<DataServiceResponse<Guid>> CreateOrder(CreateOrderRequest request)
    {
        var orderRepository = _unitOfWork.GetRepository<Order, Guid>();
        var productRepository = _unitOfWork.GetRepository<Product, Guid>();
        var orderDetailRepository = _unitOfWork.GetRepository<OrderDetail, Guid>();

        // Validate all products exist and are active
        var productIds = request.OrderItems.Select(oi => oi.ProductId).ToList();
        var existingProducts = await productRepository.Query()
            .Where(p => productIds.Contains(p.Id) && p.IsActive)
            .ToListAsync();

        var missingProductIds = productIds.Except(existingProducts.Select(p => p.Id)).ToList();
        if (missingProductIds.Any())
        {
            return new DataServiceResponse<Guid>()
            {
                Success = false,
                Message = $"Products not found or inactive: {string.Join(", ", missingProductIds)}",
                Data = Guid.Empty
            };
        }

        // Validate quantities
        if (request.OrderItems.Any(oi => oi.Quantity <= 0))
        {
            return new DataServiceResponse<Guid>()
            {
                Success = false,
                Message = "All order items must have quantity greater than 0",
                Data = Guid.Empty
            };
        }

        // Create order
        var order = request.ToOrder();
        await orderRepository.AddAsync(order);

        // Create order details
        var orderDetails = request.OrderItems.Select(oi =>
        {
            var product = existingProducts.First(p => p.Id == oi.ProductId);
            return new OrderDetail
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = oi.ProductId,
                Quantity = oi.Quantity,
                UnitPrice = product.Price,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }).ToList();

        foreach (var orderDetail in orderDetails)
        {
            await orderDetailRepository.AddAsync(orderDetail);
        }

        await _unitOfWork.SaveChangesAsync();

        return new DataServiceResponse<Guid>()
        {
            Success = true,
            Message = "Order created successfully",
            Data = order.Id
        };
    }

    public async Task<BaseServiceResponse> UpdateOrder(Guid orderId, UpdateOrderRequest request)
    {
        var orderRepository = _unitOfWork.GetRepository<Order, Guid>();
        var productRepository = _unitOfWork.GetRepository<Product, Guid>();
        var orderDetailRepository = _unitOfWork.GetRepository<OrderDetail, Guid>();

        var order = await orderRepository.Query()
            .Include(o => o.OrderDetails)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.IsActive);

        if (order == null)
        {
            return new BaseServiceResponse()
            {
                Success = false,
                Message = $"Order with id {orderId} not found"
            };
        }

        if (order.Status != OrderStatus.Editing)
        {
            return new BaseServiceResponse()
            {
                Success = false,
                Message = "Only editing orders can be updated"
            };
        }

        // Validate all products exist and are active
        var productIds = request.OrderItems.Select(oi => oi.ProductId).ToList();
        var existingProducts = await productRepository.Query()
            .Where(p => productIds.Contains(p.Id) && p.IsActive)
            .ToListAsync();

        var missingProductIds = productIds.Except(existingProducts.Select(p => p.Id)).ToList();
        if (missingProductIds.Any())
        {
            return new BaseServiceResponse()
            {
                Success = false,
                Message = $"Products not found or inactive: {string.Join(", ", missingProductIds)}"
            };
        }

        // Validate quantities
        if (request.OrderItems.Any(oi => oi.Quantity <= 0))
        {
            return new BaseServiceResponse()
            {
                Success = false,
                Message = "All order items must have quantity greater than 0"
            };
        }

        // Remove existing order details
        var existingOrderDetails = order.OrderDetails.ToList();
        foreach (var existingDetail in existingOrderDetails)
        {
            await orderDetailRepository.RemoveAsync(existingDetail);
        }

        // Create new order details
        var newOrderDetails = request.OrderItems.Select(oi =>
        {
            var product = existingProducts.First(p => p.Id == oi.ProductId);
            return new OrderDetail
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                ProductId = oi.ProductId,
                Quantity = oi.Quantity,
                UnitPrice = product.Price,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }).ToList();

        foreach (var orderDetail in newOrderDetails)
        {
            await orderDetailRepository.AddAsync(orderDetail);
        }

        // Update order timestamp
        order.UpdateOrder(request);
        await orderRepository.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync();

        return new BaseServiceResponse()
        {
            Success = true,
            Message = "Order updated successfully"
        };
    }

    public async Task<BaseServiceResponse> UpdateOrderStatus(Guid orderId, OrderStatus status)
    {
        var orderRepository = _unitOfWork.GetRepository<Order, Guid>();

        var order = await orderRepository.Query()
            .FirstOrDefaultAsync(o => o.Id == orderId && o.IsActive);

        if (order == null)
        {
            return new BaseServiceResponse()
            {
                Success = false,
                Message = $"Order with id {orderId} not found"
            };
        }

        if (order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Completed)
        {
            return new BaseServiceResponse()
            {
                Success = false,
                Message = "Order is already cancelled or success"
            };
        }

        if (order.Status == OrderStatus.Pending && status == OrderStatus.Editing)
        {
            return new BaseServiceResponse()
            {
                Success = false,
                Message = "Pending orders can't be edited"
            };
        }

        if (order.Status == OrderStatus.Editing && status != OrderStatus.Pending)
        {
            return new BaseServiceResponse()
            {
                Success = false,
                Message = "Editing orders can't be cancelled or success"
            };
        }

        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;

        await orderRepository.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync();

        return new BaseServiceResponse()
        {
            Success = true,
            Message = "Order cancelled successfully"
        };
    }
}


