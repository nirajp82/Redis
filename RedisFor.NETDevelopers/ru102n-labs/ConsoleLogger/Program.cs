using ConsoleLogger;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Diagnostics;

// Create a logger factory
var loggerFactory = LoggerFactory.Create(builder =>
{
    // Add console logging
    builder.AddConsole();
});

// Create a custom TextWriter that redirects to the logger
var logTextWriter = new LoggerTextWriter(loggerFactory.CreateLogger("RedisLogger"));

var options = new ConfigurationOptions
{
    EndPoints = { "localhost:6379" },
};
//This will write the log information about the connection. See log.txt file fore more info.
var muxer = await ConnectionMultiplexer.ConnectAsync(options, logTextWriter);

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

await UnPipelined(); //1000 un - pipelined commands took: 1705ms to execute

// Close the connection when done
muxer.Close();