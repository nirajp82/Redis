Connecting to an OSS ("Open Source Software") Cluster instance is the same as connecting to a standalone Redis Instance.

Connecting to a Redis Cluster involves utilizing multiple endpoints to ensure continuous connectivity even in case of endpoint failures. This approach ensures redundancy and fault tolerance, allowing the connection to remain active despite potential disruptions.

```cs
var options = new ConfigurationOptions
{
    // add and update parameters as needed
    EndPoints = {"redis-1:6379", "redis-2:6379", "redis-3:6379"}
};

ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(options);
```
**EndPoints Collection:**
   - The `EndPoints` property of `ConfigurationOptions` is assigned a collection of multiple endpoints. Each endpoint represents a master node in the Redis Cluster.
   - In the example, three endpoints are provided: "redis-1:6379", "redis-2:6379", and "redis-3:6379".
   - These endpoints serve as potential connection points, and the `ConnectionMultiplexer` will attempt to connect to them in the specified order.
   - The `ConnectionMultiplexer` will attempt to connect to the endpoints provided in the `EndPoints` collection. If one endpoint fails, it will try the next one.


**Key Points:**

1. **Multiple Endpoints:** Unlike standalone Redis instances, connecting to a Redis Cluster requires specifying multiple endpoints. This redundancy ensures that if one endpoint becomes unavailable, the connection can be redirected to another endpoint, maintaining connectivity.

2. **ConnectionMultiplexer:** The ConnectionMultiplexer is a crucial component in managing connections to a Redis Cluster. It handles the communication with the cluster, automatically redirecting connections to available endpoints when necessary.

3. **ConfigurationOptions:** The ConfigurationOptions object allows you to configure the connection parameters, including the list of endpoints. This object provides flexibility in tailoring the connection behavior to your specific requirements.

4. **Example Configuration:** The provided example demonstrates the ConfigurationOptions object with multiple endpoints. The EndPoints property specifies three endpoints, "redis-1:6379", "redis-2:6379", and "redis-3:6379". If one of these endpoints fails, the ConnectionMultiplexer will attempt to connect to the remaining endpoints, ensuring uninterrupted connectivity.

This approach is important for resilience in a Redis Cluster setup. If one of the master nodes becomes unreachable or fails over, the `ConnectionMultiplexer` can automatically redirect its requests to the other available master nodes, ensuring continued availability and fault tolerance.
