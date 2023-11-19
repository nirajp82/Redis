using StackExchange.Redis;

var muxer = ConnectionMultiplexer.Connect(new ConfigurationOptions
{
    EndPoints = { "localhost:6379" }
});

var db = muxer.GetDatabase();
var res = db.Ping();
Console.WriteLine($"The ping took: {res.TotalMilliseconds} ms");