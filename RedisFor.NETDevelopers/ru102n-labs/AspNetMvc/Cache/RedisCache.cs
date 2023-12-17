using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AspNetMvc.Cache
{
    public class RedisCache
    {
        static ConfigurationOptions options = new ConfigurationOptions
        {
            EndPoints = { "localhost:6379" }
        };

        /// <summary>
        /// Gets the lazy-initialized ConnectionMultiplexer instance.
        /// </summary>
        private static readonly Lazy<ConnectionMultiplexer> _lazyMuxer = new Lazy<ConnectionMultiplexer>
        (
            () => ConnectionMultiplexer.Connect(options)
        );

        static ConnectionMultiplexer _muxer = _lazyMuxer.Value;

        public static IDatabase GetDatabase()
        {
            return _muxer.GetDatabase();
        }
    }
}