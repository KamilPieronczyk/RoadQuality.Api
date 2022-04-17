using RoadQuality.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoadQuality.DTO
{
    public class OverallStatsDTO : IOverallStatistics
    {
        public int PointsCollected { get; set; } = 0;
        public double DistanceTraveled { get; set; } = 0;
    }
}
