using System;
using System.Collections.Generic;
using System.Linq;

namespace viadflib
{
    public class Indexer
    {
        public static void ClearIndex()
        {
            DataHandler.ClearIndex();
        }

        public static void RebuildIndex(int timeoutSeconds)
        {
            DateTime start = DateTime.Now;

            List<Route> routesToIndex = null;
            List<Route> allIndexedRoutes = null;

            using (DataContext context = new DataContext())
            {
                var dlo = new System.Data.Linq.DataLoadOptions();
                dlo.LoadWith<Route>(x => x.RoutePieces);
                dlo.LoadWith<Route>(x => x.Type);
                context.LoadOptions = dlo;

                routesToIndex = context.Routes.Where(x => x.Status == (int)StatusEnum.Active).ToList();
                allIndexedRoutes = context.Routes.Where(x => x.Status == (int)StatusEnum.ActiveAndIndexed).ToList();
            }


            foreach (var route in routesToIndex)
            {
                // add this route to index
                AddRouteToIndex(route, allIndexedRoutes);

                // add to indexed list
                route.Status = (int)StatusEnum.ActiveAndIndexed;              
                allIndexedRoutes.Add(route);

                // save status to DB
                using (DataContext context = new DataContext())
                {
                    var dbRoute = context.Routes.First(x => x.ID == route.ID);
                    dbRoute.Status = (int)StatusEnum.ActiveAndIndexed;
                    context.SubmitChanges();
                }

                Console.Write(".");

                if (DateTime.Now.Subtract(start).TotalSeconds > timeoutSeconds)
                {
                    break;
                }
            }
        }

