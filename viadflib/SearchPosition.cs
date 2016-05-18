using System.Globalization;
using System.Runtime.Serialization;
namespace viadflib
{
    [DataContract]
    public class SearchPosition
    {
        [DataMember]
        public double Lat { get; set; }
        [DataMember]
        public double Lng { get; set; }

        [DataMember(EmitDefaultValue=false)]
        public string Name { get; set; }
        
        public static SearchPosition CreateSearchPosition(string coords, string name) {
            try
            {
                string[] c = coords.Split(',');
                double lat = double.Parse(c[0], CultureInfo.InvariantCulture);
                double lng = double.Parse(c[1], CultureInfo.InvariantCulture);
                return new SearchPosition(lat, lng, name);
            } catch {
                return null;
            }
            
        }

        public SearchPosition(double lat, double lng, string name)
        {
            Lat = lat;
            Lng = lng;
            Name = name;
        }       

        public void LoadNameFromDB() {
            if (string.IsNullOrWhiteSpace(Name))
            {
                Name = DataHandler.GetNameAtPosition(Lat, Lng);                
            } 
        }
    }

}