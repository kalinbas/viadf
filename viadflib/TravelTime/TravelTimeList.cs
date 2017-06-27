using System.Collections.Generic;

namespace viadflib.TravelTime
{
    public class TravelTimeList
    {
        public TravelTimeList()
        {
            Points = new List<TravelTimeListPoint>();
        }        
        public LatLng Start { get; set; }

        public List<TravelTimeListPoint> Points { get; set; }
    }

    public class TravelTimeListPoint
    {
        public TravelTimeListPoint()
        {
        }
        public LatLng Coords { get; set; }

        public double Time { get; set; }
    }
}
