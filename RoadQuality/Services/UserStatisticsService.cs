using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RoadQuality.Configurations;
using RoadQuality.Models;
using RoadQuality.DTO;
using MongoDB.Driver.GeoJsonObjectModel;
using MongoDB.Entities;
using MongoDB.Bson;
using System.Net.Http;
using System.Text.Json;
using System.Net;
using Newtonsoft.Json;
using MongoDB.Bson.Serialization;
using Microsoft.Extensions.Caching.Memory;
using GeoLibrary.Model;
using Geolocation;

namespace RoadQuality.Services
{
    public class UserStatisticsService
    {
        private readonly IMongoCollection<UserStatisticsRecord> _stats;
        private readonly CacheService _cacheService;
        public UserStatisticsService(IDatabaseSettings settings, CacheService cacheService)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _cacheService = cacheService;

            _stats = database.GetCollection<UserStatisticsRecord>(settings.UserStatisticsCollectionName);
        }

        public async Task AddGeoPointToUserStats(RoadPointDTO point, string userId)
        {
            var now = DateTime.UtcNow;
            var currentMonth = new DateTime(now.Year, now.Month, 1);
            currentMonth = DateTime.SpecifyKind(currentMonth, DateTimeKind.Utc);
            var distance = CalculateDistanceFromLastPoint(point, userId);
            _cacheService.SaveGeoPointInCache(point, userId);

            UserStatisticsRecord userStats = new UserStatisticsRecord
            {
                Date = currentMonth,
                DistanceTraveled = distance,
                PointsCollected = 1,
                UserId = userId
            };

            var builder = Builders<UserStatisticsRecord>.Filter;
            var filter = builder.Eq(x => x.Date, userStats.Date) & builder.Eq(x => x.UserId, userId);

            var list = await _stats.Find(filter).Limit(1).ToListAsync();

            if (list.Count == 0)
            {
                await _stats.InsertOneAsync(userStats);
                return;
            } else
            {
                var update = Builders<UserStatisticsRecord>.Update;
                var updateDef = update.Inc(x => x.DistanceTraveled, userStats.DistanceTraveled).Inc(x => x.PointsCollected, 1);

                await _stats.UpdateOneAsync(filter, updateDef);
                return;
            }
        }

        public async Task<List<UserStatisticsRecord>> GetUserStats(string userId, DateRangeDTO dateRange)
        {
            var builder = Builders<UserStatisticsRecord>.Filter;
            var filter = builder.Eq(x => x.UserId, userId);
            filter &= builder.Lte(x => x.Date, dateRange.End);

            if (dateRange.Start.HasValue)
            {
                filter &= builder.Gte(x => x.Date, dateRange.Start);
            }

            var list = (await _stats.FindAsync(filter)).ToList();
            return list;
        }

        public async Task<OverallStatsDTO> GetOverallUserStats(string userId)
        {
            OverallStatsDTO empty = new OverallStatsDTO();
            var stats = await _stats.Aggregate()
                .Match(x => x.UserId == userId)
                .Group(x => x.UserId, g => new OverallStatsDTO
                {
                    DistanceTraveled = g.Sum(x => x.DistanceTraveled),
                    PointsCollected = g.Sum(x => x.PointsCollected)
                }).ToListAsync();

            return stats.FirstOrDefault() ?? empty;
        }

        public async Task<OverallStatsDTO> GetOverallStats()
        {
            OverallStatsDTO empty = new OverallStatsDTO();
            var stats = await _stats.Aggregate()
                .Group(x => true, g => new OverallStatsDTO
                {
                    DistanceTraveled = g.Sum(x => x.DistanceTraveled),
                    PointsCollected = g.Sum(x => x.PointsCollected)
                }).ToListAsync();

            return stats.FirstOrDefault() ?? empty;
        }

        private double CalculateDistanceFromLastPoint(RoadPointDTO point, string userId)
        {
            RoadPointDTO lastPoint = _cacheService.GetGeoPointFromCache(userId);
            
            if (lastPoint == null)
            {
                return 0;
            }

            var geoPoint1 = new Coordinate(lastPoint.Latitude, lastPoint.Longitude);
            var geoPoint2 = new Coordinate(point.Latitude, point.Longitude);

            var distance = GeoCalculator.GetDistance(geoPoint1, geoPoint2, 2, DistanceUnit.Meters);

            return distance;
        }

    }
}
