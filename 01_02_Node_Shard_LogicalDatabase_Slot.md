## Shard
A **shard** is a logical partition of a Redis dataset. Each shard is responsible for storing a subset of the overall dataset. Sharding is used to improve the performance and scalability of Redis clusters.

A shard is defined as a collection of nodes that serve the same set of slots and that replicate from each other. A shard may only have a single master at a given time, but may have multiple or no replicas. It is possible for a shard to not be serving any slots while still having replicas.

* Within a shard, one node functions as the read/write primary node. All the other nodes in a shard function as read-only replicas of the primary node.
* Nodes should be deployed in a shard on multiple availability zones or data centers for fault tolerance
* In case of a master node failure, one of the replicas will become the master

A Redis instance can be either a stand-alone instance or a member of a Redis cluster. A stand-alone Redis instance does not have any shards, while a Redis cluster can have one or more shards.

Here are some examples of when sharding may be necessary:

* The application has a large dataset that cannot be stored on a single node.
* The application has a high read and write load that cannot be handled by a single node.
* The application requires high availability and needs to be able to continue operating even if one or more nodes fail.

## Nodes: 

A node in Redis is a physical Redis server. Each node contains a subset of the Redis data, and it can be either a primary node or a replica node. A primary node is responsible for handling read and write requests, while a replica node is a read-only copy of a primary node.

The relationship between shards and nodes in Redis is as follows:

A shard can contain one or more nodes.
A node can only belong to one shard.
A shard can have only one primary node, but it can have multiple replica nodes.

When a client sends a request to Redis, the Redis cluster will first determine which shard the request belongs to. Once the shard has been determined, the request will be routed to the primary node of the shard. If the primary node is unavailable, the request will be routed to one of the replica nodes.


Each node is responsible for a subset of the 16384 slots. Nodes communicate with each other to share information about slots and keys, enabling distributed data management.

Each Redis node has 16 logical databases (indexed from 0 to 15).

## Slots: 

Redis uses hashing to map keys to slots. There are 16384 slots in total in a Redis Cluster (ranging from 0 to 16383). Each key belongs to a specific slot, determined by a hash function applied to the key. Slots are distributed across nodes, ensuring even data distribution in the cluster.

* Formula for Slots per Node:

Slots per Node = Total Slots / Total Nodes

Slots per Node =  16384 (Total Slots)/ Total Nodes

## Logical Databases:
Redis supports multiple logical databases within a single Redis server. Each database is identified by an index number (from 0 to 15 by default) and operates independently of others. You can switch between databases using the SELECT command. Logical databases are useful for separating different types of data or isolating data for different applications within the same Redis instance. Logical databases exist within individual nodes. Each Redis node can have multiple logical databases, and these databases are independent of each other. When you connect to a Redis node, you can select a specific logical database on that node using the SELECT command.

When you connect to a specific node, you can work with the logical databases hosted by that node. Each node can have its own set of logical databases, providing data separation and isolation for different use cases or applications.

