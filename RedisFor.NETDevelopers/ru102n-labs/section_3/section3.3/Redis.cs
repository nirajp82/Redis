using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace section3._3
{
    /// <summary>
    /// Represents a utility class for managing Redis connections and databases.
    /// </summary>
    public class Redis
    {
        /// <summary>
        /// Gets the lazy-initialized ConnectionMultiplexer instance.
        /// </summary>
        private static readonly Lazy<ConnectionMultiplexer> LazyMuxer;

        static Redis()
        {
            // Configuration options for connecting to the Redis server
            var options = new ConfigurationOptions
            {
                EndPoints = { "localhost:6379" }, // Replace with the actual Redis server endpoint
                Password = "" // Replace with the actual password if required
            };

            // Lazily initialize the ConnectionMultiplexer using a lambda expression
            LazyMuxer = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(options));
        }

        /// <summary>
        /// Gets the shared ConnectionMultiplexer instance.
        /// </summary>
        public static ConnectionMultiplexer Muxer => LazyMuxer.Value;

        /// <summary>
        /// Gets the default Redis database instance.
        /// </summary>
        public static IDatabase Database => Muxer.GetDatabase();
    }
 }