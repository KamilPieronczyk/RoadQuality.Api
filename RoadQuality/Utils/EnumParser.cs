using RoadQuality.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoadQuality.Utils
{
    public class EnumParser<EnumType> where EnumType : struct, IConvertible
    {
        public EnumType Parse(string pointType, EnumType defaultVal)
        {
            if (pointType == null || pointType.Length == 0)
            {
                return defaultVal;
            }
            EnumType parsedPointType;
            if (Enum.TryParse<EnumType>(pointType, true, out parsedPointType))
            {
                return parsedPointType;
            }
            return defaultVal;
        }
    }
}
