using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using viadflib.TravelTime.KDTree;
using viadflib.TravelTime.PublicTransportTimeAlgorithm;

namespace viadflib.TravelTime.Data
{
    public class ViaDfDataProvider : IPublicTransportTimeDataProvider
    {
        private readonly Dictionary<int, List<SearchIndex>> connectionCache;
        private readonly Dictionary<int, RoutePiece> routePieceCache;
        private readonly KdTree<RoutePiece> routePieceKdTree;

        public ViaDfDataProvider()
        {
            using (var context = new DataContext())
            {
                var options = new DataLoadOptions();
                options.LoadWith<RoutePiece>(x => x.Route);
                context.LoadOptions = options;

                var routePieces = context.RoutePieces.ToList();

                routePieceCache = routePieces.ToDictionary(x => x.ID, x => x);
                connectionCache = context.SearchIndexes.ToList().GroupBy(x => x.RoutePieceID).ToDictionary(x => x.Key, x => x.ToList());
                routePieceKdTree = new KdTree<RoutePiece>(routePieces, x => x.Lat, x => x.Lng);
            }
        }

        public List<PublicTransportConnection> GetInRange(LatLng position, double radius)
        {
            var result = new List<PublicTransportConnection>();

            var pieces = routePieceKdTree.FindInRange(new Vector(position.Lat, position.Lng), radius); // routePieceCache.Values.Where(x => x.Lat > bounds.Lower.Lat && x.Lat < bounds.Upper.Lat && x.Lng > bounds.Lower.Lng && x.Lng < bounds.Upper.Lng).ToList();

            foreach (var routePiece in pieces)
            {
                var walkingTimeMins = (position.DistanceInKmTo(new LatLng(routePiece.Lat, routePiece.Lng)) / PublicTransportTimeAlgorithm.PublicTransportTimeAlgorithm.WalkingKmh) * 60.0 + 6; // 6 mins - average time to wait for transport - TODO take from DB type / route frequency
                result.Add(new PublicTransportConnection
                {
                    LatLng = new LatLng(routePiece.Lat, routePiece.Lng),
                    Cost = walkingTimeMins,
                    ID = routePiece.ID,
                    RouteID = routePiece.RouteID,
                    Direction = routePiece.Route.SplitRoutePieceID.HasValue ? routePiece.Route.SplitRoutePieceID >= routePiece.ID : false,
                    Name = null
                });
            }

            return result;
        }

        public List<PublicTransportConnection> GetReachableByTransport(int id, double maxTime)
        {
            var piece = routePieceCache[id];

            var result = new List<PublicTransportConnection>();

            List<SearchIndex> connections;

            if (connectionCache.TryGetValue(id, out connections))
            {
                foreach (var connection in connections)
                {
                    if (connection.Cost < maxTime)
                    {
                        var routePiece = routePieceCache[connection.RoutePiece2ID];
                        string name = piece.RouteID == routePiece.RouteID ? routePiece.Route.Name : null;

                        result.Add(new PublicTransportConnection
                        {
                            LatLng = new LatLng(routePiece.Lat, routePiece.Lng),
                            Cost = connection.Cost + 0.00001, // FIX for 0 cost transitions
                            ID = routePiece.ID,
                            RouteID = routePiece.RouteID,
                            Direction = routePiece.Route.SplitRoutePieceID.HasValue ? routePiece.Route.SplitRoutePieceID >= routePiece.ID : false,
                            Name = name
                        });
                    }
                }
            }

            return result;
        }
    }
}
