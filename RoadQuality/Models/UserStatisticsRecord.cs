using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoadQuality.Models
{
    public class UserStatisticsRecord : IOverallStatistics
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public int PointsCollected { get; set; }
        public double DistanceTraveled { get; set; }
        public DateTime Date { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }
    }
    public interface IOverallStatistics
    {
        int PointsCollected { get; set; }
        double DistanceTraveled { get; set; }
    }
}
