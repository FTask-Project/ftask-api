using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace FTask.Service.Caching
{
    public class CacheService<T> : ICacheService<T> where T : class
    {
        private readonly IDistributedCache _distributedCache;

        public CacheService(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public async Task<T?> GetAsync(string key)
        {
            string? cacheData = await _distributedCache.GetStringAsync(key.ToString());

            if (cacheData is null)
            {
                return null;
            }

            T? deserializedData = JsonConvert.DeserializeObject<T>(cacheData);
            return deserializedData;
        }

        public async Task<T[]> GetAsyncArray(string key)
        {
            string? cacheData = await _distributedCache.GetStringAsync(key.ToString());

            if (cacheData is null)
            {
                return new T[0];
            }

            T[] deserializedData = JsonConvert.DeserializeObject<T[]>(cacheData) ?? new T[0];
            return deserializedData;
        }

        public async Task SetAsync(string key, T entity)
        {
            string cacheData = JsonConvert.SerializeObject(entity, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            });

            await _distributedCache.SetStringAsync(key.ToString(), cacheData);
        }

        public async Task SetAsyncArray(string key, T[] entity)
        {
            await SetAsyncArray(key, entity, 5);
        }

        public async Task SetAsyncArray(string key, T[] entity, double expiredMinute)
        {
            var timeToLive = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(expiredMinute));

            string cacheData = JsonConvert.SerializeObject(entity);

            await _distributedCache.SetStringAsync(key.ToString(), cacheData, timeToLive);
        }

        public async Task RemoveAsync(string key)
        {
            await _distributedCache.RemoveAsync(key.ToString());
        }
    }
}
