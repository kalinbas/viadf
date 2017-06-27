using System.Collections.Generic;

namespace viadflib.TravelTime
{
    public class TravelTimePolygon
    {
        public TravelTimePolygon()
        {
            Paths = new List<TravelTimePolygonPath>();
        }
        
        public LatLng Start { get; set; }
        public double Time { get; set; }

        public List<TravelTimePolygonPath> Paths { get; set; }
    }

    public class TravelTimePolygonPath
    {
        public TravelTimePolygonPath()
        {
            Coords = new List<LatLng>();
        }

        public List<LatLng> Coords { get; set; }
    }
}
