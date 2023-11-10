### Set
In Redis, a set is an unordered collection of strings that contains no duplicates. So if you add the same value to an empty set 20 times or even a million times, the set will contain just one member.

This makes sets a natural fit for tasks like de-duplication. For ex:
* Did I see this IP address in the last hour?
* Is this user online?
* Has this URL been blacklisted?" 
* Redis sets support standard mathematical set operations, like intersection, difference, and union.
