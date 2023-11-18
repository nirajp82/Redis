## ISubscriber Interface

The `ISubscriber` interface in StackExchange.Redis is responsible for managing subscriptions to Redis in the pub/sub system. Unlike the other interfaces, the ISubscriber does not use the interactive Redis connection.

This is because Redis is a publish-subscribe system, which means that messages are sent to channels and subscribers listen for messages on those channels. When you subscribe to a channel, your Redis connection enters subscription mode, which limits the connection to only use commands that are related to the pub/sub system.

Because of this, the ISubscriber is designed to use a separate connection for subscriptions. The ConnectionMultiplexer will automatically open a separate connection for subscriptions when you call the IConnectionMultiplexer.GetSubscriber() method. This ensures that your interactive Redis connection is not affected by your subscriptions.

### Purpose and Design:

- **Subscription Handling:**
  - The primary purpose of `ISubscriber` is to handle subscriptions to Redis channels. In a pub/sub system, clients can subscribe to channels to receive messages published to those channels.

- **Separate Connection for Subscriptions:**
  - The `Multiplexer` (connection manager) explicitly opens a separate connection for handling subscriptions. This separation is necessary because when a client subscribes to any channel, the connection transforms into subscription mode. In this mode, only commands related to subscriber functionality are allowed.

- **Maintaining a Single Connection:**
  - Despite opening a separate connection for subscriptions, the `Multiplexer` continues to maintain a single connection per Redis server. This ensures efficiency and simplicity in managing both regular commands and subscriptions.

- **Subscription Mode Limitations:**
  - In subscription mode, the connection is restricted to commands specific to handling subscriptions, such as subscribing to channels, unsubscribing, and receiving published messages.

### Interaction with Connection Multiplexer:

- **Single Connection for All Subscriptions:**
  - All subscriptions are handled on the same connection maintained by the `Multiplexer`. This consolidated approach simplifies the management of multiple subscriptions and avoids unnecessary overhead.

- **Getting an Instance:**
  - To obtain an instance of `ISubscriber`, you can call the `IConnectionMultiplexer.GetSubscriber()` method. This method provides access to the `ISubscriber` interface, allowing you to manage subscriptions programmatically.

### Example Usage:

```csharp
// Getting an instance of ISubscriber from the connection multiplexer
ISubscriber subscriber = connectionMultiplexer.GetSubscriber();

// Subscribe to a channel
subscriber.Subscribe("channel_name", (channel, message) =>
{
    // Handle the received message
    Console.WriteLine($"Received message on channel {channel}: {message}");
});
```

In summary, `ISubscriber` is the interface designed specifically for managing Redis pub/sub subscriptions. It operates on a separate connection for subscriptions while maintaining overall simplicity through a single connection per Redis server. This interface is crucial for implementing and handling real-time communication and event-driven architectures in Redis.
