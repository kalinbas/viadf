using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;

namespace viadflib
{
    public class Program
    {      
        public static void CrawlViaDF()
        {
            string[] lines = File.ReadAllLines("C:\\temp.txt");

            using (var context = new DataContext())
            {               
                foreach (var line in lines)
                {
                    string[] eles = line.Split('|');

                    List<RoutePiece> pieces = context.RoutePieces.Where(x => x.Lat == double.Parse(eles[1]) && x.Lng == double.Parse(eles[2])).ToList();
                    Console.WriteLine(eles[0] + " " + pieces.Count);
                    foreach (var piece in pieces)
                    {
                        piece.Name = eles[0];
                    }
                    context.SubmitChanges();
                }               
            }

            /*
            List<Delegacion> delegaciones = new DataDataContext().Delegacions.ToList();
            foreach (var delegacion in delegaciones)
            {
                Console.WriteLine(delegacion.Name);
                Console.WriteLine("--------------");
                ProcessColonias(delegacion);                
            }*/

            //ProcessGeoInformation(); 

            /*
            ProcessRoutes("http://viadf.org/ListaRutas.aspx?code=Metro", 1);
            ProcessRoutes("http://viadf.org/ListaRutas.aspx?code=Metrobus", 2);
            ProcessRoutes("http://viadf.org/ListaRutas.aspx?code=RTP", 3);
            ProcessRoutes("http://viadf.org/ListaRutas.aspx?code=Microbus", 4);
            ProcessRoutes("http://viadf.org/ListaRutas.aspx?code=TrenLigero", 5);
            ProcessRoutes("http://viadf.org/ListaRutas.aspx?code=Trolebus", 6);
            ProcessRoutes("http://viadf.org/ListaRutas.aspx?code=Pumabus", 7);
            ProcessRoutes("http://viadf.org/ListaRutas.aspx?code=Suburbano", 8);*/
        }

        public static void ProcessRoutes(string url, int transportType)
        {
            WebClient client = new WebClient();
            client.Encoding = UTF8Encoding.UTF8;
            string data = client.DownloadString(url);

            Regex regex = new Regex("<td><a href = \"(.*?)\">(.*?)</a></td>.*?<td>(.*?)</td>.*?<td>(.*?)</td>", RegexOptions.Singleline);

            

            foreach (Match match in regex.Matches(data))
            {
                ProcessRoute("http://viadf.org/" + match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value, match.Groups[4].Value, transportType);
            }        

            
        }

        public static void ProcessRoute(string url, string name, string from, string to, int transportType)
        {
            Console.WriteLine(name + " " + from + " - " + to);

            using (var context = new DataContext())
            {                

                Route route = new Route();
                route.Name = name;
                route.Status = 1;
                route.FromName = from;
                route.ToName = to;
                route.TypeID = transportType;

                WebClient client = new WebClient();
                string data = client.DownloadString(url);
                Regex regex1 = new Regex("polyLine1(.*?)#ff0000");
                
                string data1 = regex1.Match(data).Groups[1].Value; 
               
                Regex coordRegex = new Regex("new GLatLng\\((.*?), (.*?)\\)");
                foreach (Match match in coordRegex.Matches(data1))
                {
                    RoutePiece piece = new RoutePiece();
                    piece.Lat = double.Parse(match.Groups[1].Value);
                    piece.Lng = double.Parse(match.Groups[2].Value);
                    route.RoutePieces.Add(piece);
                }

                Regex regex2 = new Regex("polyLine2(.*?)#0000ff");
                RoutePiece firstPiece = null;
                if (regex2.IsMatch(data))
                {
                    string data2 = regex2.Match(data).Groups[1].Value;
                    foreach (Match match in coordRegex.Matches(data2))
                    {                       
                        RoutePiece piece = new RoutePiece();
                        piece.Lat = double.Parse(match.Groups[1].Value);
                        piece.Lng = double.Parse(match.Groups[2].Value);
                        route.RoutePieces.Add(piece);

                        if (firstPiece == null) {
                            firstPiece = piece;
                        }
                    }
                }


                context.Routes.InsertOnSubmit(route);
                context.SubmitChanges();  
              
                route.SplitRoutePieceID = (firstPiece == null) ? null : (int?)firstPiece.ID;
                context.SubmitChanges();
            }           
        }

        public static void ProcessGeoInformation()
        {
            using (var context = new DataContext())
            {
                foreach (var cs in context.StreetCrossings)
                {
                    if (cs.Lng == 0 || cs.Lat == 0)
                    {                        
                        StreetCrossing inverseCrossing = null;
                        var streets = context.Streets.Where(x => x.SourceID == cs.SourceID && x.ColoniaID == cs.Street.ColoniaID).ToList();
                        if (streets.Count == 1)
                        {
                            var street = streets.First();
                            var inverseCrossings = context.StreetCrossings.Where(x => x.StreetID == street.ID && x.SourceID == cs.Street.SourceID).ToList();
                            if (inverseCrossings.Count == 1)
                            {
                                inverseCrossing = inverseCrossings.First();
                            }
                        }

                        if (inverseCrossing != null && inverseCrossing.Lat != 0 && inverseCrossing.Lng != 0)
                        {
                            cs.Lat = inverseCrossing.Lat;
                            cs.Lng = inverseCrossing.Lng;
                        } 
                        else 
                        {
                            var coords = GetCoordinates(cs.Street.Colonia.Delegacion.Estado.SourceID, cs.Street.Colonia.Delegacion.SourceID, cs.Street.Colonia.SourceID, cs.Street.SourceID, cs.SourceID);
                            cs.Lat = coords.Item1;
                            cs.Lng = coords.Item2;
                        }                       
                        context.SubmitChanges();
                        Console.WriteLine(cs.Street.Name+" "+cs.Name+" "+cs.Lat + " " + cs.Lng);
                    }
                }
            }            
        }
               
