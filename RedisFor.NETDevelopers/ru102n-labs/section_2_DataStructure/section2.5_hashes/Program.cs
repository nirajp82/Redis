using StackExchange.Redis;
using System.Reflection.Metadata;

var muxer = await ConnectionMultiplexer.ConnectAsync(new ConfigurationOptions
{
    EndPoints = { "localhost:6379" }
});

var db = muxer.GetDatabase();
var person1 = "person:1";
var person2 = "person:2";
var person3 = "person:3";

db.KeyDelete(new RedisKey[] { person1, person2, person3 });

db.HashSet(person1, new HashEntry[]
{
    new("name","Alice"),
    new("age", 33),
    new("email","alice@example.com")
});

db.HashSet(person2, new HashEntry[]
{
    new("name","Bob"),
    new("age", 27),
    new("email","robert@example.com")
});

db.HashSet(person3, new HashEntry[]
{
    new("name","Charlie"),
    new("age", 50),
    new("email","chuck@example.com")
});

var IncrementAsync = async () =>
{
    var newAge = await db.HashIncrementAsync(person3, "age");
    Console.WriteLine($"person:3 new age: {newAge}"); //person: 3 new age: 51
};

var FetchAsync = async () =>
{
    var name = await db.HashGetAsync(person1, "name");
    Console.WriteLine($"person:1 name: {name}"); //person: 1 name: Alice
};

var HashGetAllAsync = async () =>
{
    //When hash is relatively small, means less than 1000 fields. You can use HashGetAll.
    var fields = await db.HashGetAllAsync(person2);
    Console.WriteLine($"person:2 fields: {string.Join(", ", fields)}"); //person:2 fields: name: Bob, age: 27, email: robert @example.com
};

var HashScan = () =>
{
    //If you are working with a very large hash with many thousands of fields, you may want to use HashScan instead.
    //HashScan allows you to paginate over your hash.
    //This will decrease the amount of time that Redis is busy servicing any one request, but will require multiple round trips to Redis.
    var person3Fields = db.HashScan(person3);
    Console.WriteLine($"person:3 fields: {string.Join(", ", person3Fields)}");//person:3 fields: name: Charlie, age: 51, email: chuck @example.com
};
await IncrementAsync();
await FetchAsync();
await HashGetAllAsync();
HashScan();