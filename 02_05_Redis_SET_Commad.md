### Set
Redis Sets are an implementation of a mathematical set. Like mathematical sets they have a number of key properties:

* They are unordered.
* They do not allow duplication, so there are no repeated members.
* They can be combined together to create new sets

In Redis, a set is an unordered (no position or index used to retrieve values.) collection of strings that contains no duplicates. So if you add the same value to an empty set 20 times or even a million times, the set will contain just one member.

This makes sets a natural fit for tasks like de-duplication. For ex:
* Did I see this IP address in the last hour?
* Is this user online?
* Has this URL been blacklisted?" 
* Redis sets support standard mathematical set operations, like intersection, difference, and union.

  let's say you want to record the unique visitors of each page on our website for a given period of time. A set can be created for each unique URL and time period. For example  Each visitors cookie can be recorded as a member of the set. The set now contains all the unique visitors to that page within the time period which can be retrieved with the SSCAN command. EXPIRE can be used to define the retention period for the metric.

Certainly! Here's an explanation of the Redis Set commands you provided along with examples:

### SADD
- **Description**: Adds one or more members to a set. Creates the key if it doesn't exist.
- **Example**:
  ```bash
  > SADD myset "member1"
  > SADD myset "member2" "member3"
  ```
  This command adds "member1" to the set named 'myset'. It then adds "member2" and "member3" to the same set.

### SCARD
- **Description**: Returns the number of members in a set.
- **Example**:
  ```bash
  > SCARD myset
  ```
  Returns the cardinality (number of members) of the set 'myset'.

### SDIFF
- **Description**: Returns the difference of multiple sets.
- **Example**:
  ```bash
   > sadd set-one a b c d e
   > sadd set-two e f g
   > sadd set-three c e
   > sdiff set-one set-two set-three
  ```
  SDIFF performs a logical subtraction from the first Set with the subsequent Sets given. Result would a, b, and d

### SDIFFSTORE
- **Description**: Stores the difference of multiple sets in a key.
- **Example**:
  ```bash
  > SDIFFSTORE difference_set set1 set2
  ```
  Stores the elements that are in 'set1' but not in 'set2' into a new set named 'difference_set'.

### SINTER
- **Description**: Returns the intersect of multiple sets.
- **Example**:
  ```bash
  > sadd set-one a b c d e
   > sadd set-two e f g
   > sadd set-three c e
   > sinter set-one set-two set-three
  ```
  Returns the elements that are common in 'set-one', 'set-two' and 'set-three'. >> e

### SINTERCARD
- **Description**: Returns the number of members of the intersect of multiple sets.
- **Example**:
  ```bash
  > SINTERCARD set1 set2
  ```
  Returns the number of elements that are common to both 'set1' and 'set2'.

### SINTERSTORE
- **Description**: Stores the intersect of multiple sets in a key.
- **Example**:
  ```bash
  > SINTERSTORE intersection_set set1 set2
  ```
  Stores the elements that are common to both 'set1' and 'set2' into a new set named 'intersection_set'.

### SISMEMBER
- **Description**: Determines whether a member belongs to a set.
- **Example**:
  ```bash
  > SISMEMBER myset "member1"
  ```
  Checks if "member1" is a member of the set 'myset'.

### SMEMBERS
- **Description**: Returns all members of a set.
- **Example**:
  ```bash
  > SMEMBERS myset
  ```
  Returns all members of the set 'myset'.

### SMISMEMBER
- **Description**: Determines whether multiple members belong to a set.
- **Example**:
  ```bash
  > SMISMEMBER myset "member1" "member2"
  ```
  Checks if "member1" and "member2" are members of the set 'myset'.

### SMOVE
- **Description**: Moves a member from one set to another.
- **Example**:
  ```bash
  > SMOVE source_set destination_set "member1"
  ```
  Moves "member1" from 'source_set' to 'destination_set'.

### SPOP
- **Description**: Returns one or more random members from a set after removing them. Deletes the set if the last member was popped.
- **Example**:
  ```bash
  > SPOP myset
  ```
  Removes and returns a random member from 'myset'. If 'myset' becomes empty, the set is deleted.

### SRANDMEMBER
- **Description**: Get one or multiple random members from a set.
- **Example**:
  ```bash
  > SRANDMEMBER myset 2
  ```
  Returns 2 random members from 'myset' without removing them.

### SREM
- **Description**: Removes one or more members from a set. Deletes the set if the last member was removed.
- **Example**:
  ```bash
  > SREM myset "member1" "member2"
  ```
  Removes "member1" and "member2" from 'myset'. If 'myset' becomes empty, the set is deleted.

### SSCAN
- **Description**: Iterates over members of a set.
- **Example**:
  ```bash
  > SSCAN myset 0
  ```
  Starts an iteration over members of 'myset'. The '0' is the cursor indicating the start.

### SUNION
- **Description**: Returns the union of multiple sets.
- **Example**:
  ```bash
   > sadd set-one a b c d e
   > sadd set-two e f g
   > sadd set-three c e
   > SUNION set-one set-two set-three
  ```
  Returns all unique elements present in 'set-one', 'set-two' and 'set-three'. >> a,b,c,d,e,f,g

### SUNIONSTORE
- **Description**: Stores the union of multiple sets in a key.
- **Example**:
  ```bash
  > SUNIONSTORE union_set set1 set2
  ```
  Stores all unique elements present in 'set1' or 'set2' or both into a new set named 'union_set'.
