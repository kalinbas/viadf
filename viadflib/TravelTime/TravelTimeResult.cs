using System.Collections.Generic;

namespace viadflib.TravelTime
{
    public class TravelTimeResult
    {
        public TravelTimeResult()
        {
            Items = new List<TravelTimeResultItem>();
        }

        public TravelTimeResultItem Start { get; set; }
        public List<TravelTimeResultItem> Items { get; set; }
    }

    public class TravelTimeResultItem
    {
        public LatLng LatLng { get; set; }
        public double TimePassed { get; set; }

        public LatLng FromLatLng { get; set; }
        public string TravelType { get; set; }
    }
}
