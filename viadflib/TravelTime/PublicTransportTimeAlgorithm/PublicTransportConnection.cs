using System;

namespace viadflib.TravelTime.PublicTransportTimeAlgorithm
{
    public class PublicTransportConnection
    {
        public int ID { get; set; }
        public int RouteID { get; set; }
        public bool Direction { get; set; }
        public double Cost { get; set; }
        public LatLng LatLng { get; set; }
        public string Name { get; set; }
    }
}
