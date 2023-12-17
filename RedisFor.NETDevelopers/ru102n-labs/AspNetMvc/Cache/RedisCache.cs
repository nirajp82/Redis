using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AspNetMvc.Cache
{
    /// <summary>
    /// Represents a utility class for managing Redis connections and databases.
    /// </summary>
    public class RedisCache
    {
        /// <summary>
        /// Gets the lazy-initialized ConnectionMultiplexer instance.
        /// </summary>
        private static readonly Lazy<ConnectionMultiplexer> _lazyMuxer;

        /// <summary>
        /// Initializes static members of the <see cref="Redis"/> class.
        /// </summary>
        static RedisCache()
        {
            // Configuration options for connecting to the Redis server
            var options = new ConfigurationOptions
            {
                EndPoints = { "localhost:6379" }, // Replace with the actual Redis server endpoint
                Password = "" // Replace with the actual password if required
            };

            // Lazily initialize the ConnectionMultiplexer using a lambda expression
            _lazyMuxer = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(options));
        }

        /// <summary>
        /// Gets the shared ConnectionMultiplexer instance.
        /// </summary>
        static ConnectionMultiplexer _muxer = _lazyMuxer.Value;

        /// <summary>
        /// Gets the default Redis database instance.
        /// </summary>
        public static IDatabase Database 
        {
            get 
            {
               return _muxer.GetDatabase();
            }
        }
    }
}