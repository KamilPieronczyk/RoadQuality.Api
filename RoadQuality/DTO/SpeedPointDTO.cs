using RoadQuality.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoadQuality.DTO
{
    public class SpeedPointDTO
    {
        public double Speed { get; set; }
        public GeoPoint Location { get; set; }
    }
}
