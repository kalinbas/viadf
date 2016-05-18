using System;
using System.Collections;

namespace viadflib.AStar
{	
	[Serializable]
	public class Node
	{
		Point2D _Position;
		ArrayList _IncomingArcs, _OutgoingArcs;

        public int RouteID { get; set; }
        public int RoutePieceID { get; set; }

		public Node(double PositionX, double PositionY)
		{
			_Position = new Point2D(PositionX, PositionY);
			_IncomingArcs = new ArrayList();
			_OutgoingArcs = new ArrayList();
		}

		public IList IncomingArcs { get { return _IncomingArcs; } }

		public IList OutgoingArcs { get { return _OutgoingArcs; } }

		public double X { get { return Position.X; } }

		public double Y { get { return Position.Y; } }

		public Point2D Position
		{			
			get { return _Position; }
		}

		public Arc ArcGoingTo(Node N)
		{
			if ( N==null ) throw new ArgumentNullException();
			foreach (Arc A in _OutgoingArcs)
				if (A.EndNode == N) return A;
			return null;
		}

		public override string ToString() { return RouteID + " "+ Position.ToString(); }

        public override bool Equals(object O)
		{
			Node N = (Node)O;
			if ( N==null ) throw new ArgumentException(O.GetType()+" cannot be compared with "+GetType());
			return N.RoutePieceID == RoutePieceID;
		}

		public object Clone()
		{
			Node N = new Node(X, Y);
			return N;
		}

		public override int GetHashCode() { return RoutePieceID; }

		public static double ManhattanDistance(Node N1, Node N2)
		{
			if ( N1==null || N2==null ) throw new ArgumentNullException();
			double DX = N1.Position.X - N2.Position.X;
			double DY = N1.Position.Y - N2.Position.Y;			
			return Math.Abs(DX)+Math.Abs(DY);
		}

        public static double EuclidianDistance(Node N1, Node N2)
        {
            return Math.Sqrt(Math.Pow(N1.X - N2.X, 2) + Math.Pow(N1.Y - N2.Y, 2));
        }
	}
}

