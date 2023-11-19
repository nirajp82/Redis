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


---
```cs
    var allUsersSet = "users";
    var activeUsersSet = "users:state:active";
    var inactiveUsersSet = "users:state:inactive";
    var offlineUsersSet = "users:state:offline";
```
**Populate Sub Sets**

Now let's go about populating our active, inactive, and offline user sets. To do this, we'll use the `SetAdd` Method. This method is variadic, so we can do it in one command for each set.

```csharp
db.SetAdd(activeUsersSet, new RedisValue[]{"User:1", "User:2"});
db.SetAdd(inactiveUsersSet, new RedisValue[]{"User:3", "User:4"});
db.SetAdd(offlineUsersSet, new RedisValue[]{"User:5", "User:6", "User:7"});
```

**Combining sets to get all users**

You'll notice that we did not populate our `allUsersSet` set. If we consider active, inactive, offline to be an exhaustive list of states, we can use the set combination operations to get a set with all of our users. We can even use `SetCombineAndStore` to store those combined users in our all users key.

```csharp
db.SetCombineAndStore(SetOperation.Union, allUsersSet, new RedisKey[]{activeUsersSet, inactiveUsersSet, offlineUsersSet});
```

**Check Membership**

Sets do not allow duplication, but they allow very rapid O(1) membership checks. So if we wanted to check to see if `User:6` is offline, we could do so very easily:

```csharp
var user6Offline = db.SetContains(offlineUsersSet, "User:6");
Console.WriteLine($"User:6 offline: {user6Offline}");
```

**Enumerate Set**

When you want to enumerate the members of a set, you have two options: you can enumerate them all in one shot, or you can scan over the set and enumerate everything. We'll go over how to do each of those here.

**Enumerate Entire Set**

If you want to guarantee that you are enumerating the entire set in one round trip, you can do so by using the `SetMembers` method. This will use the `SMEMBERS` command in Redis. If your set is relatively compact (under 1000 members), this is a perfectly valid way to pull back all of your set members.

```csharp
Console.WriteLine($"All Users In one shot: {string.Join(", ", db.SetMembers(allUsersSet))}");
```

**Enumerate Set in Chunks**

The alternate way to enumerate a set is to enumerate it with `SetScan`, which will create a Set Enumerator and use the `SSCAN` command to scan over the entire set until the set is exhausted.

```csharp
Console.WriteLine($"All Users with scan  : {string.Join(", ", db.SetScan(allUsersSet))}");
```

**Move Elements Between Sets**

A very normal operation you might need to perform with sets is to move elements between them. For example, if `User:1` were to move offline, you can use `SetMove` to move them from the active user set to the offline user set.

```csharp
Console.WriteLine("Moving User:1 from active to offline");
var moved = db.SetMove(activeUsersSet, offlineUsersSet, "User:1");
Console.WriteLine($"Move Successful: {moved}");
```

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
