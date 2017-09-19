using System.Collections.Generic;
using viadflib;

namespace viadf.Models
{
    public class BusinessModel
    {
        public Business Business { get; set; }
        public List<Route> ConnectingRoutes { get; set; }
        public SmallSearchBoxModel SmallSearchBoxModel { get; set; }
        public List<Business> CloseBusinesses { get; set; }
        public Colonia Colonia { get; set; }
    }
}