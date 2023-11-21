using StackExchange.Redis;

var userAgeSet = "users:age";
var userLastAccessSet = "users:lastAccess";
var userHighScoreSet = "users:highScores";
var namesSet = "names";
var mostRecentlyActive = "users:mostRecentlyActive";

var muxer = await ConnectionMultiplexer.ConnectAsync(new ConfigurationOptions
{
    EndPoints = { "localhost:6379" }
});

var db = muxer.GetDatabase();
await db.KeyDeleteAsync(new RedisKey[] { userAgeSet, userLastAccessSet, userHighScoreSet, namesSet, mostRecentlyActive });

var Populate = async () =>
{
    var tsk1 = db.SortedSetAddAsync(userAgeSet,
         new SortedSetEntry[] {
            new("User:1", 20),
            new("User:2", 23),
            new("User:3", 18),
            new("User:4", 35),
            new("User:5", 55),
            new("User:6", 62)
         });

    var tsk2 = db.SortedSetAddAsync(userLastAccessSet,
        new SortedSetEntry[] {
            new("User:1", 1648483867),
            new("User:2", 1658074397),
            new("User:3", 1659132660),
            new("User:4", 1652082765),
            new("User:5", 1658087415),
            new("User:6", 1656530099)
        });

    var tsk3 = db.SortedSetAddAsync(userHighScoreSet,
        new SortedSetEntry[]
        {
            new("User:1", 10),
            new("User:2", 55),
            new("User:3", 36),
            new("User:4", 25),
            new("User:5", 21),
            new("User:6", 44)
        });

    var tsk4 = db.SortedSetAddAsync(namesSet,
        new SortedSetEntry[]
        {
            new("John", 0),
            new("Fred", 0),
            new("Bob", 0),
            new("Susan", 0),
            new("Alice", 0),
            new("Tom", 0)
        });
};

var Fetch = async () => {
    var user3HighScore = await db.SortedSetScoreAsync(userHighScoreSet, "User:3");
    Console.WriteLine($"User:3 High Score: {user3HighScore}");
};

await Populate();
await Fetch();