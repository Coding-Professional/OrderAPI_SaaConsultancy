using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace OrderApi_SaaConsultancy.DTOs;

public class CreateOrderDto
{
    [Required]
    [Range(1, 999999, ErrorMessage = "CustomerId must be between 1 and 999999")]
    [DefaultValue(123)]
    public int CustomerId { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one item is required")]
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    [Required]
    [Range(1, 999999, ErrorMessage = "ProductId must be between 1 and 999999")]
    [DefaultValue(1)]
    public int ProductId { get; set; }

    [Required]
    [Range(1, 10000, ErrorMessage = "Quantity must be between 1 and 10000")]
    [DefaultValue(6)]
    public int Quantity { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    [DefaultValue(100.00)]
    public decimal Price { get; set; }
}
