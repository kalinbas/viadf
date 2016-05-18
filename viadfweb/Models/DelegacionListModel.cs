using System.Collections.Generic;
using viadflib;

namespace viadf.Models
{
    public class DelegacionListModel
    {
        public List<Delegacion> All { get; set; }
        public Delegacion Delegacion { get; set; }
        public List<Colonia> Colonias { get; set; }
    }
}