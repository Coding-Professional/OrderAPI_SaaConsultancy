using Microsoft.AspNetCore.Mvc;
using OrderApi_SaaConsultancy.Services;

namespace OrderApi_SaaConsultancy.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly InventoryService _inventoryService;
    private readonly ILogger<InventoryController> _logger;

    public InventoryController(InventoryService inventoryService, ILogger<InventoryController> logger)
    {
        _inventoryService = inventoryService;
        _logger = logger;
    }

    /// <summary>
    /// Get current stock for a product
    /// </summary>
    [HttpGet("{productId}")]
    public ActionResult<int> GetStock(int productId)
    {
        var stock = _inventoryService.GetStock(productId);
        return Ok(new { productId, stock });
    }

    /// <summary>
    /// Manually reserve stock (for testing thread-safety)
    /// </summary>
    [HttpPost("reserve")]
    public ActionResult ReserveStock([FromBody] ReserveStockRequest request)
    {
        var success = _inventoryService.ReserveStock(request.ProductId, request.Quantity);
        
        if (success)
        {
            _logger.LogInformation("Reserved {Quantity} units of product {ProductId}", 
                request.Quantity, request.ProductId);
            
            return Ok(new 
            { 
                success = true, 
                message = $"Reserved {request.Quantity} units",
                remainingStock = _inventoryService.GetStock(request.ProductId)
            });
        }

        _logger.LogWarning("Failed to reserve {Quantity} units of product {ProductId}", 
            request.Quantity, request.ProductId);
        
        return BadRequest(new 
        { 
            success = false, 
            message = "Insufficient stock or product not found" 
        });
    }

    /// <summary>
    /// Set stock level (for testing)
    /// </summary>
    [HttpPut("{productId}")]
    public ActionResult SetStock(int productId, [FromBody] SetStockRequest request)
    {
        _inventoryService.SetStock(productId, request.Quantity);
        return Ok(new { productId, stock = request.Quantity });
    }
}

public class ReserveStockRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class SetStockRequest
{
    public int Quantity { get; set; }
}
