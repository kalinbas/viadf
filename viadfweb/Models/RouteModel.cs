using System.Collections.Generic;
using viadflib;

namespace viadf.Models
{
    public class RouteModel
    {
        public Route Route { get; set; }
        public List<RoutePiece> RoutePieces { get; set; }
        public List<RoutePiece> RoutePieces1 { get; set; }
        public List<RoutePiece> RoutePieces2 { get; set; }
        public List<string> RouteStreets { get; set; }
        public List<Colonia> RouteColonias { get; set; }
    }
}