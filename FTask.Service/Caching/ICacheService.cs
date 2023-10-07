﻿using System.Linq.Expressions;

namespace FTask.Service.Caching
{
    public interface ICacheService<T> where T : class
    {
        Task<T?> GetAsync(string key);
        Task<T[]> GetAsyncArray(string key);
        Task SetAsync<T>(string key, T entity);
        Task RemoveAsync(string key);
    }
}
