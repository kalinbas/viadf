using System;
using System.Collections.Generic;

namespace viadflib.AStar
{
	internal class Track : IComparable
	{
        private AStar aStar;

		public Node EndNode;
		public Track Queue;

		private int _NbArcsVisited;
		public int NbArcsVisited { get { return _NbArcsVisited; } }

		private double _Cost;
		public double Cost { get { return _Cost; } }

		public double Evaluation
		{
			get
			{
                return _Cost + aStar.Heuristic(EndNode);
			}
		}

		public bool Succeed { get { return EndNode==aStar.EndNode; } }


        /// <summary>
        /// First track
        /// </summary>
        public Track(Node GraphNode, AStar a)
		{
            aStar = a;
            _Cost = 0;
			_NbArcsVisited = 0;
			Queue = null;
			EndNode = GraphNode;
		}

        /// <summary>
        /// Track with PreviousTrack and Transition
        /// </summary>
        public Track(Track PreviousTrack, Arc Transition, AStar a)
		{
            aStar = a;
			Queue = PreviousTrack;
			_Cost = Queue.Cost + Transition.Cost;
			_NbArcsVisited = Queue._NbArcsVisited + 1;
			EndNode = Transition.EndNode;
		}

		public int CompareTo(object obj)
		{
            Track otherTrack = (Track)obj;
            return otherTrack.Evaluation.CompareTo(Evaluation);
		}

        public bool IsDuplicate(Track other)
        {
            var thisRoutes = CleanDuplicateRouteElements(CollectRoutes());
            var otherRoutes = CleanDuplicateRouteElements(other.CollectRoutes());

            if (thisRoutes.Count > otherRoutes.Count)
            {
                return false;
            }

            bool upDuplicate = true;
            bool downDuplicate = true;

            for (int i = 0; i < thisRoutes.Count; i++)
            {
                if (thisRoutes[i] != otherRoutes[i])
                {
                    upDuplicate = false;
                }
                if (thisRoutes[thisRoutes.Count - i - 1] != otherRoutes[otherRoutes.Count - i - 1])
                {
                    downDuplicate = false;
                }
            }

            return upDuplicate || downDuplicate;
        }

        private List<int> CleanDuplicateRouteElements(List<int> routeElements)
        {
            List<int> routes = new List<int>();
            int lastElement = int.MinValue;
            foreach (var element in routeElements)
            {
                if (lastElement != element)
                {
                    routes.Add(element);
                    lastElement = element;
                }
            }

            return routes;
        }

        public List<int> CollectRoutes()
        {
            List<int> routes = new List<int>();
            if (Queue != null)
            {
                routes.AddRange(Queue.CollectRoutes());
                routes.Add(EndNode.RouteID);
            }
            else
            {
                routes.Add(EndNode.RouteID);
            }
            return routes;
        }

		public static bool SameEndNode(object O1, object O2)
		{
			Track P1 = O1 as Track;
			Track P2 = O2 as Track;
			if ( P1==null || P2==null ) throw new ArgumentException("Objects must be of 'Track' type.");
			return P1.EndNode==P2.EndNode;
		}
	}
}
