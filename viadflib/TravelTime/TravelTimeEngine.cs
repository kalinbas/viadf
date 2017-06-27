using System;
using System.Collections.Generic;
using viadflib.TravelTime.Data;

namespace viadflib.TravelTime
{
    public class TravelTimeEngine
    {
        private Dictionary<TravelTimeType, ITravelTimeAlgorithm> _algorithms;

        public TravelTimeEngine()
        {
            _algorithms = new Dictionary<TravelTimeType, ITravelTimeAlgorithm>();
            _algorithms[TravelTimeType.PublicTransport] = new PublicTransportTimeAlgorithm.PublicTransportTimeAlgorithm(new ViaDfDataProvider());
            _algorithms[TravelTimeType.Walking] = new PublicTransportTimeAlgorithm.PublicTransportTimeAlgorithm(new WalkingDataProvider());
        }

     
        /// <summary>
        /// Calculates a Travel Time Polygon given a start position and maximal travel time
        /// </summary>
        /// <param name="type"></param>
        /// <param name="startPosition"></param>
        /// <param name="maxTime"></param>
        /// <returns></returns>
        public TravelTimePolygon GetPolygon(TravelTimeType type, LatLng startPosition, double maxTime)
        {
            return _algorithms[type].GetPolygon(startPosition, maxTime);
        }

        /// <summary>
        /// Order points by distance
        /// </summary>
        /// <param name="type"></param>
        /// <param name="startPosition"></param>
        /// <param name="maxTime"></param>
        /// <returns></returns>
        public TravelTimeList GetOrderPointList(TravelTimeType type, LatLng start, List<LatLng> points)
        {
            TravelTimeList list = new TravelTimeList();
            list.Start = start;
            list.Points = _algorithms[type].GetOrderedList(start, points, int.MaxValue);
            return list;
        }
    }
}
