using StackExchange.Redis;
using System.Runtime.Intrinsics.X86;

var muxer = await ConnectionMultiplexer.ConnectAsync(new ConfigurationOptions
{
    EndPoints = { "localhost:6379" }
});

var channelName = "PubSubDemoChannel";
var subscriber = muxer.GetSubscriber();
var db = muxer.GetDatabase();
var cancellationTokenSource = new CancellationTokenSource();
var token = cancellationTokenSource.Token;
await subscriber.UnsubscribeAllAsync();
await subscriber.UnsubscribeAsync(channelName);

var simpleSequentialAsync = async () =>
{
    var channel = await subscriber.SubscribeAsync(channelName);
    channel.OnMessage(msg =>
    {
        Console.WriteLine($"Sequentially received: {msg.Message} on channel: {msg.Channel}");
    });
};

var simpleConcurrentAsync = async () =>
{
    await subscriber.SubscribeAsync(channelName, (channel, value) =>
    {
        Console.WriteLine($"Concurrent Received: {value} on channel: {channel}");
    });
};

var basicProducerAsync = async () =>
{
    var cnt = 0;
    while (!token.IsCancellationRequested)
    {
        await subscriber.PublishAsync(channelName, DateTime.Now.AddMinutes(++cnt).ToString());
        await Task.Delay(5000);
    }
};

var patternSubscriberAsync = async () =>
{
    await subscriber.SubscribeAsync("MyGuidPattern:*", (channel, value) =>
    {
        Console.WriteLine($"Concurrent Received: {value} on channel: {channel}");
    });
};

var patternProducerAsync = async () =>
{
    var i = 0;
    while (!token.IsCancellationRequested)
    {
        await subscriber.PublishAsync($"MyGuidPattern:{Guid.NewGuid()}", $"My pattern pub sub message is :{i++}");
        await Task.Delay(1000);
    }
};


var allChannellSubscriberAsync = async () =>
{
    await subscriber.SubscribeAsync("*", (channel, value) =>
    {
        Console.WriteLine($"** AllChannell Pattern Subscription: {value} on channel: {channel}");
    });
};

var cancelTaskAsync = async () =>
{
    await Task.Delay(10000);
    cancellationTokenSource.Cancel();
};

await Task.WhenAll(
    allChannellSubscriberAsync(),
    simpleSequentialAsync(), simpleConcurrentAsync(),
    patternSubscriberAsync(),
    basicProducerAsync(),
    patternProducerAsync(),
    cancelTaskAsync()
 );