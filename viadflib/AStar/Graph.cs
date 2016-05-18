using System;
using System.Collections.Generic;

namespace viadflib.AStar
{
    [Serializable]
    public class Graph
    {
        private List<Node> _nodeList;

        public Graph()
        {
            _nodeList = new List<Node>();
        }

        public List<Node> Nodes { get { return _nodeList; } }

        public void Clear()
        {
            _nodeList.Clear();
        }

        public bool AddNode(Node NewNode)
        {
            _nodeList.Add(NewNode);
            return true;
        }      

        public bool AddArc(Node StartNode, Node EndNode, double Cost)
        {
            Arc newArc = new Arc(StartNode, EndNode, Cost);
            newArc.StartNode.OutgoingArcs.Add(newArc);
            newArc.EndNode.IncomingArcs.Add(newArc);
            return true;
        }

        public void Add2Arcs(Node Node1, Node Node2, double Cost)
        {
            AddArc(Node1, Node2, Cost);
            AddArc(Node2, Node1, Cost);
        }

        public bool RemoveNode(Node NodeToRemove)
        {
            if (NodeToRemove == null) return false;
            try
            {
                foreach (Arc a in NodeToRemove.IncomingArcs)
                {
                    a.StartNode.OutgoingArcs.Remove(a);
                }
                foreach (Arc a in NodeToRemove.OutgoingArcs)
                {
                    a.EndNode.IncomingArcs.Remove(a);
                }
                _nodeList.Remove(NodeToRemove);
            }
            catch { return false; }
            return true;
        }

        public bool RemoveArc(Arc ArcToRemove)
        {
            if (ArcToRemove == null) return false;

            ArcToRemove.StartNode.OutgoingArcs.Remove(ArcToRemove);
            ArcToRemove.EndNode.IncomingArcs.Remove(ArcToRemove);

            return true;
        }
    }
}
