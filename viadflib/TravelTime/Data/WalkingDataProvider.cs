using System;
using System.Collections.Generic;
using viadflib.TravelTime.PublicTransportTimeAlgorithm;

namespace viadflib.TravelTime.Data
{
    internal class WalkingDataProvider : IPublicTransportTimeDataProvider
    {
        public List<PublicTransportConnection> GetInRange(LatLng position, double radius)
        {
            return new List<PublicTransportConnection>();
        }

        public List<PublicTransportConnection> GetReachableByTransport(int id, double maxTime)
        {
            return new List<PublicTransportConnection>();
        }
    }
}
