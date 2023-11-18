The most fundamental architectural feature of StackExchange.Redis is Connection Multiplexing.

**Fundamental Feature: Connection Multiplexing**
- **Key Component:** ConnectionMultiplexer class is central to StackExchange.Redis, ConnectionMultiplexer class is responsible for arbitrating all connections to Redis, and routing all commands we want to send through the library through a **single connection**.
- **Number of Connections:** he ConnectionMultiplexer opens exactly 2 connections per Redis Server, one of which is the interactive command connection to Redis, the other being the subscription connection for the pub/sub API.

ConnectionMultiplexer hides away the details of multiple servers. Because the ConnectionMultiplexer does a lot, it is designed to be shared and reused between callers. You should not create a ConnectionMultiplexer per operation. It is fully thread-safe and ready for this usage.



Refereces: 
* https://stackexchange.github.io/StackExchange.Redis/Basics.html
* https://university.redis.com/courses/course-v1:redislabs+RU102N+2023_11/courseware/1a3812362378461cb366aee0ed6fcc0f/d1a842bf4314460dac045607f5169457/?child=first
