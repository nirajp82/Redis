using StackExchange.Redis;
using System;
using System.Transactions;

var muxer = await ConnectionMultiplexer.ConnectAsync(new ConfigurationOptions
{
    EndPoints = { "localhost:6379" }
});

var db = muxer.GetDatabase();
await db.KeyDeleteAsync(new RedisKey[] { "person:1", "person:name:JohnDoe", "person:age:32", "person:postal_code:12345" });

var basicTranAsync = async () =>
{
    //Hash person with three fields, name, age, and postal code and also will index all three fields.
    var transaction = db.CreateTransaction();
    var tsk1 = transaction.HashSetAsync("person:1", new HashEntry[]
      {
        new HashEntry("name","JohnDoe"),
        new HashEntry("age",32),
        new HashEntry("postal_code",12345)
      });

    //Index fields
    var tsk2 = transaction.SortedSetAddAsync("person:name:JohnDoe", "person:1", 0);
    var tsk3 = transaction.SortedSetAddAsync("person:age:32", "person:1", 0);
    var tsk4 = transaction.SortedSetAddAsync("person:postal_code:12345", "person:1", 0);

    // Execute the entire transaction when you're ready to commit (blocking)
    var success = await transaction.ExecuteAsync();
    if (success)
    {
        Console.WriteLine($"Transaction Successful: {success}");
        //Commands executed inside a transaction do not return results until after you execute the transaction.
        // If we want the result of command in transaction, we should store(not await) the task, and await it after the execute:
        await Task.WhenAll(tsk1, tsk2, tsk3, tsk4);
    }
};

var conditionalSuccessAsync = async () =>
{
    //Hash person with three fields, name, age, and postal code and also will index all three fields.
    var transaction = db.CreateTransaction();

    transaction.AddCondition(Condition.HashEqual("person:1", "age", 32));
    var tsk1 = transaction.HashIncrementAsync("person:1", "age");
    var tsk2 = transaction.SortedSetIncrementAsync("person:age", "person:1", 1);

    bool success = await transaction.ExecuteAsync();
    Console.WriteLine($"Transaction Successful?: {success}");
};

var conditionalFailureAsync = async () =>
{
    var transaction = db.CreateTransaction();
    transaction.AddCondition(Condition.HashEqual("person:1", "age", 31));
    var tsk1 = transaction.HashIncrementAsync("person:1", "age");
    var tsk2 = transaction.SortedSetIncrementAsync("person:age", "person:1", 1);

    bool success = await transaction.ExecuteAsync();
    Console.WriteLine($"Transaction Successful?: {success}");
};

await basicTranAsync();
await conditionalSuccessAsync();
await conditionalFailureAsync();