        public static void ProcessColonias(Delegacion delegacion)
        {
            var colonias = new List<Colonia>();

            WebClient client = new WebClient();
            string data = client.DownloadString("http://santander.mapasactivos.com/pob_search.asp?e="+delegacion.Estado.SourceID+"&ee=1&c=&suc=1&pob=" + delegacion.SourceID);

            Regex minimizer = new Regex("<select name=\"icl\"(.*?)</select>", RegexOptions.Singleline);
            string relevantData = minimizer.Match(data).Groups[1].Value;

            Regex regex = new Regex("<option value=\"(\\d+)\">(.+?)</option>");

            foreach (Match match in regex.Matches(relevantData))
            {
                int sourceColoniaID = int.Parse(match.Groups[1].Value);

                // just process it if not already in DB
                if (new DataContext().Colonias.Count(x => x.SourceID == sourceColoniaID && x.Delegacion.SourceID == delegacion.SourceID) == 0)
                {
                    Colonia col = new Colonia { SourceID = sourceColoniaID, Name = match.Groups[2].Value, DelegacionID = delegacion.ID };
                    col.Streets.AddRange(GetStreets(delegacion.Estado.SourceID, delegacion, col));
                    Console.WriteLine(col.Name + " " + col.Streets.Count);
                    SaveColoniaToDB(col);
                }
            }
        }

        public static void SaveColoniaToDB(Colonia colonia)
        {
            using (var context = new DataContext())
            {
                context.Colonias.InsertOnSubmit(colonia);
                context.SubmitChanges();
            }
        }

        public static List<Street> GetStreets(int estadoID, Delegacion delegacion, Colonia colonia)
        {
            var streets = new List<Street>();

            WebClient client = new WebClient();
            string data = client.DownloadString("http://santander.mapasactivos.com/street_finder.asp?pob=" + delegacion.SourceID + "&suc=1&c=" + colonia.SourceID + "&cd=1&m=" + delegacion.SourceID + "&e=" + estadoID + "&d=1&tu=4&icl=" + colonia.SourceID + "&id2=0");

            Regex regex = new Regex("parent.streets\\[ parent.streets.length \\] = \"(.*?)\";  parent.idStreets\\[ parent.idStreets.length \\] = \"(.*?)\";");

            foreach (Match match in regex.Matches(data))
            {
                var street = new Street { SourceID = int.Parse(match.Groups[2].Value), Name = match.Groups[1].Value };
                street.FullName = street.Name + ", " + colonia.Name + ", " + delegacion.Name;
                street.StreetCrossings.AddRange(GetCrossingStreets(estadoID, delegacion.SourceID, colonia.SourceID, street));
                streets.Add(street);
                Console.WriteLine(street.FullName);
            }

            return streets;
        }

        public static List<StreetCrossing> GetCrossingStreets(int estadoID, int delegacionID, int coloniaID, Street street)
        {
            var streets = new List<StreetCrossing>();

            WebClient client = new WebClient();
            string data = client.DownloadString(string.Format("http://santander.mapasactivos.com/corner_selector2.asp?pob={0}&suc=1&c={1}&cd=1&m={0}&e=" + estadoID + "&d=1&tu=4&icl={1}&id1={2}", delegacionID, coloniaID, street.SourceID));

            Regex regex = new Regex("parent.corners\\[ parent.corners.length \\] = \"(.*?)\";  parent.idCalles\\[ parent.idCalles.length \\] = \"(.*?)\";");

            foreach (Match match in regex.Matches(data))
            {
                StreetCrossing cs = new StreetCrossing { SourceID = int.Parse(match.Groups[2].Value), Name = match.Groups[1].Value };
                //var coords = GetCoordinates(estadoID, delegacionID, coloniaID, street.SourceID, cs.SourceID);
                cs.Lat = 0;//coords.Item1;
                cs.Lng = 0;//coords.Item2;
                
                cs.Street = street;
                cs.Street1 = street;
                
                streets.Add(cs);
            }

            return streets;
        }

        public static Tuple<double, double> GetCoordinates(int estadoID, int delegacionID, int coloniaID, int streetID, int crossingID)
        {
            WebClient client = new WebClient();
            string data = client.DownloadString(string.Format("http://santander.mapasactivos.com/geo_search.asp?pob={0}&suc=1&c={1}&cd=1&m={0}&e=" + estadoID + "&d=1&tu=4&icl={1}&id1={2}&id2={3}", delegacionID, coloniaID, streetID, crossingID));

            Regex regex = new Regex("map2\\.asp\\?tu=4&x=(.*?)&y=(.*?)&");
            Match match = regex.Match(data);
            if (match.Groups.Count > 1)
            {
                return new Tuple<double, double>(double.Parse(match.Groups[2].Value), double.Parse(match.Groups[1].Value));
            }
            else
            {
                return new Tuple<double, double>(-1, -1);
            }
        }
    }
}
