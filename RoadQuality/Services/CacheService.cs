using Microsoft.Extensions.Caching.Memory;
using RoadQuality.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoadQuality.Services
{
    public class CacheService
    {
        private IMemoryCache _cache;
        public CacheService(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
        }
        public RoadPointDTO GetGeoPointFromCache(string userId)
        {
            RoadPointDTO point;
            var cacheKey = CreateGeoPointCacheKey(userId);
            if (!_cache.TryGetValue(cacheKey, out point))
            {
                return null;
            }
            return point;
        }

        public void SaveGeoPointInCache(RoadPointDTO point, string userId)
        {
            var cacheExpiryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTime.Now.AddSeconds(15),
                Priority = CacheItemPriority.High,
                SlidingExpiration = TimeSpan.FromSeconds(10)
            };

            var cacheKey = CreateGeoPointCacheKey(userId);

            _cache.Set(cacheKey, point, cacheExpiryOptions);
        }

        private string CreateGeoPointCacheKey(string userId)
        {
            return userId + "-UserStats";
        }
    }
}
