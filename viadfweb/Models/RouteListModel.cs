using System.Collections.Generic;
using viadflib;

namespace viadf.Models
{
    public class RouteListModel
    {
        public List<viadflib.Type> AllTypes { get; set; }
        public viadflib.Type SelectedType { get; set; }
        public List<Route> Routes { get; set; }
    }
}