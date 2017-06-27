using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace viadflib.TravelTime
{
    internal interface ITravelTimeAlgorithm
    {
        TravelTimePolygon GetPolygon(LatLng startPosition, double maxTime);
        List<TravelTimeListPoint> GetOrderedList(LatLng start, List<LatLng> points, int maxTime);
    }
}
