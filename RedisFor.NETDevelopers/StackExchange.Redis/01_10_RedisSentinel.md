Redis Sentinel provides high availability for Redis when <ins> not using Redis Cluster</ins>.

In a typical Redis Sentinel setup without Redis Cluster, you have a master Redis node and one or more replica nodes. The data resides on the master node, and the replica nodes maintain a copy of this data through replication. When a failover occurs, one of the replica nodes is promoted to become the new master, and the other replicas are reconfigured to sync with the new master.

Redis Sentinel also provides other collateral tasks such as monitoring, notifications, Automatic failover and acts as a configuration provider for clients.

Redis Sentinel is used to ensure that Redis applications are always up and running, even in the event of a hardware failure or software crash. It is particularly useful for applications that require high levels of availability, such as e-commerce platforms, financial systems, and social media applications. Redis Sentinel addresses this limitation by providing a solution for automatic failover and monitoring.

![image](https://github.com/nirajp82/Redis/assets/61636643/53b1a6ea-04b4-4f66-a764-2972c5d2f45b)

**How does Redis Sentinel work?**

Redis Sentinel works by continuously monitoring the state of Redis master and replica nodes, and it takes specific actions when it detects failures or other predefined events. Here's an overview of how Redis Sentinel works:

1. **Deployment:**
   - Redis Sentinel is deployed as a separate process alongside the Redis instances in a Redis deployment.
   - Typically, multiple Sentinel instances are deployed to provide fault tolerance and avoid a single point of failure.

2. **Monitoring:**
   - Each Sentinel instance continuously monitors the health of the Redis nodes in the deployment.
   - Monitoring involves sending regular "ping" messages (heartbeats) to the Redis instances and checking their responses.
   - Sentinels also perform more advanced checks to assess the overall health of a Redis node.

3. **Quorum-Based Decision Making:**
   - Sentinel instances use a quorum-based approach for decision making. A quorum is a majority of Sentinel nodes.
   - Before taking significant actions, such as initiating a failover, Sentinels need to reach a quorum agreement to ensure a reliable decision.

4. **Event Detection:**
   - Sentinel instances are programmed to detect various events, including:
     - Redis master node failure
     - Redis replica node failure
     - Redis node comeback (after a failure)
     - Changes in the configuration of the Redis deployment

5. **Failover Process:**
   - When a Sentinel instance detects that the Redis master node is unreachable or unhealthy, it initiates a failover process.
   - The Sentinel instance selects a suitable replica (based on its configuration and health) to promote it as the new master.
   - The other replicas are reconfigured to sync with the new master.

6. **Configuration Management:**
   - Sentinel helps manage the configuration of the Redis deployment.
   - It can propagate changes in the Redis topology, such as promoting a replica to a master, to all other nodes in the system.

7. **Notification and Alerts:**
   - Sentinel provides a monitoring dashboard for administrators to check the status of the Redis deployment.
   - It can send notifications and alerts (via email, API calls, etc.) to notify administrators of important events like failovers.

8. **Dynamic Reconfiguration:**
   - Sentinel instances can dynamically reconfigure themselves based on the current state of the deployment. For example, if a Sentinel node fails or new nodes are added, the remaining Sentinels can adjust their quorum and reconfigure the system accordingly.

By continuously monitoring the Redis nodes, coordinating failovers, and managing configuration changes, Redis Sentinel enhances the availability and reliability of Redis in production environments. It ensures that the system can adapt to failures and maintain uninterrupted service.

Connection to Redis Sentil
```cs
var options = new ConfigurationOptions
    {
        EndPoints = {"sentinel-1:26379"},
        ServiceName = "sentinel"
    };

    var muxer = ConnectionMultiplexer.Connect(options);
```

References: https://developer.redis.com/operate/redis-at-scale/high-availability/understanding-sentinels/
