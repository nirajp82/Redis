The most fundamental architectural feature of StackExchange.Redis is Connection Multiplexing.

This class is responsible for arbitrating all connections to Redis, and routing all commands you want to send through the library through a single connection.
