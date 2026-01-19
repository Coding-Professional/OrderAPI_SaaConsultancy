namespace OrderApi_SaaConsultancy.Services;

/// <summary>
/// Thread-safe inventory management service
/// </summary>
public class InventoryService
{
    // In-memory stock storage
    private readonly Dictionary<int, int> _stock = new();
    
    // Lock object for thread safety
    private readonly object _lock = new();

    public InventoryService()
    {
        // Initialize with some sample stock for testing
        _stock[1] = 100;
        _stock[2] = 50;
        _stock[3] = 75;
        _stock[4] = 200;
        _stock[5] = 30;
    }

    /// <summary>
    /// Thread-safe method to reserve stock
    /// </summary>
    /// <param name="productId">Product identifier</param>
    /// <param name="quantity">Quantity to reserve</param>
    /// <returns>True if reservation successful, false otherwise</returns>
    public bool ReserveStock(int productId, int quantity)
    {
        // Thread-safe block - only one thread can execute this at a time
        lock (_lock)
        {
            // Check if product exists
            if (!_stock.ContainsKey(productId))
            {
                return false;
            }

            // Check if sufficient stock available
            if (_stock[productId] < quantity)
            {
                return false;
            }

            // Reserve (deduct) stock
            _stock[productId] -= quantity;
            return true;
        }
    }

    /// <summary>
    /// Get current stock level for a product
    /// </summary>
    public int GetStock(int productId)
    {
        lock (_lock)
        {
            return _stock.TryGetValue(productId, out var stock) ? stock : 0;
        }
    }

    /// <summary>
    /// Add or update stock for a product (useful for testing)
    /// </summary>
    public void SetStock(int productId, int quantity)
    {
        lock (_lock)
        {
            _stock[productId] = quantity;
        }
    }
}
