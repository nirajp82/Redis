using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AspNetMvc.Cache
{
    public class RedisCache
    {
        /// <summary>
        /// Gets the lazy-initialized ConnectionMultiplexer instance.
        /// </summary>
        private static readonly Lazy<ConnectionMultiplexer> _lazyMuxer;

        /// <summary>
        /// This will create the Multiplexer the first time it's referenced in our app, 
        /// and provides a static way of accessing a single multiplexer throughout our application.
        /// </summary>
        static RedisCache()
        {
            var options = new ConfigurationOptions
            {
                EndPoints = { "localhost:6379" },
                Password = "" 
            };

            _lazyMuxer = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(options));
        }

        static ConnectionMultiplexer _muxer = _lazyMuxer.Value;

        /// Gets the default Redis database instance.
        public static IDatabase Database 
        {
            get 
            {
               return _muxer.GetDatabase();
            }
        }
    }
}