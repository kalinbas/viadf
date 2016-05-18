using System;

namespace viadflib.AStar
{

	[Serializable]
	public class Arc
	{
        public Node StartNode { get; set; }
        public Node EndNode { get; set; }
        public double Cost { get; set; }

        public Arc(Node startNode, Node endNode, double cost)
        {
            StartNode = startNode;
            EndNode = endNode;
            Cost = cost;
        }

		public override string ToString()
		{
            return StartNode + "-->" + EndNode;
		}

		public override bool Equals(object O)
		{
			Arc A = (Arc) O;
			if ( A==null ) throw new ArgumentException("Cannot compare "+GetType()+" with "+O.GetType());
            return StartNode.Equals(A.StartNode) && EndNode.Equals(A.EndNode);
		}

        public override int GetHashCode() { return StartNode.GetHashCode() + EndNode.GetHashCode(); }
	}
}

