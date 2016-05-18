using System;
using System.Collections.Generic;
using System.Linq;

namespace viadf.Models
{
    public class CreateRouteModel
    {
        public List<viadflib.Type> TypeList { get; set; }

        public int? id { get; set; }
        public string routename { get; set; }
        public string email { get; set; }
        public string description { get; set; }
        public int type { get; set; }

        public int? parentId { get; set; }

        public int status { get; set; }

        public string origin { get; set; }
        public string destination { get; set; }
        public string mapdata1 { get; set; }
        public string mapdata2 { get; set; }
        public string mapnames { get; set; }
        

        public bool Valid { get; set; }

        public string GetMapNameAtPosition(string coords)
        {
            if (NameDictionary.Keys.Contains(coords))
            {
                return NameDictionary[coords];
            }
            else
            {
                return null;
            }
        }

        public Dictionary<string, string> NameDictionary
        {
            get
            {
                var dict = new Dictionary<string, string>();
                if (!string.IsNullOrWhiteSpace(mapnames))
                {                  
                    var namecoords = mapnames.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var nc in namecoords)
                    {
                        var values = nc.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                        dict[values[0]] = values[1];
                    }
                }
                return dict;
            }
        }

        public string[] Positions1
        {
            get
            {
                if (string.IsNullOrWhiteSpace(mapdata1))
                {
                    return new string[0];
                }
                else
                {
                    return mapdata1.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                }
            }
        }

        public string[] Positions2
        {
            get
            {
                if (string.IsNullOrWhiteSpace(mapdata2))
                {
                    return new string[0];
                }
                else
                {
                    return mapdata2.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                }
            }
        }
    }

    
}