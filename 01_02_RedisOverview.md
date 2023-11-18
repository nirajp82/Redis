### What is Redis
Redis (Remote Dictionary Server) is an open-source in-memory data structure store, used as a database, cache and message broker. It is known for its speed and performance, making it a popular choice for high-traffic web applications.

Redis supports a variety of data structures, including strings, hashes, lists, sets, and sorted sets. It also supports a number of features that make it ideal for caching, such as the ability to set expiration times for keys and evict keys when memory is low.

Redis is also highly scalable and can be clustered to handle large datasets and high workloads.

Redis is, mostly, a single-threaded server from the POV (Point of view) of commands execution (actually modern versions of Redis use threads for different things).  Redis is a single-threaded data store, meaning it can only process one command at a time. This means that if you send multiple commands to Redis concurrently, they will all be queued up and processed one after the other.


### Redis vs Redis Cluster

Redis: Redis is a single-node database. It operates on a single server and can handle all the data in memory. While it provides great performance for read and write operations, it lacks built-in high availability and automatic sharding for large datasets.

Redis Cluster: In the case of a redis cluster, a single logical database may be spread over multiple nodes. Redis Cluster is a distributed implementation of Redis. It provides automatic sharding and partitioning of data across multiple nodes. Redis Cluster offers high availability through automatic failover and supports data replication and partitioning for large datasets, making it suitable for scalable and fault-tolerant applications.

Redis Cluster uses a number of features to achieve this, including:

* Sharding: Redis Cluster shards the data across multiple nodes. This allows Redis Cluster to scale to large datasets.
* Replication: Redis Cluster replicates the data to multiple nodes. This makes Redis Cluster more available and durable.
* Consistent hashing: Redis Cluster uses a consistent hashing algorithm to ensure that keys are always assigned to the same node. This ensures that operations on the same key always go to the same node, even if the cluster topology changes.

Redis Cluster is a good choice for applications that need to handle large datasets and high workloads, or that need high availability and durability.


### Sharding vs Replication vs Clustering
#### Sharding
- **Definition:** Having a portion of the data, e.g., one server holding the odd user IDs and the other holding the even ones.
- **Performance Improvement:** Improves performance by distributing data across shards, reducing the amount of data queried in each shard.
- **Implementation:** Requires logic in the program to decide which server to query based on the sharding key.

#### Replication
- **Definition:** Having a full copy of the data of the master. In the simplest form, writes go to the master, while reads can go to either the master or slaves.
- **Scaling:** Allows horizontal scaling for reading queries. All servers can handle reads, while writes are typically directed to the master.
- **Use Cases:** Often used for backups, standby instances, and supporting scenarios like analytics and reporting.

#### Clustering
- **Definition:** Having a group of servers in master-master replication, where writes can go to any of them.
- **Single Point of Failure:** Eliminates the single point of failure by allowing writes on any server in the cluster.
- **Coordination:** Write transactions are coordinated across the cluster, potentially leading to slower writes.

#### Performance Considerations
- **Sharding:** Improves both queries and writes by distributing data, but queries and data design must consider that only a portion of the data is on each server.
- **Replication:** Primarily improves reads, works as backups, and supports use cases like analytics.
- **Clustering:** Takes out the single point of failure but can lead to slower writes due to coordination requirements.

#### Combinations
- **Mixes:** You can have mixes between them, such as shards with replicas or clusters, and replicas of clusters.
