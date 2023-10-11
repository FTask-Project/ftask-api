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

        public async Task RemoveAsync(string key)
        {
            await _distributedCache.RemoveAsync(key.ToString());
        }

        public async Task SetAsync<Y>(string key, Y entity)
        {
            string cacheData = JsonConvert.SerializeObject(entity, new JsonSerializerSettings()
            {
                //NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            await _distributedCache.SetStringAsync(key.ToString(), cacheData);
        }
    }
}
