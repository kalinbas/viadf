using System;
using System.Collections.Generic;


namespace viadflib.AStar
{
    /// <summary>
    /// Class to search the best path between two nodes on a graph.
    /// </summary>
    public class AStar
    {        
        Graph _Graph;
        PriorityQueue _Open;
        Dictionary<Node, double> _Closed;
        int _NbIterations = -1;
        int _NbResults = 1;
        List<Track> _results;

        private Node _startNode;
        public Node StartNode { 
            get
            {
                return _startNode;
            }
            set
            {
                _startNode = value;
            }
        }

        private Node _endNode;
        public Node EndNode
        {
            get
            {
                return _endNode;
            }
            set
            {
                _endNode = value;

                // calculate routepieceids which lead to the end node
                RoutePieceIDArcsToEndNode = new Dictionary<int, Arc>();
                foreach (Arc arc in EndNode.IncomingArcs)
                {
                    RoutePieceIDArcsToEndNode.Add(arc.StartNode.RoutePieceID, arc);
                }
            }
        }
        public int NrOfFoundResults
        {
            get
            {
                return _results.Count;
            }           
        }

        public Dictionary<int, Arc> RoutePieceIDArcsToEndNode { get; set; }
    
        public AStar(Graph G)
        {
            _Graph = G;
            _Open = new PriorityQueue();
            _Closed = new Dictionary<Node, double>();            
        }

        public bool SearchPath(int nrOfResults)
        {
            Initialize();

            _NbResults = nrOfResults;

            while (NextStep()) { }
            return PathFound;
        }      

        private void Initialize()
        {
            if (StartNode == null || EndNode == null) throw new ArgumentNullException();
            _Closed.Clear();
            _Open.Clear();
            _Open.Enqueue(new Track(StartNode, this));
            _NbIterations = 0;
            _NbResults = 1;
            _results = new List<Track>();
        }

        private bool NextStep()
        {
            if (!Initialized) throw new InvalidOperationException("You must initialize AStar before launching the algorithm.");
            if (_Open.Count == 0) return false;
            _NbIterations++;

            Track bestTrack = (Track)_Open.Dequeue();
            if (bestTrack.Succeed)
            {
                if (!_results.Exists(x => x.IsDuplicate(bestTrack)))
                {
                    _results.Add(bestTrack);
                    if (_results.Count >= _NbResults)
                    {
                        _Open.Clear();
                    }
                }
            }
            else
            {
                Propagate(bestTrack);
            }
            return _Open.Count > 0;
        }

        private void Propagate(Track track)
        {
            double oldValue = double.MaxValue;
            if (_Closed.ContainsKey(track.EndNode))
            {
                oldValue = _Closed[track.EndNode];
            }

            // only if new value is better - propagate
            if (track.Evaluation < oldValue)
            {
                foreach (Arc arc in track.EndNode.OutgoingArcs)
                {
                    Track successor = new Track(track, arc, this);
                    double oldValue2 = double.MaxValue;
                    if (_Closed.ContainsKey(arc.EndNode))
                    {
                        oldValue2 = _Closed[arc.EndNode];
                    }
                    if (track.Evaluation < oldValue2)
                    {
                        _Open.Enqueue(successor);
                    }
                }

                // only add track to end node when not in a change of transport
                bool inChange = track.Queue != null && track.EndNode.RouteID != track.Queue.EndNode.RouteID;
                if (!inChange && RoutePieceIDArcsToEndNode.ContainsKey(track.EndNode.RoutePieceID))
                {
                    Track successor = new Track(track, RoutePieceIDArcsToEndNode[track.EndNode.RoutePieceID], this);
                    _Open.Enqueue(successor);
                }
                _Closed[track.EndNode] = track.Evaluation;
            }                  
        }

        /// <summary>
        /// To know if the search has been initialized.
        /// </summary>
        public bool Initialized { get { return _NbIterations >= 0; } }

        /// <summary>
        /// To know if the search has been started.
        /// </summary>
        public bool SearchStarted { get { return _NbIterations > 0; } }

        /// <summary>
        /// To know if the search has ended.
        /// </summary>
        public bool SearchEnded { get { return SearchStarted && _Open.Count == 0; } }

        /// <summary>
        /// To know if a path has been found.
        /// </summary>
        public bool PathFound { get { return _results.Count > 0; } }

        /// <summary>
        /// Use for a 'step by step' search only.
        /// Gets the number of the current step.
        /// -1 if the search has not been initialized.
        /// 0 if it has not been started.
        /// </summary>
        public int StepCounter { get { return _NbIterations; } }

        private void CheckSearchHasEnded()
        {
            if (!SearchEnded) throw new InvalidOperationException("You cannot get a result unless the search has ended.");
        }
         
        public Arc[] GetPathByArcs(int index)
        {            
            CheckSearchHasEnded();
            if (!PathFound) return null;
            int Nb = _results[index].NbArcsVisited;
            Arc[] Path = new Arc[Nb];
            Track Cur = _results[index];
            for (int i = Nb - 1; i >= 0; i--, Cur = Cur.Queue)
            {
                Path[i] = Cur.Queue.EndNode.ArcGoingTo(Cur.EndNode);
                if (Path[i] == null)
                {
                    foreach (Arc arc in Cur.EndNode.IncomingArcs)
	                {
		                if (arc.StartNode.RoutePieceID == Cur.Queue.EndNode.RoutePieceID) {
                            Path[i] = arc;
                        }
	                }
                                         
                }
            }
            return Path;
        }

        public double Heuristic(Node from)
        {
            double distance = Node.ManhattanDistance(from, EndNode) / ViaDFGraph.KM_IN_DEGREES;
            //double factor = distance < 0.5 ? 10 : distance > 10 ? 2.5 : (1 - (distance - 0.5) / (10 - 0.5)) * 7.5 + 2.5;
            
            // assume overall average travel speed of 24 km/h
            return 2.5 * distance;
        }
    }
}

