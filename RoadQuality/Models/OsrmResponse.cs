using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoadQuality.Models
{
    public class OsrmResponse
    {
        public string Code { get; set; }
        public List<OsrmResponseElement> Waypoints { get; set; }
    }

    public class OsrmResponseElement
    {
        public string Name { get; set; }
        public List<string> Location { get; set; }
    }
}
