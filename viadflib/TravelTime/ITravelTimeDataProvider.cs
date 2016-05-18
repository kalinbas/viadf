using System.Collections.Generic;

namespace viadflib.TravelTime
{
    public interface ITravelTimeDataProvider
    {
        List<TravelTimeDataConnection> GetReachableByWalking(LatLng startPosition, double maxTime);

        List<TravelTimeDataConnection> GetReachableByWalkingOrTransport(int id, double maxTime);
    }
}
