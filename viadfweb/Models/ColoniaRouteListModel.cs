using System.Collections.Generic;
using viadflib;

namespace viadf.Models
{
    public class ColoniaRouteListModel
    {
        public Colonia Colonia { get; set; }
        public List<Route> Routes { get; set; }
        public SmallSearchBoxModel SmallSearchBoxModel { get; set; }
    }
}