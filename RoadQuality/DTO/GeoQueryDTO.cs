using RoadQuality.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoadQuality.DTO
{
    public class GeoQueryDTO : IGeoPoint, IFilterQuery
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public double? Radius { get; set; }
        public bool? OnlyLoggedUserData { get; set; }
    }
}
