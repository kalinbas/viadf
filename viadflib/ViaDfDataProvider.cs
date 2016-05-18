using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using viadflib.TravelTime;

namespace viadflib
{
    public class ViaDfDataProvider : ITravelTimeDataProvider
    {
        private const double WalkingKmh =  5.0;

        private readonly List<LatLngBounds> alreadySearchedBounds;
        private readonly List<SearchIndex> searchIndexCache;
        private readonly List<RoutePiece> routePieceCache;

        public ViaDfDataProvider(LatLng startPosition)
        {
            alreadySearchedBounds = new List<LatLngBounds>();
            using (var context = new DataContext())
            {
                var options = new DataLoadOptions();
                options.LoadWith<RoutePiece>(x => x.Route);
                context.LoadOptions = options;

                var bounds = LatLngBounds.FromWalkingDistance(startPosition, 60, WalkingKmh);
                searchIndexCache = context.SearchIndexes.ToList();
                routePieceCache = context.RoutePieces.Where(x => x.Lat > bounds.Lower.Lat && x.Lat < bounds.Upper.Lat && x.Lng > bounds.Lower.Lng && x.Lng < bounds.Upper.Lng).ToList();
            }
        }

        public List<TravelTimeDataConnection> GetReachableByWalking(LatLng startPosition, double maxTime)
        {
            var result = new List<TravelTimeDataConnection>();

            var bounds = LatLngBounds.FromWalkingDistance(startPosition, maxTime, WalkingKmh);

            // skip this search - already found all those elements
            if (alreadySearchedBounds.Any(x => x.Contains(bounds)))
            {
                return result;
            }

            alreadySearchedBounds.Add(bounds);

            var pieces = routePieceCache.Where(
                    x =>
                    x.Lat > bounds.Lower.Lat && x.Lat < bounds.Upper.Lat && x.Lng > bounds.Lower.Lng &&
                    x.Lng < bounds.Upper.Lng).ToList();


            foreach (var routePiece in pieces)
            {
                var walkingTimeMins = (startPosition.DistanceInKmTo(new LatLng(routePiece.Lat, routePiece.Lng)) / WalkingKmh) * 60.0;
                if (walkingTimeMins < maxTime)
                {
                    result.Add(new TravelTimeDataConnection
                    {
                        LatLng = new LatLng(routePiece.Lat, routePiece.Lng),
                        Cost = walkingTimeMins,
                        ID = routePiece.ID,
                        Name = null
                    });
                }
            }

            return result;
        }

        public List<TravelTimeDataConnection> GetReachableByWalkingOrTransport(int id, double maxTime)
        {
            var piece = routePieceCache.First(x => x.ID == id);

            var result = GetReachableByWalking(new LatLng(piece.Lat, piece.Lng), maxTime);

            var connections = searchIndexCache.Where(x => x.RoutePieceID == id && x.Cost < maxTime);

            foreach (var connection in connections)
            {
                var routePiece = routePieceCache.First(x => x.ID == connection.RoutePiece2ID);
                string name = piece.RouteID == routePiece.RouteID ? routePiece.Route.Name : null;

                result.Add(new TravelTimeDataConnection { LatLng = new LatLng(routePiece.Lat, routePiece.Lng), Cost = connection.Cost, ID = routePiece.ID, Name = name });
            }

            return result;
        }
    }
}
