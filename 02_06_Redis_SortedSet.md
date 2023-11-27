In Redis, a Sorted Set is a data structure that combines the features of a set and a sorted map. It is an ordered collection of unique elements, where each element is associated with a score. The score is used to maintain the order of elements, and it must be a floating-point number or integer. Sorted Sets are often used when you need to represent a collection of unique items with an associated ranking or score. Here are some key aspects of Redis Sorted Sets along with examples:

* Rank (Aka index) is the absolute position of the element in the Sorted Set. Rank is zero based, with zero starting at the lowest scoring element. ZRANK can be used to find out the rank for a given element. Multiple elements of a Sorted Set can not have the same Rank as rank is absolute position of the element in the Sorted Set. 

* If members are added with same score, they will be returned in lexicographic order *not* insertion order.

* Value are unique in a Sorted Set, so duplicate values are not allowed even if they have different scores. Adding a value with ZADD, where the value already exists, results in updating the score with the value supplied in the ZADD command.
  
* The sorted set can be traversed in ascending or descending order and can be manipulated by score, value and lexicography.

* The sorted sets also provide the ability to create a union, intersection and difference results from a number of other sets and sorted sets. The Difference operator was added in Redis 6.2 as the ZDIFF command.


**Introduction**

Sorted sets are a type of data structure that stores elements in a sorted order. They are useful for storing collections of elements that need to be accessed in a sorted order, such as lists of users or scores.

**Populating Sorted Sets**

To populate a sorted set, you can use the `SortedSetAdd` method. This method takes two arguments: the name of the sorted set and an array of `SortedSetEntry` objects. A `SortedSetEntry` object has two properties: `Member` and `Score`. The `Member` property is the name of the element to add to the sorted set, and the `Score` property is the score of the element.

**Checking Score and Rank**

To check the score of an element in a sorted set, you can use the `SortedSetScore` method. This method takes two arguments: the name of the sorted set and the name of the element.

To check the rank of an element in a sorted set, you can use the `SortedSetRank` method. This method takes three arguments: the name of the sorted set, the name of the element, and an `Order` object. The `Order` object can be either `Ascending` or `Descending`.

**Range Queries**

There are three ways to query members from a sorted set:

* **By rank:** This is the default method for ranges with sorted sets. You can use the `SortedSetRangeByRank` method to range over a sorted set by rank.

* **By score:** You can use the `SortedSetRangeByScore` method to range over a sorted set by score.

* **By lex:** Lexicographic ordering is used when all the scores within a sorted set are set to the same score, traditionally 0. You can use the `SortedSetRangeByValue` method to range over a sorted set by lex.

**Combining Sorted Sets**

You can combine sorted sets together to help you answer more interesting questions. For example, you can use the `SortedSetCombineWithScores` method to find the intersection of two sorted sets.

**Example**

The following example shows how to use sorted sets to store a list of users and their scores. The example then shows how to query the sorted set to find the three most recently active users and then determine the rank order of those three by high score.

```c#
db.SortedSetAdd(userHighScoreSet,
        new SortedSetEntry[]
        {
            new("User:1", 10),
            new("User:2", 55),
            new("User:3", 36),
            new("User:4", 25),
            new("User:5", 21),
            new("User:6", 44)
        });

var FetchByScoreAsync = async () => {
    var user3Score = await db.SortedSetScoreAsync(userHighScoreSet, "User:3");
    Console.WriteLine($"User:3 Score: {user3Score}");//User:3 Score: 36
};

var FetchByRank = () =>
{
    var user2Rank = db.SortedSetRank(userHighScoreSet, "User:2", Order.Descending);
    Console.WriteLine($"User:2 Rank: {user2Rank}"); //User:2 Rank: 0    
};

var FetchByRankAsync = async () =>
{
    var topThreeScores = await db.SortedSetRangeByRankAsync(userHighScoreSet, 0, 2, Order.Descending);
    Console.WriteLine($"Top three: {string.Join(", ", topThreeScores)}");//Top three: User:2, User:6, User:3
};
```

### Use Cases
 Let's go through the commands for building a priority queue and a leaderboard using Redis Sorted Sets, along with explanations for each command:

### Building a Priority Queue:

1. **Enqueue (Add an item with priority):**
    ```bash
    > ZADD priority_queue 10 "task1"
    > ZADD priority_queue 5 "task2"
    > ZADD priority_queue 15 "task3"
    ```

    - **Explanation**: The `ZADD` command adds members with associated scores to the Sorted Set (`priority_queue`). In this case, "task1" has a priority of 10, "task2" has a priority of 5, and "task3" has a priority of 15.

2. **Dequeue (Get the item with the highest priority):**
    ```bash
    > ZPOPMAX priority_queue
    ```

    - **Explanation**: The `ZPOPMAX` command removes and returns the member with the highest score (priority) from the Sorted Set (`priority_queue`). This simulates dequeuing the task with the highest priority.

3. **Check the size of the Priority Queue:**
    ```bash
    > ZCARD priority_queue
    ```

    - **Explanation**: The `ZCARD` command returns the number of elements in the Sorted Set (`priority_queue`). This gives the current size of the priority queue.

### Building a Leaderboard:

1. **Add Players with Scores:**
    ```bash
    > ZADD leaderboard 100 "player1"
    > ZADD leaderboard 150 "player2"
    > ZADD leaderboard 120 "player3"
    ```

    - **Explanation**: The `ZADD` command adds players with associated scores to the Sorted Set (`leaderboard`). In this case, "player1" has a score of 100, "player2" has a score of 150, and "player3" has a score of 120.

