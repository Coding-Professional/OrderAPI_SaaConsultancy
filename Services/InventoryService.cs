namespace OrderApi_SaaConsultancy.Services;

public class InventoryService
{
    private readonly Dictionary<int, int> _stock = new();
    private readonly object _lock = new();

    public InventoryService()
    {
        _stock[1] = 100;
        _stock[2] = 50;
        _stock[3] = 75;
        _stock[4] = 200;
        _stock[5] = 30;
    }

    public bool ReserveStock(int productId, int quantity)
    {
        lock (_lock)
        {
            if (!_stock.ContainsKey(productId))
                return false;

            if (_stock[productId] < quantity)
                return false;

            _stock[productId] -= quantity;
            return true;
        }
    }

    public int GetStock(int productId)
    {
        lock (_lock)
        {
            return _stock.TryGetValue(productId, out var stock) ? stock : 0;
        }
    }

    public void SetStock(int productId, int quantity)
    {
        lock (_lock)
        {
            _stock[productId] = quantity;
        }
    }
}
