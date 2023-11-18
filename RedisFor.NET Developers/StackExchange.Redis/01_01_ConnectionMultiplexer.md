The most fundamental architectural feature of StackExchange.Redis is Connection Multiplexing.

**Fundamental Feature: Connection Multiplexing**
- **Key Component:** ConnectionMultiplexer class is central to StackExchange.Redis, ConnectionMultiplexer class is responsible for arbitrating all connections to Redis, and routing all commands we want to send through the library through a **single connection**.
  
- **Number of Connections:** he ConnectionMultiplexer opens exactly 2 connections per Redis Server, one of which is the interactive command connection to Redis, the other being the subscription connection for the pub/sub API.
  
- **Alignment with Redis Architecture:** StackExchange.Redis uses a single connection multiplexer to manage all connections to Redis. This means that even if you send multiple commands concurrently, they will all be sent through the same connection. Sending additional commands concurrently doesn't help much because Redis can only process one command at a time. This means that even if you send multiple commands concurrently, they will still be queued up and processed one after the other.  Number of connections managed by the connection multiplexer matches the number of command threads in Redis. Since Redis only has one command thread, there is no need to open multiple connections. So this design of StackExchange.Redis aligns with Redis's single-threaded architecture. It uses a single connection because Redis's command processing is inherently single-threaded. Since Redis only has one command thread, there is no need to open multiple connections.

ConnectionMultiplexer hides away the details of multiple servers. Because the ConnectionMultiplexer does a lot, it is designed to be shared and reused between callers. You should not create a ConnectionMultiplexer per operation. It is fully thread-safe and ready for this usage.

**Benefits:**
 ConnectionMultiplexer is highly performant and robust, efficiently pushing commands to Redis.
 
1. **Matching Redis Threads:**
   - The single connection multiplexer matches the cardinality of Redis Threads. There's only one command thread in Redis, so sending additional commands concurrently doesn't help much as they will be waiting to be serviced by the command thread.

2. **Socket Optimization:**
    It minimizes the number of sockets application needs to open and maintain and wards off Socket Exhaustion.  Sockets are communication endpoints used for data transfer between your application and Redis.

3. **Automatic Command Pipelining:**
    The multiplexer will maximize usage of your sockets, and automatically pipeline commands sent concurrently.
   - Command pipelining is an optimization technique that allows multiple commands to be sent to Redis without waiting for each response. This is particularly beneficial for applications that send a large number of commands to Redis. By automatically pipelining commands, the multiplexer significantly reduces latency and improves throughput.
   - Here's how command pipelining works:
    
    1. **Batching Commands:**
       - Instead of sending commands one at a time and waiting for each response before sending the next command, the client batches multiple commands together.
    
    2. **Sending Commands in Bulk:**
       - The client sends the entire batch of commands to the Redis server in a single network request.
    
    3. **Server Processing:**
       - The Redis server processes the batch of commands sequentially and generates responses for each command.
    
    4. **Bulk Response:**
       - The server sends back a bulk response containing the results of each command in the order they were received.
    
    5. **Client Parsing:**
       - The client then parses the bulk response, extracting the results for each com

**Tradeoffs:**
1. **Head-of-Line Blockages:**
   - Head-of-line blockages can occur with large payloads blocking out other requests. Head-of-line blockage occurs when a particular task or request, often a large or time-consuming one, delays the processing of subsequent tasks or requests that are waiting in line behind it.

2. **Blocking Commands:**
   - Blocking commands, like stream reads, cannot be used as they would block the interactive connection.
     - Blocking commands is an operations that wait for specific conditions, like waiting for a certain element to be added to a list or waiting for a specific score in a sorted set or waiting for new entries to be added to a Redis stream.
   - It prevents other threads or tasks from using the same connection concurrently. This limitation can impact the overall concurrency and responsiveness of the application.
  
3. **Transaction Differences:**
   - Transactions operate differently, with limited support for watches, and commands aren't dispatched to Redis from redis client until execution time (Until client executes the transaction.commit()).

```cs
using StackExchange.Redis;

public static void PerformTransaction()
{
    var connection = ConnectionMultiplexer.Connect("localhost");
    var db = connection.GetDatabase();

    using (var transaction = db.CreateTransaction())
    {
        transaction.StringSetAsync("key1", "value1");
        transaction.StringIncrementAsync("counter", 1);
        transaction.StringSetAsync("key2", "value2");

        transaction.Execute();
    }
}
```
These commands are not sent to Redis immediately. Instead, they are queued up and executed only when the transaction.Execute() method is called. This can lead to potential race conditions, as the values of key1 or counter could change between the time the commands are added to the transaction and the time the transaction is executed.

While Redis supports watches, the ConnectionMultiplexer has limitations when it comes to fully supporting them in the context of transactions.

Refereces: 
* https://stackexchange.github.io/StackExchange.Redis/Basics.html
* https://university.redis.com/courses/course-v1:redislabs+RU102N+2023_11/courseware/1a3812362378461cb366aee0ed6fcc0f/d1a842bf4314460dac045607f5169457/?child=first
