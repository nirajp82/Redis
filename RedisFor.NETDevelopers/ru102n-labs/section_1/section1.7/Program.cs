using StackExchange.Redis;
using System.Diagnostics;

class Program
{
    static async Task Main()
    {
        try
        {
            Task tA = TransactionAsync(true, "ClientA");
            Task tB = TransactionAsync(false, "ClientB");
            await Task.WhenAll(tA, tB);
            Console.WriteLine($"Done");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    static async Task TransactionAsync(bool delay, string name)
    {
        string key = "Count";

        var connMuxer = await ConnectionMultiplexer.ConnectAsync(new ConfigurationOptions
        {
            EndPoints = { "localhost:6379" }
        });

        var db = connMuxer.GetDatabase();

        ITransaction trans = db.CreateTransaction();

        //Note: Do not await here. https://stackoverflow.com/questions/25976231/stackexchange-redis-transaction-methods-freezes
        //Commands executed inside a transaction do not return results until after you execute the transaction.
        //This is simply a feature of how transactions work in Redis.
        // At the moment awaiting something that hasn't even been sent yet (transactions are buffered locally until executed)
        // - but even if it had been sent: results simply aren't available until the transaction completes.
        var _ = trans.StringGetAsync(key);
        var __ = trans.StringIncrementAsync(key);
        bool success = await trans.ExecuteAsync();
        if (success)
            Console.WriteLine($"Counter incremented by {name}!");
        else
            Console.WriteLine($"Transaction for {name} failed. Retry or handle accordingly.");
    }
}