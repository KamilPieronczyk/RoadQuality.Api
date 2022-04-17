using RoadQuality.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoadQuality.DTO
{
    public class QualityPointDTO
    {
        public double Vector { get; set; }
        public double Speed { get; set; }
        public GeoPoint Location { get; set; }
    }
}
