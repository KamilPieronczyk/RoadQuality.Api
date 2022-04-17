using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoadQuality.Models
{
    public class FilterQuery : IFilterQuery
    {
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public bool? OnlyLoggedUserData { get; set; }
    }
    public interface IFilterQuery
    {
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public bool? OnlyLoggedUserData { get; set; }
    }
}
