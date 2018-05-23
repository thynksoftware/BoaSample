using System;
using System.Collections.Generic;
using System.Linq;
using FluentScheduler;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Boa.Sample.Services
{
    public class MemoryCacheRepository : ICacheRepository
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<MemoryCacheRepository> _logger;
        private readonly List<Tuple<string, string, DateTime?>> _keys = new List<Tuple<string, string, DateTime?>>();
        
        public MemoryCacheRepository(IMemoryCache memoryCache, ILogger<MemoryCacheRepository> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;

            StartExipiryMonitor();
        }

        public void ClearAll()
        {
            WrapInKeysLock(() =>
            {
                try
                {
                    for (var index = _keys.Count - 1; index >= 0; index--)
                    {
                        var managedKey = _keys[index].Item1;
                        Remove(managedKey);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Could not clear all cache from memory", ex);
                }
                return true;
            });
        }

        public T Get<T>(string key)
        {
            return WrapInKeysLock(() =>
            {
                return _keys.Any(tuple => tuple.Item1 == key) ? _memoryCache.Get<T>(key) : default(T);
            });
        }

        public T Put<T>(string key, T item, CacheRepositoryOptions cacheRepositoryOptions, string category)
        {
            return WrapInKeysLock(() =>
            {
                if (_keys.Any(tuple => tuple.Item2 == category && tuple.Item1 == key))
                {
                    Remove(key);
                }

                DateTime? expiryDate = null;

                if (cacheRepositoryOptions != null)
                {
                    if (cacheRepositoryOptions.AbsoluteExpirationRelativeToNow.HasValue)
                    {
                        expiryDate = DateTime.UtcNow + cacheRepositoryOptions.AbsoluteExpirationRelativeToNow;
                    }
                    else if (cacheRepositoryOptions.AbsoluteExpiration.HasValue)
                    {
                        expiryDate = cacheRepositoryOptions.AbsoluteExpiration.Value.UtcDateTime;
                    }
                }

                _keys.Add(new Tuple<string, string, DateTime?>(key, category, expiryDate));

                return _memoryCache.Set(key, item, cacheRepositoryOptions);
            });
        }

        public void Remove(string key, bool contains = false)
        {
            WrapInKeysLock(() =>
            {
                var keys = new List<string>();

                if (contains)
                {
                    keys =
                        _keys.Where(tuple => tuple.Item1.ToLower().Contains(key.ToLower())).Select(tuple => tuple.Item1).ToList();                    
                }
                else
                {
                    keys.Add(key);
                }

                foreach (var x in keys)
                {
                    _memoryCache.Remove(x);
                }
                
                _keys.RemoveAll(tuple => keys.Contains(tuple.Item1));

                return true;
            });

        }

        public void RemoveByCategory(string category)
        {
            WrapInKeysLock(() =>
            {
                var keys = _keys.Where(tuple => tuple.Item2 == category).Select(tuple => tuple.Item1);

                foreach (var key in keys)
                {
                    Remove(key);
                }

                return true;
            });

        }

        private void StartExipiryMonitor()
        {

            JobManager.AddJob(() => CheckExpiredCache(), (s) => s.ToRunEvery(5).Minutes());
        }
        
        public void CheckExpiredCache()
        {
            try
            {
                var expiredKeys =
                    _keys?.Where(x =>
                        x.Item3.HasValue &&
                         (x.Item3.Value - DateTime.UtcNow).TotalMilliseconds < 0)?.ToList();

                expiredKeys.ForEach(expiredKey =>
                {
                    Remove(expiredKey.Item1);
                });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
        }

        private TResult WrapInKeysLock<TResult>(Func<TResult> action)
        {
            lock (_keys)
            {
                return action.Invoke();
            }
        }
    }
}