using StackExchange.Redis;

var muxer = await ConnectionMultiplexer.ConnectAsync(new ConfigurationOptions
{
    EndPoints = { "localhost:6379" }
});

var db = muxer.GetDatabase();