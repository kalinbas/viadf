using System.Collections.Generic;

namespace viadf.Models
{
    public class MapResultModel
    {
        public List<MapResultModelItem> Stops { get; set; }
    }

    public class MapResultModelItem
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
        public double Radius { get; set; }

        public double? PrevLat { get; set; }
        public double? PrevLng { get; set; }

        public string Name { get; set; }
    }
}