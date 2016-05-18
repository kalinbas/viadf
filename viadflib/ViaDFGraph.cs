using System;
using System.Collections.Generic;
using System.Linq;
using viadflib.AStar;

namespace viadflib
{
    /// <summary>
    /// Multithread-capable singleton Graph Object used in viadf searches
    /// </summary>
    public sealed class ViaDFGraph : Graph
    {
        public const double KM_IN_DEGREES = 0.00926;
        public const double HALF_KM_IN_DEGREES = KM_IN_DEGREES / 2;
        public const double M100_IN_DEGREES = KM_IN_DEGREES / 10;

        public static Type WALKING_TYPE = new DataContext().Types.Where(x => x.IsWalkingType).First();

        private static volatile ViaDFGraph instance;
        private static volatile bool instanceNeedsReload;
        private static object syncRoot = new Object();

        public static ViaDFGraph Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new ViaDFGraph();
                        }
                    }
                }

                if (instanceNeedsReload)
                {
                    lock (syncRoot)
                    {
                        if (instanceNeedsReload)
                        {
                            ViaDFGraph tempInstance = new ViaDFGraph();
                            instance = tempInstance;
                            instanceNeedsReload = false;
                        }
                    }
                }

                return instance;
            }
        }

        public static void SetReloadFlag()
        {
            lock (syncRoot)
            {
                instanceNeedsReload = true;
            }
        }

        public ViaDFGraph()
        {
            InitializePrecalculatedGraph();
        }

        private void InitializePrecalculatedGraph()
        {
            // clear the existing graph
            Clear();

            // load index from DB
            using (DataContext context = new DataContext())
            {
                Dictionary<int, Node> nodeIndex = new Dictionary<int, Node>();

                List<RoutePiece> pieces = context.RoutePieces.Where(x => x.Route.Status == (int)StatusEnum.ActiveAndIndexed).ToList();                
                foreach (var p in pieces)
	            {
                    Node n = new Node(p.Lat, p.Lng);
                    n.RouteID = p.RouteID;
                    n.RoutePieceID = p.ID;
                    AddNode(n);
                    nodeIndex[n.RoutePieceID] = n;
	            }
                List<SearchIndex> index = context.SearchIndexes.ToList();
                foreach (var i in index)
                {
                    if (nodeIndex.Keys.Contains(i.RoutePieceID) && nodeIndex.Keys.Contains(i.RoutePiece2ID))
                    {
                        AddArc(nodeIndex[i.RoutePieceID], nodeIndex[i.RoutePiece2ID], i.Cost);
                    }
                }
            }
        }     

        private List<Node> ClosestNodes(Node node, int count, double maxDistance = double.MaxValue)
        {
            SortedList<double, Node> bestNodes = new SortedList<double, Node>();

            foreach (var n in Nodes)
            {
                double distance = Node.ManhattanDistance(n, node);
                if (distance < maxDistance)
                {
                    if (bestNodes.Count < count || bestNodes.Last().Key > distance)
                    {
                        bool added = false;
                        while (!added)
                        {
                            try
                            {
                                bestNodes.Add(distance, n);
                                if (bestNodes.Count(x => x.Value.RouteID == n.RouteID) > 1) {
                                    bestNodes.Remove(bestNodes.Last(x => x.Value.RouteID == n.RouteID).Key);
                                }
                                added = true;
                            }
                            catch (Exception ex)
                            {
                                distance += 0.0000000001;
                            }
                        }
                    }
                    if (bestNodes.Count > count)
                    {
                        bestNodes.RemoveAt(bestNodes.Count - 1);                        
                    }                    
                }
            }

            return bestNodes.Values.ToList();
        }

        public void CreateDirectLink(Node n1, Node n2, double maxWalkTime)
        {
            double time = Indexer.CalculateTransportTime(n1.X, n1.Y, n2.X, n2.Y, null, WALKING_TYPE);

            if (time <= maxWalkTime) {
                Arc arc = new Arc(n1, n2, time);
                n1.OutgoingArcs.Add(arc);
                n2.IncomingArcs.Add(arc);
            }
        }

        public void CreateLinksForTemporaryNode(Node n, bool first, int count)
        {           
            // DO NOT ADD Node to Graph Node List
            List<Node> nearest = ClosestNodes(n, count, KM_IN_DEGREES);
            if (nearest.Count == 0)
            {
                // if none found just take closest of all
                nearest = ClosestNodes(n, 1);
            }

            List<Route> routes = new List<Route>();
            if (first)
            {
                routes = DataHandler.GetAllRoutesForIds(nearest.Select(x => x.RouteID).Distinct().ToList());
            }

            foreach (Node node in nearest)
            {
                if (first)
                {
                    var route = routes.Where(x => x.ID == node.RouteID).First();
                    n.OutgoingArcs.Add(new Arc(n, node, Indexer.CalculateChangeTransportTime(n.X, n.Y, node.X, node.Y, route, route.Type)));
                }
                else
                {
                    n.IncomingArcs.Add(new Arc(node, n, Indexer.CalculateTransportTime(n.X, n.Y, node.X, node.Y, null, WALKING_TYPE)));
                }
            }
        }

        public double GetArcDistanceInKm(Arc arc)
        {
            return Node.EuclidianDistance(arc.StartNode, arc.EndNode) / KM_IN_DEGREES;
        }
    }

}
