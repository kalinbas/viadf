using System;
using System.Collections.Generic;
using System.Linq;
using viadflib.AStar;

namespace viadflib
{
    public class Searcher
    {
        private ViaDFGraph graph;
        private AStar.AStar aStar;
        private Dictionary<string, string> nameCache;
        private Dictionary<int, RoutePiece> routePieceCache;
        private Dictionary<int, Route> routeCache;

        public Searcher(ViaDFGraph g)
        {
            graph = g;
            aStar = new AStar.AStar(graph);
            nameCache = new Dictionary<string, string>();
            routePieceCache = new Dictionary<int, RoutePiece>();
            routeCache = new Dictionary<int, Route>();
        }

        private string GetPieceName(ResultRoutePiece piece)
        {            
            return GetNameCached(piece.Lat, piece.Lng);
        }

        private string GetNameCached(double lat, double lng)
        {
            string key = lat + "_" + lng;
            if (!nameCache.ContainsKey(key))
            {
                nameCache[key] = DataHandler.GetNameAtPosition(lat, lng);
            }
            return nameCache[key];
        }

        private RoutePiece GetRoutePieceCached(int id, DataContext context)
        {
            if (!routePieceCache.ContainsKey(id))
            {
                routePieceCache[id] = context.RoutePieces.FirstOrDefault(x => x.ID == id);
            }
            return routePieceCache[id];
        }

        private Route GetRouteCached(int id, DataContext context)
        {
            if (!routeCache.ContainsKey(id))
            {
                routeCache[id] = context.Routes.FirstOrDefault(x => x.ID == id);
            }
            return routeCache[id];
        }

