# Redis
 - https://www.youtube.com/watch?v=NXbOVLpage0
 - https://www.youtube.com/watch?v=h30k7YixrMo
 - https://www.youtube.com/watch?v=zlxdX9f4l50
 - https://go.dev/talks/2012/waza.slide#58
 - https://betterprogramming.pub/internals-workings-of-redis-718f5871be84
 - In server-side software, concurrency and parallelism are often considered as different concepts. In a server, supporting concurrent I/Os means the server is able to serve several clients by executing several flows corresponding to those clients with only one computation unit. In this context, parallelism would mean the server is able to perform several things at the same time (with multiple computation units), which is different.

For instance a bartender is able to look after several customers while he can only prepare one beverage at a time. So he can provide concurrency without parallelism.

This question has been debated here: What is the difference between concurrency and parallelism?

See also this presentation from Rob Pike.

A single-threaded program can definitely provide concurrency at the I/O level by using an I/O (de)multiplexing mechanism and an event loop (which is what Redis does).

Parallelism has a cost: with the multiple sockets/multiple cores you can find on modern hardware, synchronization between threads is extremely expensive. On the other hand, the bottleneck of an efficient storage engine like Redis is very often the network, well before the CPU. Isolated event loops (which require no synchronization) are therefore seen as a good design to build efficient, scalable, servers.

The fact that Redis operations are atomic is simply a consequence of the single-threaded event loop. The interesting point is atomicity is provided at no extra cost (it does not require synchronization). It can be exploited by the user to implement optimistic locking and other patterns without paying for the synchronization overhead.
-
* Scan vs Keys
* Unlink vs del
* Keyspace
* Command
  * Get
  * Set
    * NX - parameter
    * XX - Parameter
    * ex - paramter
  * Scan
  * Keys

    
  * Set Expiration
   * EXPIRE
   * EXPIREAT
   * PEXPIRE
   * PEXPIREAT
 
     
 * Inspect Expiration
  * PTTL
  * TTL

 * HashSet
  * HGET
  * HSET
  * HDEL
  * HGETALL: O(N)
  * HINCRBY
  * HEXISTS
  * RedisJSON
  * HMSET

 * TYPE command
 * OBJECT command
 
 * Remove Expiration
  * PERSIST
