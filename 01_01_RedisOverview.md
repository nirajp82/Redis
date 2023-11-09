### What is Redis
Redis (Remote Dictionary Server) is an open-source in-memory data structure store, used as a database, cache and message broker. It is known for its speed and performance, making it a popular choice for high-traffic web applications.

Redis supports a variety of data structures, including strings, hashes, lists, sets, and sorted sets. It also supports a number of features that make it ideal for caching, such as the ability to set expiration times for keys and evict keys when memory is low.

Redis is also highly scalable and can be clustered to handle large datasets and high workloads.

### Redis vs Redis Cluster

Redis: Redis is a single-node database. It operates on a single server and can handle all the data in memory. While it provides great performance for read and write operations, it lacks built-in high availability and automatic sharding for large datasets.

Redis Cluster: Redis Cluster is a distributed implementation of Redis. It provides automatic sharding and partitioning of data across multiple nodes. Redis Cluster offers high availability through automatic failover and supports data replication and partitioning for large datasets, making it suitable for scalable and fault-tolerant applications.

Redis Cluster uses a number of features to achieve this, including:

* Sharding: Redis Cluster shards the data across multiple nodes. This allows Redis Cluster to scale to large datasets.
* Replication: Redis Cluster replicates the data to multiple nodes. This makes Redis Cluster more available and durable.
* Consistent hashing: Redis Cluster uses a consistent hashing algorithm to ensure that keys are always assigned to the same node. This ensures that operations on the same key always go to the same node, even if the cluster topology changes.

Redis Cluster is a good choice for applications that need to handle large datasets and high workloads, or that need high availability and durability.

