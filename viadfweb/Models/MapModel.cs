using System.Collections.Generic;
using viadflib;

namespace viadf.Models
{
    public class MapModel
    {
        public List<viadflib.Type> AllTypes { get; set; }
        public viadflib.Type SelectedType { get; set; }
        public Dictionary<Route, List<RoutePiece>> Routes { get; set; }       
    }
}