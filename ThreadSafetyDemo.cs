using OrderApi_SaaConsultancy.Services;

namespace OrderApi_SaaConsultancy.Tests;

/// <summary>
/// Demonstration of thread-safety testing for InventoryService
/// This shows how you would explain thread-safety in an interview
/// </summary>
public class InventoryServiceThreadSafetyDemo
{
    /// <summary>
    /// Test: Multiple threads trying to reserve stock concurrently
    /// Expected: No race conditions, all operations are atomic
    /// </summary>
    public static async Task DemonstrateThreadSafety()
    {
        var inventory = new InventoryService();
        
        // Set initial stock
        inventory.SetStock(1, 100);
        
        Console.WriteLine("Initial stock for Product 1: " + inventory.GetStock(1));
        Console.WriteLine("Starting 10 concurrent threads, each reserving 10 units...\n");

        // Create 10 tasks that will run concurrently
        var tasks = new List<Task>();
        var results = new bool[10];

        for (int i = 0; i < 10; i++)
        {
            int index = i;
            tasks.Add(Task.Run(() =>
            {
                results[index] = inventory.ReserveStock(1, 10);
                Console.WriteLine($"Thread {index + 1}: Reservation {(results[index] ? "SUCCESS" : "FAILED")}");
            }));
        }

        // Wait for all tasks to complete
        await Task.WhenAll(tasks);

        Console.WriteLine("\n--- Results ---");
        Console.WriteLine($"Successful reservations: {results.Count(r => r)}");
        Console.WriteLine($"Final stock for Product 1: {inventory.GetStock(1)}");
        Console.WriteLine($"Expected final stock: 0");
        Console.WriteLine($"Thread-safe: {inventory.GetStock(1) == 0}");
    }

    /// <summary>
    /// Test: Demonstrate what happens WITHOUT thread safety (for comparison)
    /// WARNING: This is intentionally broken to show the problem
    /// </summary>
    public static async Task DemonstrateRaceCondition()
    {
        Console.WriteLine("\n=== DEMONSTRATING RACE CONDITION (Without Lock) ===\n");
        
        var unsafeStock = new Dictionary<int, int> { [1] = 100 };
        
        var tasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                // UNSAFE: No lock, race condition possible
                if (unsafeStock[1] >= 10)
                {
                    Thread.Sleep(1); // Simulate processing time
                    unsafeStock[1] -= 10;
                }
            }));
        }

        await Task.WhenAll(tasks);

        Console.WriteLine($"Final stock (UNSAFE): {unsafeStock[1]}");
        Console.WriteLine($"Expected: 0, but might be negative or incorrect due to race conditions!");
    }

    /// <summary>
    /// Run all demonstrations
    /// </summary>
    public static async Task Main(string[] args)
    {
        Console.WriteLine("=== INVENTORY SERVICE THREAD-SAFETY DEMONSTRATION ===\n");
        
        await DemonstrateThreadSafety();
        await DemonstrateRaceCondition();
        
        Console.WriteLine("\n=== Key Takeaway ===");
        Console.WriteLine("The 'lock' statement ensures only ONE thread can modify stock at a time.");
        Console.WriteLine("This prevents race conditions and ensures data consistency.\n");
    }
}

/// <summary>
/// Alternative implementation using SemaphoreSlim (async-friendly)
/// Use this if you want to show knowledge of advanced patterns
/// </summary>
public class InventoryServiceWithSemaphore
{
    private readonly Dictionary<int, int> _stock = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1); // Only 1 thread at a time

    public InventoryServiceWithSemaphore()
    {
        _stock[1] = 100;
    }

    /// <summary>
    /// Async thread-safe method using SemaphoreSlim
    /// </summary>
    public async Task<bool> ReserveStockAsync(int productId, int quantity)
    {
        // Wait for semaphore (async-friendly alternative to lock)
        await _semaphore.WaitAsync();
        try
        {
            if (!_stock.ContainsKey(productId) || _stock[productId] < quantity)
            {
                return false;
            }

            _stock[productId] -= quantity;
            return true;
        }
        finally
        {
            // Always release the semaphore
            _semaphore.Release();
        }
    }

    public async Task<int> GetStockAsync(int productId)
    {
        await _semaphore.WaitAsync();
        try
        {
            return _stock.TryGetValue(productId, out var stock) ? stock : 0;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}

/*
 * INTERVIEW TALKING POINTS:
 * 
 * 1. Why use 'lock' over SemaphoreSlim?
 *    - Simpler for synchronous operations
 *    - Less overhead
 *    - Easier to understand and maintain
 * 
 * 2. When would you use SemaphoreSlim?
 *    - When you need async locking
 *    - When lock duration might be long
 *    - When you need to limit concurrent access to N threads (not just 1)
 * 
 * 3. Why Singleton for InventoryService?
 *    - Stock must be shared across all requests
 *    - Single source of truth for inventory
 *    - In production: would use Redis or database
 * 
 * 4. What are the limitations?
 *    - Only works in single-server deployments
 *    - In-memory data is lost on restart
 *    - For production: use distributed cache (Redis) or database with transactions
 * 
 * 5. How to test thread-safety?
 *    - Create multiple concurrent tasks
 *    - Verify final state is consistent
 *    - Check no negative stock values
 *    - Use stress testing tools
 */
