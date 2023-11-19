using StackExchange.Redis;
using System.Diagnostics;

var options = new ConfigurationOptions
{
    EndPoints =
    {
        "localhost:6379"
    }
};

var muxer = await ConnectionMultiplexer.ConnectAsync(options);
var db = muxer.GetDatabase();
async Task UnPipelined()
{
    var stopWatch = Stopwatch.StartNew();
    for (int i = 0; i < 1000; i++)
    {
        await db.PingAsync();
    }
    Console.WriteLine($"1000 un-pipelined commands took: {stopWatch.ElapsedMilliseconds}ms to execute");
}

async Task Pipelined()
{
    var pingTasks = new List<Task<TimeSpan>>();
    var stopWatch = Stopwatch.StartNew();
    for (int i = 0; i < 1000; i++)
    {
        pingTasks.Add(db.PingAsync());
    }
    await Task.WhenAll(pingTasks);
    Console.WriteLine($"1000 automatically pipelined tasks took: {stopWatch.ElapsedMilliseconds}ms to execute," +
        $" first result: {pingTasks[0].Result}");
}

async Task Batch() 
{
    var pingTasks = new List<Task<TimeSpan>>();
    IBatch batch = db.CreateBatch();
    var stopWatch = Stopwatch.StartNew();
    for (var i = 0; i < 1000; i++)
    {
        pingTasks.Add(batch.PingAsync());
    }
    batch.Execute();
    await Task.WhenAll(pingTasks);
    Console.WriteLine($"1000 batched commands took: {stopWatch.ElapsedMilliseconds}ms to execute, first result: {pingTasks[0].Result}");
}

await UnPipelined(); //1000 un - pipelined commands took: 1705ms to execute
await Pipelined(); //1000 automatically pipelined tasks took: 18ms to execute, first result: 00:00:00.0101917
await Batch(); //1000 batched commands took: 26ms to execute, first result: 00:00:00.0074689