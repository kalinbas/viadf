using System;
using System.Collections.Generic;
using System.Linq;

namespace viadflib.TravelTime
{
    public class TravelTimeAlgorithm
    {
        private class TravelTimeAlgorithmItem : IComparable
        {
            public TravelTimeDataConnection Stop { get; set; }
            public TravelTimeDataConnection PreviousStop { get; set; }
            public double TotalCost { get; set; }

            public int CompareTo(object obj)
            {
                var otherTrack = (TravelTimeAlgorithmItem)obj;
                return otherTrack.TotalCost.CompareTo(TotalCost);
            }
        }

        private readonly ITravelTimeDataProvider _dataProvider;

        public TravelTimeAlgorithm(ITravelTimeDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        public TravelTimeResult Calculcate(LatLng startPosition, double maxTime)
        {
            var stopQueue = new SimplePriorityQueue<TravelTimeAlgorithmItem>();
            var stopList = new Dictionary<int, TravelTimeAlgorithmItem>();

            const int initialWait = 3; //TODO analyse

            List<TravelTimeDataConnection> stops = _dataProvider.GetReachableByWalking(startPosition, maxTime);
            foreach (var stop in stops)
            {
                var stopItem = new TravelTimeAlgorithmItem() { Stop = stop, TotalCost = stop.Cost + initialWait };
                stopQueue.Enqueue(stopItem, stopItem.TotalCost);
                stopList[stop.ID] = stopItem;
            }

            while (stopQueue.Count > 0)
            {
                var stop = stopQueue.Dequeue();
                if (stop.TotalCost >= maxTime)
                {
                    break;
                }

                List<TravelTimeDataConnection> reachableStops = _dataProvider.GetReachableByWalkingOrTransport(stop.Stop.ID, maxTime - stop.TotalCost);
                foreach (var reachableStop in reachableStops)
                {
                    var existingStop = stopList.ContainsKey(reachableStop.ID) ? stopList[reachableStop.ID] : null;
                    double newTotalCost = stop.TotalCost + reachableStop.Cost;

                    if (existingStop == null)
                    {
                        // add it - if it was not yet in list
                        var newStop = new TravelTimeAlgorithmItem { PreviousStop = stop.Stop, Stop = reachableStop, TotalCost = newTotalCost };
                        stopQueue.Enqueue(newStop, newStop.TotalCost);
                        stopList[reachableStop.ID] = newStop;
                    }
                    else
                    {
                        // update it - if cost smaller
                        if (newTotalCost < existingStop.TotalCost)
                        {
                            stopQueue.Remove(existingStop, existingStop.TotalCost);
                            existingStop.TotalCost = newTotalCost;
                            existingStop.PreviousStop = stop.Stop;
                            stopQueue.Enqueue(existingStop, existingStop.TotalCost);
                        }
                    }
                }
            }

            TravelTimeResult result = new TravelTimeResult();
            result.Start = new TravelTimeResultItem() {LatLng = startPosition, TimePassed = 0};
            foreach (var stop in stopList.Values.OrderBy(x => x.TotalCost))
            {
                result.Items.Add(new TravelTimeResultItem { LatLng = stop.Stop.LatLng, FromLatLng = stop.PreviousStop == null ? null : stop.PreviousStop.LatLng, TimePassed = stop.TotalCost, TravelType = stop.PreviousStop == null ? null : stop.PreviousStop.Name });
            }

            return result;
        }

    }
}
