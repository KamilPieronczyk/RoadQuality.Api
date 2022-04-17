using MongoDB.Bson;
using MongoDB.Entities;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver.GeoJsonObjectModel;

namespace RoadQuality.Models
{
    public class QualityPoint
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public double AvgVector { get; set; }
        public double AvgSpeed { get; set; }
        public List<PointDataObject> Data { get; set; }

        public GeoJsonPoint<GeoJson2DGeographicCoordinates> Location { get; set; }
    }

    public class PointDataObject
    {
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonId]
        public ObjectId Id { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }
        public double Speed { get; set; }
        public double Vector { get; set; }
        public DateTime Date { get; set; }

        public PointDataObject()
        {
            Id = ObjectId.GenerateNewId();
        }
    }

    public class QualityPointUnwind
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public double AvgVector { get; set; }
        public double AvgSpeed { get; set; }
        public PointDataObject Data { get; set; }

        public GeoJsonPoint<GeoJson2DGeographicCoordinates> Location { get; set; }
    }
}
