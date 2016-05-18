using System.Collections.Generic;
using viadflib;

namespace viadf.Models
{
    public class RoutePieceModel
    {
        public RoutePiece RoutePiece { get; set; }
        public List<Route> ConnectingRoutes { get; set; }
        public SmallSearchBoxModel SmallSearchBoxModel { get; set; }
    }
}