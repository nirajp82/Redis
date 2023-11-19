using StackExchange.Redis;
using System.Collections.Generic;
using System.Net;

var muxer = await ConnectionMultiplexer.ConnectAsync(new ConfigurationOptions
{
    EndPoints = { "localhost:6379" }
});
var db = muxer.GetDatabase();
var fruitKey = "fruits";
var vegetableKey = "vegetables";
//Just to start each program run with a clean slate.
await db.KeyDeleteAsync(new RedisKey[] { fruitKey, vegetableKey });

var PushLeft = async () =>
{
    await db.ListLeftPushAsync(fruitKey, new RedisValue[] { "Banana", "Mango", "Apple", "Pepper", "Kiwi", "Grape" });
    // Banana
    // Mango -> Banana
    // Grape -> Kiwi -> ... Mango -> Banana
    Console.WriteLine($"The first fruit in the list is: {await db.ListGetByIndexAsync(fruitKey, 0)}"); // Grape
    Console.WriteLine($"The last fruit in the list is:  {db.ListGetByIndex(fruitKey, -1)}"); //Banana

};

var PushRight = async () =>
{
    await db.ListRightPushAsync(vegetableKey, new RedisValue[] { "Potato", "Carrot", "Asparagus", "Beet", "Garlic", "Tomato" });
    Console.WriteLine($"The first vegetable in the list is: {await db.ListGetByIndexAsync(vegetableKey, 0)}"); //Potato
    Console.WriteLine($"The last vegetable in the list is:  {await db.ListGetByIndexAsync(vegetableKey, -1)}"); //Tomato
};

var Enumerate = async () =>
{
    Console.WriteLine($"Fruit indexes 0 to -1: {string.Join(", ", await db.ListRangeAsync(fruitKey))}"); // Grape, Kiwi, Pepper, Apple, Mango, Banana
    Console.WriteLine($"Vegetables index 0 to -2:  {string.Join(", ", await db.ListRangeAsync(vegetableKey, 0, -2))}"); //Potato, Carrot, Asparagus, Beet, Garlic
};

var MoveElementsBetweenLists = async () =>
{
    Console.WriteLine("");

    //Grape, Kiwi, Pepper, Apple, Mango, Banana
    Console.WriteLine($"Before Move - Fruits: {string.Join(", ", await db.ListRangeAsync(fruitKey))}");
    //Potato, Carrot, Asparagus, Beet, Garlic, Tomato
    Console.WriteLine($"Before Move - Vegetables:  {string.Join(", ", await db.ListRangeAsync(vegetableKey))}");

    //Move one veg. from right side (Tail) "Tomato" to fruits 
    await db.ListMoveAsync(vegetableKey, fruitKey, ListSide.Right, ListSide.Left);

    Console.WriteLine("");
    //Tomato, Grape, Kiwi, Pepper, Apple, Mango, Banana
    Console.WriteLine($"After Move - Fruits: {string.Join(", ", await db.ListRangeAsync(fruitKey))}");
    //Potato, Carrot, Asparagus, Beet, Garli
    Console.WriteLine($"After Move - Vegetables:  {string.Join(", ", await db.ListRangeAsync(vegetableKey))}");
};

var ListAsAQueue = async () =>
{
    Console.WriteLine("Enqueuing Celery");
    await db.ListRightPushAsync(vegetableKey, "Celery");
    //Potato, Carrot, Asparagus, Beet, Garlic, Celery
    Console.WriteLine($"After Enqueue - Vegetables:  {string.Join(", ", await db.ListRangeAsync(vegetableKey))}");
    //Potato
    Console.WriteLine($"Dequeued: {string.Join(", ", await db.ListLeftPopAsync(vegetableKey))}");
};

var ListAsAStack = async () =>
{
    Console.WriteLine("Pushing Grapefruit");
    db.ListLeftPush(fruitKey, "Grapefruit");
    //Grapefruit, Tomato, Grape, Kiwi, Pepper, Apple, Mango, Banana
    Console.WriteLine($"After Push - Fruits:  {string.Join(", ", await db.ListRangeAsync(fruitKey))}");
    //Grapefruit, Tomato,
    Console.WriteLine($"Popping Fruit: {string.Join(",", db.ListLeftPop(fruitKey, 2))}");
};

var Search = async () =>
{
    //Grape, Kiwi, Pepper, Apple, Mango, Banana
    Console.WriteLine($"Fruits:  {string.Join(", ", await db.ListRangeAsync(fruitKey))}");
    //3
    Console.WriteLine($"Position of Apple: {await db.ListPositionAsync(fruitKey, "Apple")}");
    //6
    Console.WriteLine($"List Length: {await db.ListLengthAsync(fruitKey)}");
};

await PushLeft();
await PushRight();
await Enumerate();
await MoveElementsBetweenLists();
await ListAsAQueue();
await ListAsAStack();
await Search();