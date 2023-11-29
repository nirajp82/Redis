using StackExchange.Redis;
using System.Threading;

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
//Reset the sorted set.
await db.KeyDeleteAsync(new RedisKey[] { userAgeSet, userLastAccessSet, userHighScoreSet, namesSet, mostRecentlyActive });

var PopulateAsync = async () =>
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

    await Task.WhenAll(tsk1, tsk2, tsk3, tsk4);
};
var FetchByScoreAsync = async () =>
{
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

var FetchByRangeByScoreAsync = async () =>
{
    var eighteenToThirty = await db.SortedSetRangeByScoreWithScoresAsync(userAgeSet, 18, 30, Exclude.None);
    Console.WriteLine($"Users between 18 and 30: {string.Join(", ", eighteenToThirty)}");//User:3: 18, User:1: 20, User:2: 23
};

var FetchSortedSetRangeByValueAsync = async () =>
{
    var namesAlphabetized = await db.SortedSetRangeByValueAsync(namesSet);
    Console.WriteLine($"SortedSetRangeByValueAsync: {string.Join(", ", namesAlphabetized)}");//Alice, Bob, Fred, John, Susan, Tom

    var namesBetweenAandS = await db.SortedSetRangeByValueAsync(namesSet, "A", "S", Exclude.Stop);
    Console.WriteLine($"Names between A and J: {string.Join(", ", namesBetweenAandS)}");//Alice, Bob, Fred, John
};

var CombiningSortedSetsAsync = async () =>
{
    //find the three most recently active players, and then determine the rank order of those three by high score,
    //populate mostRecentlyActive setf
    await db.SortedSetRangeAndStoreAsync(userLastAccessSet, mostRecentlyActive, 0, 2, order: Order.Descending);

    var mostRecentActive = await db.SortedSetRangeByValueAsync(mostRecentlyActive);
    Console.WriteLine($"mostRecentActive: {string.Join(", ", mostRecentActive)}");//User:2, User:5, User:3

    var highScore = await db.SortedSetRangeByValueAsync(userHighScoreSet);
    Console.WriteLine($"HighScore: {string.Join(", ", highScore)}"); //User:1, User:5, User:4, User:3, User:6, User:2

    //Weight the high score to 1, and the last access time to 0, producing only the high score.
    var rankOrderMostRecentlyActive = db.SortedSetCombineWithScores(SetOperation.Intersect,
        new RedisKey[] { userHighScoreSet, mostRecentlyActive }, new double[] { 1, 0 })
        .Reverse();
    Console.WriteLine($"Highest Scores Most Recently Active: {string.Join(", ", rankOrderMostRecentlyActive)}");//User:2: 55, User:3: 36, User:5: 21
                                                                                                                //
    rankOrderMostRecentlyActive = db.SortedSetCombineWithScores(SetOperation.Intersect,
        new RedisKey[] { userHighScoreSet, mostRecentlyActive })
        .Reverse();
    Console.WriteLine($"Highest Scores Most Recently Active: {string.Join(", ", rankOrderMostRecentlyActive)}");//User:3: 1659132696, User:5: 1658087436, User:2: 1658074452   
};


await PopulateAsync();
await FetchByScoreAsync();
FetchByRank();
await FetchByRankAsync();
await FetchByRangeByScoreAsync();
await FetchSortedSetRangeByValueAsync();
await CombiningSortedSetsAsync();