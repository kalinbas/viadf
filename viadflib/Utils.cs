using System;
using System.Text;
using System.IO;
using System.Net;
using System.Xml;

namespace viadflib
{
    public class Utils
    {
        public static string Capitalize(string s)
        {
            StringBuilder sb = new StringBuilder(s.Length);
            bool capitalize = true;
            foreach (char c in s)
            {
                sb.Append(capitalize ? Char.ToUpper(c) : Char.ToLower(c));
                capitalize = !Char.IsLetter(c);
            }
            return sb.ToString();
        }
        
        public static string FormatSEO(string name)
        {
            if (name == null)
                return name;

            name = name.Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u");
            name = name.Replace("Á", "A").Replace("É", "E").Replace("Í", "i").Replace("Ó", "O").Replace("Ú", "U");
            name = name.Replace("ü", "u").Replace("ñ", "n");
            name = name.Replace(" ", "-").Replace(",", "-").Replace(".", "-").Replace("/", "-");
            name = name.Replace("--", "-").Replace("--", "-");
            return name.ToLower();
        }

        public static string FormatCoordinates(double lat, double lng)
        {
            return Math.Round(lat, 8) + "," + Math.Round(lng, 8);
        }      

        public static SearchPosition GeoCodeSearchPosition(string input)
        {
            string url = string.Format("https://maps.googleapis.com/maps/api/geocode/xml?address={0}+mexico city&sensor=false", input);
           
            try
            {              
                using (WebClient wc = new WebClient()) {

                    using (StreamReader sr = new StreamReader(wc.OpenRead(url)))
                    {

                        using (XmlTextReader xtr = new XmlTextReader(sr))
                        {

                            double? lat = null, lng = null;

                            //Start reading our xml document.
                            while (xtr.Read())
                            {
                                xtr.MoveToElement();
                                switch (xtr.Name)
                                {
                                    case "lat":
                                        if (!lat.HasValue)
                                        {
                                            xtr.Read();
                                            lat = double.Parse(xtr.Value);
                                            
                                        }
                                        break;
                                    case "lng":
                                        if (!lng.HasValue)
                                        {
                                            xtr.Read();
                                            lng = double.Parse(xtr.Value);
                                            return new SearchPosition(lat.Value, lng.Value, input);
                                        }
                                        break;
                                        
                                }
                            }
                        }
                    }
                }
            }             
            catch
            {
                
            }

            return null;
        }
    }
}
