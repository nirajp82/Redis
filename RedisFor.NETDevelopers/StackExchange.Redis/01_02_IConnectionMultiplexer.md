* The IConnectionMultiplexer is responsible for maintaining all of the connections to Redis.

* It routes all the commands to Redis through a single connection for interactive commands, and a separate connection for subscription.

* The IConnectionMultiplexer is responsible for exposing a simple interface to get other critical interfaces of the library. Including the IDatabase, ISubscriber, and IServer.
