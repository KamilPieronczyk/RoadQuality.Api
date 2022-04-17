using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoadQuality.DTO
{
    public class DateRangeDTO : IDateRangeDTO
    {
        public DateTime? Start { get; set; }
        public DateTime End { get; set; }

        public DateRangeDTO() {
            End = DateTime.Now;
        }
    }

    public interface IDateRangeDTO
    {
        public DateTime? Start { get; set; }
        public DateTime End { get; set; }
    }
}
