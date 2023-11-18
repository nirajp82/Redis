The most fundamental architectural feature of StackExchange.Redis is Connection Multiplexing.

This class is responsible for arbitrating all connections to Redis, and routing all commands you want to send through the library through a single connection.



Refereces: 
* https://stackexchange.github.io/StackExchange.Redis/Basics.html
* https://university.redis.com/courses/course-v1:redislabs+RU102N+2023_11/courseware/1a3812362378461cb366aee0ed6fcc0f/d1a842bf4314460dac045607f5169457/?child=first
