using Microsoft.EntityFrameworkCore;
using OrderApi_SaaConsultancy.Data;
using OrderApi_SaaConsultancy.DTOs;
using OrderApi_SaaConsultancy.Entities;

namespace OrderApi_SaaConsultancy.Services;

public class OrderService
{
    private readonly AppDbContext _context;
    private readonly InventoryService _inventoryService;

    public OrderService(AppDbContext context, InventoryService inventoryService)
    {
        _context = context;
        _inventoryService = inventoryService;
    }

    public async Task<OrderResponseDto> CreateOrderAsync(CreateOrderDto dto)
    {
        decimal total = dto.Items.Sum(item => item.Quantity * item.Price);
        decimal discount = total > 500 ? total * 0.10m : 0;
        decimal finalTotal = total - discount;

        var order = new Order
        {
            CustomerId = dto.CustomerId,
            Total = total,
            Discount = discount,
            FinalTotal = finalTotal,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        foreach (var itemDto in dto.Items)
        {
            if (!_inventoryService.ReserveStock(itemDto.ProductId, itemDto.Quantity))
            {
                throw new InvalidOperationException(
                    $"Insufficient stock for product {itemDto.ProductId}");
            }

            var orderItem = new OrderItem
            {
                ProductId = itemDto.ProductId,
                Quantity = itemDto.Quantity,
                Price = itemDto.Price,
                Subtotal = itemDto.Quantity * itemDto.Price
            };

            order.Items.Add(orderItem);
        }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return MapToResponseDto(order);
    }

    public async Task<OrderResponseDto?> GetOrderByIdAsync(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        return order == null ? null : MapToResponseDto(order);
    }

    public async Task<List<OrderResponseDto>> GetAllOrdersAsync()
    {
        var orders = await _context.Orders
            .Include(o => o.Items)
            .ToListAsync();

        return orders.Select(MapToResponseDto).ToList();
    }

    private OrderResponseDto MapToResponseDto(Order order)
    {
        return new OrderResponseDto
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            Total = order.Total,
            Discount = order.Discount,
            FinalTotal = order.FinalTotal,
            Status = order.Status,
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(item => new OrderItemResponseDto
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = item.Price,
                Subtotal = item.Subtotal
            }).ToList()
        };
    }
}
