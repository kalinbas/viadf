using System;
using System.Collections.Generic;
using System.Linq;

namespace viadflib.TravelTime.PublicTransportTimeAlgorithm
{
    public class PublicTransportTimeAlgorithm : ITravelTimeAlgorithm
    {
        public const double WalkingKmh = 5.0;

        private class TravelTimeAlgorithmItem : IComparable
        {
            public PublicTransportConnection Stop { get; set; }
            public TravelTimeAlgorithmItem PreviousItem { get; set; }
            public double TotalCost { get; set; }

            public int CompareTo(object obj)
            {
                var otherTrack = (TravelTimeAlgorithmItem)obj;
                return otherTrack.TotalCost.CompareTo(TotalCost);
            }
        }

        private readonly IPublicTransportTimeDataProvider _dataProvider;

        public PublicTransportTimeAlgorithm(IPublicTransportTimeDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        public TravelTimePolygon GetPolygon(LatLng startPosition, double maxTime)
        {
            var result = Calculate(startPosition, maxTime);

            // reduce biggest circles
            ReduceResult(result, result.Items.Count);

            TravelTimePolygon polygon = new TravelTimePolygon();
            polygon.Start = startPosition;
            polygon.Time = maxTime;
            polygon.Paths = Utils.PolygonHelper.CreatePolygonPaths(result.Items, WalkingKmh);
            //polygon.Paths = Utils.PolygonHelper.CreatePolygonPathUsingBitmapArray(result.Items, WalkingKmh, 500);
            return polygon;
        }
    
        private void ReduceResult(PublicTransportResult result, int count)
        {
            // find intersection bubbles
            for (int i = 0; i < Math.Min(count, result.Items.Count); i++)
            {
                var radiusKmBig = WalkingKmh * result.Items[i].TimeLeft / 60;
                for (int j = i + 1; j < result.Items.Count; j++)
                {
                    var radiusKm = WalkingKmh * result.Items[j].TimeLeft / 60;
                    if ((radiusKmBig - radiusKm) * (radiusKmBig - radiusKm) > result.Items[j].Center.DistanceInKmSquaredTo(result.Items[i].Center))
                    {
                        result.Items.RemoveAt(j);
                        j--;
                    }
                }
            }
        }

        private PublicTransportResult Calculate(LatLng startPosition, double maxTime, List<TravelTimeListPoint> points = null)
        {
            var stopQueue = new PriorityQueue<TravelTimeAlgorithmItem>();
            var stopList = new Dictionary<int, TravelTimeAlgorithmItem>();


            // start search
            var startItem = new TravelTimeAlgorithmItem { Stop = new PublicTransportConnection() { LatLng = startPosition, Cost = 0, ID = 0 }, TotalCost = 0 };
            stopQueue.Enqueue(startItem, 0);
            stopList[0] = startItem;

            while (stopQueue.Count > 0)
            {
                var stop = stopQueue.Dequeue();
                if (stop.TotalCost >= maxTime)
                {
                    break;
                }

                double timeLeft = maxTime - stop.TotalCost;

                List<PublicTransportConnection> reachableStops;
                if (stop.Stop.ID > 0)
                {
                    reachableStops = _dataProvider.GetReachableByTransport(stop.Stop.ID, timeLeft);
                }
                else
                {
                    // calculate approximate radius - its not exact as lat/lng circle cant be a perfect circle
                    double radius = LatLng.GetWalkingDistanceLatLng(startPosition.Lat, maxTime, WalkingKmh);
                    reachableStops = _dataProvider.GetInRange(startPosition, radius);
                }

                foreach (var reachableStop in reachableStops)
                {
                    double newTotalCost = stop.TotalCost + reachableStop.Cost;

                    TravelTimeAlgorithmItem existingStop;
                    if (stopList.TryGetValue(reachableStop.ID, out existingStop))
                    {
                        // update it - if cost smaller
                        if (newTotalCost < existingStop.TotalCost)
                        {
                            stopQueue.Remove(existingStop, existingStop.TotalCost);

                            existingStop.PreviousItem = stop;
                            existingStop.TotalCost = newTotalCost;

                            stopQueue.Enqueue(existingStop, existingStop.TotalCost);
                        }
                    }
                    else
                    {   
                        if (newTotalCost < maxTime)
                        {
                            // add it - its new
                            var newStop = new TravelTimeAlgorithmItem
                            {
                                PreviousItem = stop,
                                Stop = reachableStop,
                                TotalCost = newTotalCost
                            };

                            stopQueue.Enqueue(newStop, newStop.TotalCost);
                            stopList[reachableStop.ID] = newStop;
                        }
                    }
                }
            }

            PublicTransportResult result = new PublicTransportResult();
            foreach (var stop in stopList.Values.OrderBy(x => x.TotalCost))
            {
                result.Items.Add(new TravelTimeCircle { Center = stop.Stop.LatLng, TimeLeft = maxTime - stop.TotalCost });

                // check if this point is closest to stop
                if (points != null) {
                    points.ForEach(p =>
                    {
                        var time = 60 * p.Coords.DistanceInKmTo(stop.Stop.LatLng) / WalkingKmh + stop.TotalCost;
                        if (p.Time > time)
                        {
                            p.Time = time;
                        }
                    });
                }
            }
            return result;
        }

        public List<TravelTimeListPoint> GetOrderedList(LatLng start, List<LatLng> points, int maxTime)
        {
            var list = points.Select(x => new TravelTimeListPoint() { Coords = x, Time = Double.MaxValue }).ToList();
            Calculate(start, maxTime, list);
            return list.OrderBy(x => x.Time).ToList();
        }
    }
}