2. **Get Player Rank:**
    ```bash
    > ZRANK leaderboard "player2"
    ```

    - **Explanation**: The `ZRANK` command returns the rank of a member ("player2") in the Sorted Set (`leaderboard`). Ranks are 0-based, so the highest scorer has a rank of 0.

3. **Get Player Score:**
    ```bash
    > ZSCORE leaderboard "player1"
    ```

    - **Explanation**: The `ZSCORE` command returns the score of a member ("player1") in the Sorted Set (`leaderboard`). It retrieves the player's current score.

4. **Get Range of Top Players:**
    ```bash
    > ZREVRANGE leaderboard 0 2 WITHSCORES
    ```

    - **Explanation**: The `ZREVRANGE` command returns a range of members with scores in descending order from the Sorted Set (`leaderboard`). In this case, it returns the top 3 players with their scores.

5. **Get Range of Players within a Score Range:**
    ```bash
    > ZRANGEBYSCORE leaderboard 100 130 WITHSCORES
    ```

    - **Explanation**: The `ZRANGEBYSCORE` command returns a range of members with scores within a specified range from the Sorted Set (`leaderboard`). In this case, it returns players with scores between 100 and 130.

6. **Increment Player Score:**
    ```bash
    > ZINCRBY leaderboard 30 "player1"
    ```

    - **Explanation**: The `ZINCRBY` command increments the score of a member ("player1") in the Sorted Set (`leaderboard`). This can be used to update a player's score.

### Additional Operations for Both Priority Queue and Leaderboard:

1. **Remove a Player or Task:**
    ```bash
    > ZREM leaderboard "player2"
    > ZREM priority_queue "task2"
    ```

    - **Explanation**: The `ZREM` command removes a member from the Sorted Set (`leaderboard` or `priority_queue`), effectively removing a player from the leaderboard or a task from the priority queue.

2. **Check if a Player or Task Exists:**
    ```bash
    > ZSCORE leaderboard "player1"
    > ZSCORE priority_queue "task1"
    ```

    - **Explanation**: The `ZSCORE` command can be used to check if a member ("player1" or "task1") exists in the Sorted Set (`leaderboard` or `priority_queue`).

3. **Get Members with Scores in a Range:**
    ```bash
    > ZRANGEBYSCORE leaderboard 100 150
    > ZRANGEBYSCORE priority_queue 5 20
    ```

    - **Explanation**: The `ZRANGEBYSCORE` command returns members with scores within a specified range from the Sorted Set (`leaderboard` or `priority_queue`).

4. **Union and Intersection Operations:**
    ```bash
    > ZUNIONSTORE combined_leaderboard 2 leaderboard priority_queue WEIGHTS 1 2
    > ZINTERSTORE common_priority_queue 2 priority_queue another_priority_queue WEIGHTS 2 1
    ```

    - **Explanation**: The `ZUNIONSTORE` and `ZINTERSTORE` commands perform union and intersection operations, respectively, on multiple Sorted Sets (`leaderboard` or `priority_queue`). The `WEIGHTS` option can be used to give different weights to the scores of each set during the union or intersection.

In summary, these commands and operations demonstrate how Redis Sorted Sets can be effectively used to build a priority queue and a leaderboard, providing flexibility and efficiency in managing ordered collections of unique elements with associated scores.

### Sorted Set Operations:

1. **ZADD**: Adds one or more members to a sorted set, or updates the score if the member already exists.

    ```bash
    > ZADD myset 10 "member1"
    > ZADD myset 20 "member2" 30 "member3"
    ```

2. **ZCARD**: Returns the number of elements in a sorted set.

    ```bash
    > ZCARD myset
    ```

3. **ZCOUNT**: Returns the number of elements in a sorted set with a score within the given range.

    ```bash
    > ZCOUNT myset 15 25
    ```

4. **ZINCRBY**: Increments the score of a member in a sorted set.

    ```bash
    > ZINCRBY myset 5 "member1"
    ```

5. **ZRANGE**: Returns a range of members in a sorted set by index.

    ```bash
    > ZRANGE myset 0 2
    ```

6. **ZRANGEBYSCORE**: Returns a range of members in a sorted set by score.

    ```bash
    > ZRANGEBYSCORE myset 15 30
    ```

7. **ZREM**: Removes one or more members from a sorted set.

    ```bash
    > ZREM myset "member1" "member2"
    ```

8. **ZREMRANGEBYRANK**: Removes all members in a sorted set with rank between start and stop.

    ```bash
    > ZREMRANGEBYRANK myset 0 1
    ```

9. **ZREMRANGEBYSCORE**: Removes all members in a sorted set with score between min and max.

    ```bash
    > ZREMRANGEBYSCORE myset 10 20
    ```

10. **ZSCORE**: Returns the score of a member in a sorted set.

    ```bash
    > ZSCORE myset "member1"
    ```

11. **ZINTERSTORE**: Computes the intersection of multiple sorted sets and stores the result in a new key.

    ```bash
    > ZADD set1 1 "a" 2 "b" 3 "c"
    > ZADD set2 2 "b" 3 "c" 4 "d"
    > ZINTERSTORE result_set 2 set1 set2 WEIGHTS 2 3
    ```

12. **ZUNIONSTORE**: Computes the union of multiple sorted sets and stores the result in a new key.

    ```bash
    > ZADD set1 1 "a" 2 "b" 3 "c"
    > ZADD set2 2 "b" 3 "c" 4 "d"
    > ZUNIONSTORE result_set 2 set1 set2 WEIGHTS 2 3
    ```

https://redis.io/commands/?group=sortedset

In summary, Redis Sorted Sets are versatile and find application in scenarios where you need to maintain an ordered collection of unique elements with associated scores. They are particularly useful for scenarios that involve ranking, scoring, or ordering based on certain criteria.