![image](https://github.com/nirajp82/Redis/assets/61636643/03482af5-b739-42af-8f60-8b5c00ad37b9)
Reference: https://www.aeraki.net/docs/v1.x/tutorials/redis/cluster/

## Relationship between Logical Database, Nodes, and Slots:

* Each node is responsible for a subset of the keyspace, which is divided into 16384 slots.
* Each logical database is mapped to a subset of the slots.
* When a client performs a Redis operation on a key in a logical database, the operation is sent to the node that is responsible for the slot that the key belongs to.

Consider a Redis Cluster with 3 nodes (Node A, Node B, Node C). Each node is responsible for approximately 16384/3 = 5461 slots.

Nodes and Slots: Slots are distributed across nodes. For instance:

Node A: Slots 0-5460
Node B: Slots 5461-10921
Node C: Slots 10922-16383

Let's say we have two logical databases, db0 and db1, in Node A. A key "user:1234" hashes to slot 5678, which belongs to Node A. Depending on the selected logical database, the key "user:1234" can be stored in different databases:

- If `SELECT 0` is executed: 
  ```
  127.0.0.1:6379[0]> SET user:1234 "John"
  ```

  This sets the key "user:1234" with value "John" in logical database 0 of Node A.

- If `SELECT 1` is executed:
  ```
  127.0.0.1:6379[1]> SET user:1234 "Alice"
  ```

  This sets the key "user:1234" with value "Alice" in logical database 1 of Node A.

# How keys are routed to the appropriate database within a Redis Cluster.

### Example: Routing Keys to Logical Databases in a Redis Cluster

Consider a Redis Cluster with three nodes: Node A, Node B, and Node C. Each node has 16 logical databases (indexed from 0 to 15). The cluster is responsible for 16384 hash slots.

#### Step 1: Connecting to a Node
When a client connects to the Redis Cluster, it can connect to any of the nodes. Let's assume the client connects to Node A (`127.0.0.1:7000`).

#### Step 2: Sending a Command with a Key
The client wants to set a key "user:1234". To determine which logical database the key belongs to, Redis Cluster uses a hash function on the key to map it to a specific slot number (between 0 and 16383). 

Redis Cluster uses CRC16 (Cyclic Redundancy Check 16-bit) hashing algorithm to map keys to specific hash slots. When you add or access a key in Redis Cluster, the CRC16 hash function is applied to the key to determine the hash slot to which the key belongs. The resulting slot number ranges from 0 to 16383.

In this example, let's say the hash slot for "user:1234" is calculated to be 5678.

#### Step 3: Mapping Slot to Node and Database

Each node in the Redis Cluster is responsible for a range of hash slots. The cluster ensures an even distribution of slots across nodes. For instance:

*. **Slot to Node Mapping:**
   - Node A: Slots 0-5460
   - Node B: Slots 5461-10921
   - Node C: Slots 10922-16383

   In our example, slot 5678 falls within the range of Node A.

*. **Logical Database Selection  (Optional):**
   Within Node A, the client can specify the logical database to use. For example, if the client selects logical database 1 using the `SELECT` command:
   ```
   127.0.0.1:7000> SELECT 1
   OK
   ```
If you connect to a Redis server without explicitly selecting a logical database (using the SELECT command), all keys and entries will be stored in the default logical database, which is database 0. In Redis, database indexing starts from 0, so database 0 is the default logical database where keys and data are stored if no specific database is selected.

However, it is generally considered good practice to select a specific database for your application. This will help to keep your data organized and prevent conflicts with other applications that may be using the same Redis instance.

#### Step 4: Executing the Command
Now that the client is connected to Node A and has selected logical database 1, it can execute the command to set the key:
   ```
   127.0.0.1:7000[1]> SET user:1234 "John"
   OK
   ```

In this scenario, the key "user:1234" is stored in logical database 1 of Node A. The combination of slot calculation, slot to node mapping, and logical database selection ensures that the key is routed to the correct node and the specified logical database within the Redis Cluster.

# Why node should have multiple logical databases

Each node in Redis can have multiple logical databases, and the ability to support multiple databases within a single Redis instance offers several advantages:

### 1. **Data Separation:**
Logical databases allow you to separate different sets of keys and data within the same Redis node. This separation can be useful for different applications or components of your system that require distinct data storage. For example, you could use one logical database for user-related data and another for product-related data. This separation makes it easier to manage and organize your data.

### 2. **Isolation:**
Each logical database operates independently of others. This means that keys in one database are not visible or accessible from other databases. This isolation is crucial when you have multiple applications or services sharing the same Redis instance. It ensures that data from one application does not interfere with or accidentally affect data from another application.

### 3. **Resource Management:**
Logical databases allow you to allocate specific memory limits or quotas to different databases. This allocation ensures that one database cannot consume all the available memory, potentially impacting the performance of other databases or the overall Redis instance. By having separate logical databases, you can manage memory usage more effectively.

### 4. **Flexibility and Versatility:**
Having multiple databases in a single Redis node provides flexibility. Different databases can have different data structures, configurations, and persistence options. For instance, one database might use RDB snapshots for persistence, while another might use AOF logs. This flexibility allows you to tailor the storage and retrieval mechanisms for different types of data within the same Redis instance.

### 5. **Reduced Complexity in Deployment:**
In certain use cases, having multiple logical databases within a single Redis node can simplify deployment. Instead of managing multiple Redis instances on different ports or servers, you can consolidate related data into different databases within a single instance. This reduces the complexity of deployment and maintenance, making it easier to manage your Redis infrastructure.

In summary, multiple logical databases within a single Redis node provide data separation, isolation, resource management, flexibility, and simplified deployment. These benefits contribute to more organized, efficient, and manageable Redis data storage strategies in various applications and use cases.


