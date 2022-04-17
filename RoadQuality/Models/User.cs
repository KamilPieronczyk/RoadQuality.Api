using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace RoadQuality.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Email { get; set; }

        public string Name { get; set; }

        public string LastName { get; set; }

        public string PhoneNumber { get; set; }

        [BsonIgnore]
        public string JWTToken { get; set; }

        public bool IsProfileSet { get; set; }
    }
}
