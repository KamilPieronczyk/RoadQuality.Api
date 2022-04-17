using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoadQuality.Models
{
    public class GeoPoint : IGeoPoint
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }
    }

    public interface IGeoPoint
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }
    }
}
