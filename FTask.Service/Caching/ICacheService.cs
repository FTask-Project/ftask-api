using Microsoft.Extensions.Caching.Distributed;
using System.Linq.Expressions;

namespace FTask.Service.Caching
{
    public interface ICacheService<T> where T : class
    {
        Task<T> GetAsync(string key);
        Task<T[]> GetAsyncArray(string key);
        Task SetAsync(string key, T entity);
        Task SetAsyncArray(string key, T[] entity);
        Task SetAsyncArray(string key, T[] entity, double expiredMinute);
        Task RemoveAsync(string key);
    }
}