        public List<SearchResult> DoSearch(SearchParams searchParams)
        {
            List<SearchResult> results = new List<SearchResult>();

            var startNode = new Node(searchParams.StartSearch.Lat, searchParams.StartSearch.Lng);
            var endNode = new Node(searchParams.EndSearch.Lat, searchParams.EndSearch.Lng);

            graph.CreateLinksForTemporaryNode(startNode, true, 100);
            graph.CreateLinksForTemporaryNode(endNode, false, 100);

            graph.CreateDirectLink(startNode, endNode, 15);

            aStar.StartNode = startNode;
            aStar.EndNode = endNode;

            bool foundResult = aStar.SearchPath(searchParams.NrOfResults);

            if (aStar.PathFound)
            {
                using (var context = new DataContext())
                {
                    var metroIds = context.Routes.Where(x => x.TypeID == (int)TypeEnum.Metro && x.Status != (int)StatusEnum.New).Select(x => x.ID).ToList();

                    var dlo = new System.Data.Linq.DataLoadOptions();
                    dlo.LoadWith<RoutePiece>(x => x.Route);
                    dlo.LoadWith<Route>(x => x.Type);
                    context.LoadOptions = dlo;

                    Dictionary<int, RoutePiece> routePieceCache = new Dictionary<int, RoutePiece>();
                    Dictionary<int, Route> routeCache = new Dictionary<int, Route>();

                    for (int si = 0; si < aStar.NrOfFoundResults; si++)
                    {
                        SearchResult result = new SearchResult();
                        result.Start = searchParams.StartSearch;
                        result.End = searchParams.EndSearch;

                        result.Items = new List<SearchResultItem>();

                        DateTime currentTime = searchParams.StartTime ?? DateTime.Now;
                        result.StartTravelTime = currentTime;

                        var arcs = aStar.GetPathByArcs(si);

                        double accumulatedTime = 0;
                        double accumulatedDistance = 0;
                        Node accumulatedStartNode = null;
                        List<SearchPosition> accumulatedPath = new List<SearchPosition>();

                        for (int i = 0; i < arcs.Length; i++)
                        {
                            Arc arc = arcs[i];

                            if (arc.StartNode.RouteID != arc.EndNode.RouteID || arcs.Length == 1)
                            {
                                if (accumulatedTime > 0)
                                {
                                    SearchResultItem item = new SearchResultItem();

                                    Route route = GetRouteCached(arc.StartNode.RouteID, context);
                                    item.Route = ResultRoute.FromRoute(route);
                                    item.Type = ResultType.FromType(route.Type);
                                    item.Start = ResultRoutePiece.FromRoutePiece(GetRoutePieceCached(accumulatedStartNode.RoutePieceID, context));
                                    item.StartName = GetPieceName(item.Start);                                  
                                    item.End = ResultRoutePiece.FromRoutePiece(GetRoutePieceCached(arc.StartNode.RoutePieceID, context));
                                    item.EndName = GetPieceName(item.End);
                                    item.Time = accumulatedTime;
                                    item.Distance = accumulatedDistance;

                                    item.InDirection = ((!item.Route.SplitRoutePieceID.HasValue && item.Start.ID < item.End.ID) || (item.Route.SplitRoutePieceID.HasValue && item.Start.ID < item.Route.SplitRoutePieceID.Value)) ? item.Route.ToName : item.Route.FromName;
                                    
                                    // if special price type and last of same type
                                    Route nextRoute = GetRouteCached(arc.EndNode.RouteID, context);
                                    if (route.Type.PriceTypeID != (int)PriceTypeEnum.FixedPriceAllLines || nextRoute == null || nextRoute.TypeID != route.TypeID)
                                    {
                                        item.Price = GetPrice(route, route.Type, item.Distance);
                                    }
                                    else
                                    {
                                        item.Price = 0; // price will be applied later
                                    }

                                    item.StartTravelTime = currentTime;
                                    currentTime = currentTime.AddMinutes(item.Time);
                                    item.EndTravelTime = currentTime;

                                    accumulatedPath.Add(new SearchPosition(arc.StartNode.X, arc.StartNode.Y, ""));
                                    item.Path = accumulatedPath;

                                    accumulatedTime = 0;
                                    accumulatedDistance = 0;
                                    accumulatedPath = new List<SearchPosition>();
                                    result.Items.Add(item);
                                }
                                SearchResultItem item2 = new SearchResultItem();
                                item2.Type = ResultType.FromType(ViaDFGraph.WALKING_TYPE);
                                item2.Start = ResultRoutePiece.FromRoutePiece(arc.StartNode.RoutePieceID > 0 ? GetRoutePieceCached(arc.StartNode.RoutePieceID, context) : new RoutePiece { Lat = result.Start.Lat, Lng = result.Start.Lng, Name = result.Start.Name });
                                item2.StartName = GetPieceName(item2.Start);
                                item2.End = ResultRoutePiece.FromRoutePiece(arc.EndNode.RoutePieceID > 0 ? GetRoutePieceCached(arc.EndNode.RoutePieceID, context) : new RoutePiece { Lat = result.End.Lat, Lng = result.End.Lng, Name = result.End.Name });
                                item2.EndName = GetPieceName(item2.End);
                                item2.Time = arc.Cost;
                                item2.Distance = graph.GetArcDistanceInKm(arc);
                                item2.StartTravelTime = currentTime;
                                currentTime = currentTime.AddMinutes(item2.Time);
                                item2.EndTravelTime = currentTime;
                                item2.Path.Add(new SearchPosition(arc.StartNode.X, arc.StartNode.Y, ""));
                                item2.Path.Add(new SearchPosition(arc.EndNode.X, arc.EndNode.Y, ""));
                                result.Items.Add(item2);

                                accumulatedStartNode = arc.EndNode;
                            }
                            else
                            {
                                accumulatedTime += arc.Cost;
                                accumulatedDistance += graph.GetArcDistanceInKm(arc);
                                accumulatedPath.Add(new SearchPosition(arc.StartNode.X, arc.StartNode.Y, ""));
                            }
                        }

                        result.EndTravelTime = currentTime;
                        result.TotalDistance = result.Items.Sum(x => x.Distance);
                        result.TotalTime = result.Items.Sum(x => x.Time);
                        result.TotalPrice = result.Items.Sum(x => x.Price);

                        results.Add(result);
                    }
                }
            }

            return results.OrderBy(x => x.TotalTime).ToList();
        }

        public double GetPrice(Route route, Type type, double distance)
        {
            PriceTypeEnum priceType = (PriceTypeEnum)(route.PriceTypeID ?? type.PriceTypeID);
            string priceDefinition = route.PriceTypeID.HasValue ? route.PriceDefinition : type.PriceDefinition;

            switch (priceType)
            {
                case PriceTypeEnum.FixedPrice:
                    return double.Parse(priceDefinition);
                case PriceTypeEnum.VariablePrice:
                    var ranges = priceDefinition.Split('|');
                    for (int i = 0; i < ranges.Length; i++)
                    {
                        var prices = ranges[i].Split(';');
                        if (prices.Length < 2 || double.Parse(prices[1]) > distance)
                        {
                            return double.Parse(prices[0]);
                        }
                    }
                    break;
                case PriceTypeEnum.FixedPriceAllLines:
                    return double.Parse(priceDefinition);
                case PriceTypeEnum.Free:
                    return 0;                    
            }

            return 0;           
        }
    }
}
