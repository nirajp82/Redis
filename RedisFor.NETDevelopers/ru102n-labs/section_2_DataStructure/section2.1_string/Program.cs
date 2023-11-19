using StackExchange.Redis;
using System.Xml.Linq;


var muxer = await ConnectionMultiplexer.ConnectAsync(new ConfigurationOptions
{
    EndPoints = { "localhost:6379" }
});
var db = muxer.GetDatabase();

var key = new RedisKey("inst:1:name");
Func<Task> BasicExample = async () =>
{
    db.StringSet(key, "Raj");
    var name = await db.StringGetAsync(key);
    Console.WriteLine($"Instructor 1's name is: {name}");
};

var AppendString = async () =>
{
    await db.StringAppendAsync(key, " Shyamaji");
    var name = await db.StringGetAsync(key);
    Console.WriteLine($"Instructor 1's name is: {name}");
};

var Increment = async () =>
{
    var key = new RedisKey("temperature:ca");
    await db.StringSetAsync(key, 42);
    var result = await db.StringIncrementAsync(key); //Default to using an integer increment of 1
    Console.WriteLine($"Temperature: {result}"); //Temperature: 43

    var resultAsDouble = await db.StringIncrementAsync(key, 0.5);//New Temperature: 43.5
    Console.WriteLine($"New Temperature: {resultAsDouble}");

    //Note once we increment by a double, we will not be able to increment by an integer anymore, as you've changed the encoding of the string.
    // result = await db.StringIncrementAsync(key); //Run time error. //ERR value is not an integer or out of range
};

var Expiration = async () =>
{
    var key = new RedisKey("expiration:demo");
    await db.StringSetAsync(key, "Will expire after 1 second", expiry: TimeSpan.FromSeconds(1));
    var result = await db.StringGetAsync(key);
    Console.WriteLine($"{result}"); //Will expire after 1 second
    await Task.Delay(TimeSpan.FromSeconds(1));
    result = await db.StringGetAsync(key);
    Console.WriteLine(result.HasValue ? result.ToString() : "Null"); // Null
};

var Conditional = () =>
{
    var conditionalKey = "ConditionalKey";
    var conditionalKeyText = "this has been set";
    // You can also specify a condition for when you want to set a key
    // For example, if you only want to set a key when it does not exist
    // you can by specifying the NotExists condition
    bool wasSet = db.StringSet(conditionalKey, conditionalKeyText, when: When.NotExists);
    Console.WriteLine($"Key set: {wasSet}"); // Key set: True

    // Of course, after the key has been set, if you try to set the key again
    // it will not work, and you will get false back from StringSet
    wasSet = db.StringSet(conditionalKey, "this text doesn't matter since it won't be set", when: When.NotExists);
    Console.WriteLine($"Key set: {wasSet}"); //Key set: False

    // You can also use When.Exists, to set the key only if the key already exists
    wasSet = db.StringSet(conditionalKey, "we reset the key!");
    Console.WriteLine($"Key set: {wasSet}"); //Key set: True
};

await BasicExample();
await AppendString();
await Increment();
await Expiration();
Conditional();