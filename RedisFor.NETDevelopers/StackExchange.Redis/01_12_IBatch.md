An IBatch will guarantee that the client sends the entire batch to Redis in one shot, with no other commands interleaved in the pipeline. This is slightly different behavior than our implicit pipelining as in the case of implicit pipelining, commands may be interleaved with any other commands the client was executing at the time.

  To explicitly pipeline these commands we'll follow a similar pattern, in this case, however, we will use the IDatabase.CreateBatch() method to create the batch, and use the batch's async methods to 'dispatch' the the tasks. It's important to note here that unlike in our implicit case, the tasks will not be truly dispatched until after the IBatch.Execute() method is called, if you try awaiting any of the tasks before then, you can accidentally deadlock your command. After calling Execute, you can then await all of the tasks.

  Batches allow you to more intentionally group together the commands that you want to send to Redis. If you employee a batch, all commands in the batch will be sent to Redis in one contiguous block and executed in the order they were added, with no other commands from the client interleaved. Of course, if there are other clients to Redis, commands from those other clients may be interleaved with your batched commands.
  
 ```cs
 var options = new ConfigurationOptions
 {
     EndPoints =
 {
     "localhost:6379"
 }
 };

 var muxer = await ConnectionMultiplexer.ConnectAsync(options);
 var db = muxer.GetDatabase();
 var pingTasks = new List<Task<TimeSpan>>();
 IBatch batch = db.CreateBatch();
 var stopWatch = Stopwatch.StartNew();
 for (var i = 0; i < 1000; i++)
 {
     pingTasks.Add(batch.PingAsync());
 }
 batch.Execute();
 await Task.WhenAll(pingTasks);
 Console.WriteLine($"1000 batched commands took: {stopWatch.ElapsedMilliseconds}ms to execute, first result: {pingTasks[0].Result}");
 ```
