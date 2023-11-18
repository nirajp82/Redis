﻿using Redis.OM;
using Redis.OM.Modeling;
using section5._2;

var provider = new RedisConnectionProvider("redis://localhost:6379");

provider.Connection.DropIndexAndAssociatedRecords(typeof(Sale));
provider.Connection.DropIndexAndAssociatedRecords(typeof(Employee));

await provider.Connection.CreateIndexAsync(typeof(Sale));
await provider.Connection.CreateIndexAsync(typeof(Employee));

// TODO for Coding Challenge Start here on starting-point branch

// end coding challenge