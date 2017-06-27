using System;
using System.Collections.Generic;

namespace viadflib.TravelTime.PublicTransportTimeAlgorithm
{
    public interface IPublicTransportTimeDataProvider
    {
        List<PublicTransportConnection> GetInRange(LatLng position, double radius);
        List<PublicTransportConnection> GetReachableByTransport(int id, double maxTime);
    }
}
