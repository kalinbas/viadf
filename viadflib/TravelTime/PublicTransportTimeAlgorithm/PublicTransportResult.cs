using System.Collections.Generic;

namespace viadflib.TravelTime.PublicTransportTimeAlgorithm
{
    public class PublicTransportResult
    {
        public PublicTransportResult()
        {
            Items = new List<TravelTimeCircle>();
        }

        public List<TravelTimeCircle> Items { get; set; }
    }
}
