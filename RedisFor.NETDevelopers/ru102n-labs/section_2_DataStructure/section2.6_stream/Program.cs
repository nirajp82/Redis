using StackExchange.Redis;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net.WebSockets;

var muxer = await ConnectionMultiplexer.ConnectAsync(new ConfigurationOptions
{
    EndPoints = { "localhost:6379" }
});

var db = muxer.GetDatabase();

var sensor1 = "sensor:1";
var sensor2 = "sensor:2";

await db.KeyDeleteAsync(new RedisKey[] { sensor1, sensor2 });

var addAsync = async () =>
{
    long numInserted = 0;
    var s1Temp = 1;
    var s2Temp = 100001;
    var s1Humid = 500001;
    var s2Humid = 900001;
    var rnd = new Random();

    while (numInserted <= 10)
    {
        var s1Async = db.StreamAddAsync(sensor1, new[]
        {
            new NameValueEntry("temp",s1Temp),
            new NameValueEntry("humidity",s1Humid)
        });

        var s2Async = db.StreamAddAsync(sensor2, new[]
        {
            new NameValueEntry("temp",s2Temp),
            new NameValueEntry("humidity",s2Humid)
        });

        await Task.WhenAll(s1Async, s2Async);
        await Task.Delay(1000);
        numInserted++;
        s1Temp = s1Temp + 1; //rnd.Next(3) - 2;
        s2Temp = s2Temp + 1;// rnd.Next(3) - 2;
        s1Humid = s1Humid + 1;// Math.Min(s1Humid + rnd.Next(3) - 2, 100);
        s2Humid = s2Humid + 1;// Math.Min(s2Humid + rnd.Next(3) - 2, 100);
    }
};

var readAsync = async () =>
{
    //"0-0" is a concise representation in Redis streams to specify the earliest position in a stream,
    //essentially instructing Redis to begin reading from the very beginning of the stream.
    //The first zero is for the stream ID, the hyphen separates it from the entry ID,
    //and the second zero is the initial entry ID within the stream.
    var sensor1Pos = new StreamPosition(sensor1, "0-0");
    var sensor2Pos = new StreamPosition(sensor2, "0-0");

    // Initialize a dictionary to store the current positions of each sensor in a Redis stream.
    var positions = new Dictionary<string, StreamPosition>
    {
        { sensor1,  sensor1Pos },
        { sensor2, sensor2Pos }
    };
    long numRead = 0;

    while (numRead <= 10)
    {
        // Asynchronously read stream entries from Redis for all sensors.
        // The countPerStream: 1 parameter ensures that only one entry per stream is read in each iteration.
        var readResults = await db.StreamReadAsync(positions.Values.ToArray(), countPerStream: 3);

        // Check if there are no entries for any sensor.
        if (!readResults.Any(x => x.Entries.Any()))
        {
            // If no entries found, wait for 1000 milliseconds and continue to the next iteration.
            await Task.Delay(200);
            continue;
        }

        // Process each stream's entries if they exist.
        foreach (var stream in readResults)
        {
            // Iterate through each entry in the current stream.
            foreach (var entry in stream.Entries)
            {
                // Print information about the entry to the console.
                Console.WriteLine($"{stream.Key} - {entry.Id}: {string.Join(", ", entry.Values)}");

                // positions[stream.Key!]: This line updates the stored position for a specific Redis stream (stream.Key)
                // in the positions dictionary. The stream.Key! uses the null-forgiving operator (!) to indicate to the
                // compiler that the key will not be null, suppressing any potential null reference warnings.
                // The new position is set to the ID of the last processed entry in the stream, ensuring
                // that the next read operation starts from this position.
                positions[stream.Key!] = new StreamPosition(stream.Key, entry.Id);
            }
        }
        numRead++;
    }
};

await Task.WhenAll(addAsync(), readAsync());

Console.WriteLine("Start consumer group");

//----------------------Create a Consumer Group
string groupName = "tempAverage";
var createConsumerGroupAsync = async () =>
{
    var s1Group = db.StreamCreateConsumerGroupAsync(sensor1, groupName, "0-0");
    var s2Group = db.StreamCreateConsumerGroupAsync(sensor2, groupName, "0-0");
    await Task.WhenAll(s1Group, s2Group);
};

await createConsumerGroupAsync();

// Start a background task to continuously process stream entries.
var readFromGroup = Task.Run(async () =>
{
    // Dictionary to store the total temperature values for each sensor.
    var tempTotals = new Dictionary<string, int> { { sensor1, 0 }, { sensor2, 0 } };

    // Dictionary to keep track of the total number of processed messages for each sensor.
    var messageCountTotals = new Dictionary<string, int> { { sensor1, 0 }, { sensor2, 0 } };

    // Define a consumer name for identifying this consumer within the consumer group.
    var consumerName = "consumer:1";

        // Dictionary to store the current stream positions for each sensor, starting from the latest entry (">").
    var positions = new Dictionary<string, StreamPosition>
    {
        { sensor1, new StreamPosition(sensor1, ">") },
        { sensor2, new StreamPosition(sensor2, ">") }
    };

    // Run an infinite loop to continuously monitor and process stream entries.
    while (true)
    {
        // Asynchronously read stream entries from the consumer group for all sensors.
        var result = await db.StreamReadGroupAsync(positions.Values.ToArray(), groupName, consumerName, countPerStream: 1);

        // Check if there are no entries for any sensor.
        if (!result.Any(x => x.Entries.Any()))
        {
            // If no entries found, wait for 1000 milliseconds and continue to the next iteration.
            await Task.Delay(1000);
            continue;
        }

        // Process each stream's entries if they exist.
        foreach (var stream in result)
        {
            // Iterate through each entry in the current stream.
            foreach (var entry in stream.Entries)
            {
                // Extract the temperature value from the entry.
                var temp = (int)entry.Values.First(x => x.Name == "temp").Value;

                // Update the total message count for the current sensor.
                messageCountTotals[stream.Key!]++;

                // Update the total temperature value for the current sensor.
                tempTotals[stream.Key!] += temp;

                // Calculate the average temperature for the current sensor.
                var avg = (double)tempTotals[stream.Key!] / messageCountTotals[stream.Key!];

                // Print the average temperature to the console.
                Console.WriteLine($"{stream.Key} average Temp = {avg:0.###}");

                // Acknowledge the processed entry to mark it as successfully processed.
                await db.StreamAcknowledgeAsync(stream.Key, groupName, entry.Id);
            }
        }
    }
});

await Task.WhenAll(readFromGroup);
Console.ReadLine();