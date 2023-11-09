#### Nodes: 

In a Redis Cluster, nodes are individual Redis instances forming the cluster. Each node is responsible for a subset of the 16384 slots. Nodes communicate with each other to share information about slots and keys, enabling distributed data management.

Each Redis node has 16 logical databases (indexed from 0 to 15).

#### Slots: 

Redis uses hashing to map keys to slots. There are 16384 slots in total in a Redis Cluster (ranging from 0 to 16383). Each key belongs to a specific slot, determined by a hash function applied to the key. Slots are distributed across nodes, ensuring even data distribution in the cluster.

* Formula for Slots per Node:

Slots per Node = Total Slots / Total Nodes

Slots per Node =  16384 (Total Slots)/ Total Nodes

#### Logical Databases:
Redis supports multiple logical databases within a single Redis server. Each database is identified by an index number (from 0 to 15 by default) and operates independently of others. You can switch between databases using the SELECT command. Logical databases are useful for separating different types of data or isolating data for different applications within the same Redis instance. Logical databases exist within individual nodes. Each Redis node can have multiple logical databases, and these databases are independent of each other. When you connect to a Redis node, you can select a specific logical database on that node using the SELECT command.

When you connect to a specific node, you can work with the logical databases hosted by that node. Each node can have its own set of logical databases, providing data separation and isolation for different use cases or applications.


#### Relationship between Logical Database, Nodes, and Slots:

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

### How keys are routed to the appropriate database within a Redis Cluster.

### Example: Routing Keys to Logical Databases in a Redis Cluster

Consider a Redis Cluster with three nodes: Node A, Node B, and Node C. Each node has 16 logical databases (indexed from 0 to 15). The cluster is responsible for 16384 hash slots.

#### Step 1: Connecting to a Node
When a client connects to the Redis Cluster, it can connect to any of the nodes. Let's assume the client connects to Node A (`127.0.0.1:7000`).

#### Step 2: Sending a Command with a Key
The client wants to set a key "user:1234". To determine which logical database the key belongs to, Redis Cluster uses a hash function on the key to map it to a specific slot number (between 0 and 16383).

In this example, let's say the hash slot for "user:1234" is calculated to be 5678.

#### Step 3: Mapping Slot to Node and Database
1. **Slot to Node Mapping:**
   - Node A: Slots 0-5460
   - Node B: Slots 5461-10921
   - Node C: Slots 10922-16383

2. **Database Selection:**
   Since Node A is responsible for slots 0-5460, the key "user:1234" (slot 5678) belongs to Node A.

3. **Logical Database Selection:**
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



