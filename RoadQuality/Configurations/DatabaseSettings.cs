using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoadQuality.Configurations
{
    public class DatabaseSettings : IDatabaseSettings
    {
        public string UsersCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string RoadQualityCollectionName { get; set; }
        public string RoadSpeedCollectionName { get; set; }
        public string UserStatisticsCollectionName { get; set; }
        public string OsrmAdrress { get; set; }
    }

    public interface IDatabaseSettings
    {
        string UsersCollectionName { get; set; }
        string RoadQualityCollectionName { get; set; }
        string RoadSpeedCollectionName { get; set; }
        string UserStatisticsCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
        string OsrmAdrress { get; set; }
    }
}
