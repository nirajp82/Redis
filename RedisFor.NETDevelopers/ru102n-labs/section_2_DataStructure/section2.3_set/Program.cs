using Microsoft.Extensions.Logging;
using StackExchange.Redis;

// Create a logger factory
var loggerFactory = LoggerFactory.Create(builder =>
{
    // Add console logging
    builder.AddConsole();
});

var options = new ConfigurationOptions
{
    EndPoints = { "localhost:6379" },
};
var muxer = await ConnectionMultiplexer.ConnectAsync(options);

var db = muxer.GetDatabase();
var allUsersSet = "users";
var activeUsersSet = "users:state:active";
var inactiveUsersSet = "users:state:inactive";
var offlineUsersSet = "users:state:offline";

await db.KeyDeleteAsync(new RedisKey[] { allUsersSet, activeUsersSet, inactiveUsersSet, offlineUsersSet });

var PopulateSubSets = async () =>
{
    var tsk1 = db.SetAddAsync(activeUsersSet, new RedisValue[] { "User:1", "User:2" });
    var tsk2 = db.SetAddAsync(inactiveUsersSet, new RedisValue[] { "User:3", "User:4" });
    var tsk3 = db.SetAddAsync(offlineUsersSet, new RedisValue[] { "User:5", "User:6" });
    await Task.WhenAll(tsk1, tsk2, tsk3);
};

var CombiningSets = async () =>
{
    await db.SetCombineAndStoreAsync(SetOperation.Union, allUsersSet, new RedisKey[] { activeUsersSet, inactiveUsersSet, offlineUsersSet });
};

var CheckMemberExists = async () =>
{
    bool user6Offline = await db.SetContainsAsync(offlineUsersSet, "User:6");
    Console.WriteLine($"User:6 offline: {user6Offline}");
};

var Enumerate = async () =>
{
    Console.WriteLine($"All Users In one shot: {string.Join(", ", await db.SetMembersAsync(allUsersSet))}");
    Console.WriteLine($"All Users using scan: {string.Join(", ", db.SetScan(allUsersSet))}");
};

var MoveElementsBetweenSets = async () =>
{
    Console.WriteLine("Moving User:1 from active to offline");
    var moved = db.SetMove(activeUsersSet, offlineUsersSet, "User:1");
    Console.WriteLine($"All Active Users: {string.Join(", ", await db.SetMembersAsync(activeUsersSet))}");
    Console.WriteLine($"All Offline Users : {string.Join(", ", await db.SetMembersAsync(offlineUsersSet))}");
    Console.WriteLine($"Move Successful: {moved}");

};

await PopulateSubSets();
await CombiningSets();
await CheckMemberExists();
await Enumerate();
await MoveElementsBetweenSets();
muxer.Close();