        public static List<Route> AddRouteToIndex(Route route, List<Route> allIndexedRoutes = null)
        {
            if (route.RoutePieces.Count == 0)
            {
                return allIndexedRoutes;
            }

            // create all connections inside route
            List<SearchIndex> indexList = new List<SearchIndex>();

            bool bidirectional = !route.SplitRoutePieceID.HasValue;

            RoutePiece previous = null;
            RoutePiece first = null;
            foreach (var actual in route.RoutePieces)
            {
                if (first == null)
                {
                    first = actual;
                }
                if (previous != null)
                {
                    double cost = CalculateTransportTime(previous.Lat, previous.Lng, actual.Lat, actual.Lng, route, route.Type);
                    indexList.Add(new SearchIndex { RoutePieceID = previous.ID, RoutePiece2ID = actual.ID, Cost = cost });
                    if (bidirectional)
                    {
                        indexList.Add(new SearchIndex { RoutePieceID = actual.ID, RoutePiece2ID = previous.ID, Cost = cost });
                    }
                }
                previous = actual;
            }
            if (!bidirectional && previous != null && first != null)
            {
                indexList.Add(new SearchIndex { RoutePieceID = previous.ID, RoutePiece2ID = first.ID, Cost = CalculateTransportTime(previous.Lat, previous.Lng, first.Lat, first.Lng, route, route.Type) });
            }


            var nodes = route.RoutePieces.ToList();

            foreach (var route2 in allIndexedRoutes)
            {
                if (route2.ID == route.ID)
                {
                    continue;
                }

                var nodes2 = route2.RoutePieces.ToList();

                if (nodes2.Count > 0)
                {

                    var bb = new BoundingBox(nodes2.Min(x => x.Lat) - ViaDFGraph.HALF_KM_IN_DEGREES, nodes2.Max(x => x.Lat) + ViaDFGraph.HALF_KM_IN_DEGREES, nodes2.Min(x => x.Lng) - ViaDFGraph.HALF_KM_IN_DEGREES, nodes2.Max(x => x.Lng) + ViaDFGraph.HALF_KM_IN_DEGREES);

                    bool intersectAtAll = nodes.Exists(x => bb.Contains(x.Lat, x.Lng));

                    bool outsideBefore = true;
                    bool outsideAfter = true;
                    bool changeLastPiece = false;
                    RoutePiece lastNode = null;
                    RoutePiece lastNode2 = null;

                    if (intersectAtAll)
                    {
                        foreach (var node in nodes)
                        {
                            RoutePiece node2 = null;
                            if (bb.Contains(node.Lat, node.Lng))
                            {
                                node2 = nodes2.Aggregate((curmin, x) => (curmin == null || Math.Abs(x.Lat - node.Lat) + Math.Abs(x.Lng - node.Lng) < Math.Abs(curmin.Lat - node.Lat) + Math.Abs(curmin.Lng - node.Lng) ? x : curmin));
                                double minDistance = Math.Abs(node2.Lat - node.Lat) + Math.Abs(node2.Lng - node.Lng);
                                outsideAfter = (minDistance > ViaDFGraph.HALF_KM_IN_DEGREES);
                            }
                            else
                            {
                                outsideAfter = true;
                            }

                            if (outsideAfter != outsideBefore)
                            {
                                if (outsideAfter)
                                {
                                    if (!changeLastPiece)
                                    {
                                        double cost = CalculateChangeTransportTime(lastNode.Lat, lastNode.Lng, lastNode2.Lat, lastNode2.Lng, route2, route2.Type);
                                        indexList.Add(new SearchIndex { RoutePieceID = lastNode.ID, RoutePiece2ID = lastNode2.ID, Cost = cost });
                                        cost = CalculateChangeTransportTime(lastNode2.Lat, lastNode2.Lng, lastNode.Lat, lastNode.Lng, route, route.Type);
                                        indexList.Add(new SearchIndex { RoutePieceID = lastNode2.ID, RoutePiece2ID = lastNode.ID, Cost = cost });
                                    }
                                }
                                else
                                {
                                    double cost = CalculateChangeTransportTime(node.Lat, node.Lng, node2.Lat, node2.Lng, route2, route2.Type);
                                    indexList.Add(new SearchIndex { RoutePieceID = node.ID, RoutePiece2ID = node2.ID, Cost = cost });
                                    cost = CalculateChangeTransportTime(node2.Lat, node2.Lng, node.Lat, node.Lng, route, route.Type);
                                    indexList.Add(new SearchIndex { RoutePieceID = node2.ID, RoutePiece2ID = node.ID, Cost = cost });
                                }
                                changeLastPiece = true;
                            }
                            else
                            {
                                // handle special case when its a two-way way route & last element
                                if (!outsideAfter && !route.SplitRoutePieceID.HasValue && node == nodes.Last())
                                {
                                    double cost = CalculateChangeTransportTime(node.Lat, node.Lng, node2.Lat, node2.Lng, route2, route2.Type);
                                    indexList.Add(new SearchIndex { RoutePieceID = node.ID, RoutePiece2ID = node2.ID, Cost = cost });
                                    cost = CalculateChangeTransportTime(node2.Lat, node2.Lng, node.Lat, node.Lng, route, route.Type);
                                    indexList.Add(new SearchIndex { RoutePieceID = node2.ID, RoutePiece2ID = node.ID, Cost = cost });
                                }
                                else
                                {
                                    changeLastPiece = false;
                                }
                            }
                            outsideBefore = outsideAfter;
                            lastNode = node;
                            lastNode2 = node2;
                        }
                    }
                }
            }

            using (DataContext context = new DataContext())
            {
                // save index
                context.SearchIndexes.InsertAllOnSubmit(indexList);
                context.SubmitChanges();
            }

            return allIndexedRoutes;
        }


        public static double CalculateTransportTime(double lat, double lng, double lat2, double lng2, Route route, Type type)
        {
            return 60 * ((Math.Sqrt((lat - lat2) * (lat - lat2) + (lng - lng2) * (lng - lng2)) / ViaDFGraph.KM_IN_DEGREES) / (route != null && route.AverageSpeed.HasValue ? route.AverageSpeed.Value : type.AverageSpeed));
        }

        public static double CalculateChangeTransportTime(double lat, double lng, double lat2, double lng2, Route routeTo, Type typeTo)
        {
            // half of frequency + 1 minute
            double estimatedChangeTimeMins = (routeTo.Frequency ?? typeTo.Frequency) / 2 + 1;

            return CalculateTransportTime(lat, lng, lat2, lng2, null, ViaDFGraph.WALKING_TYPE) + estimatedChangeTimeMins;
        }
    }
}
