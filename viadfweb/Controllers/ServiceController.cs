using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using viadf.Models;
using viadflib;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using viadflib.TravelTime;
using SearchResultModel = viadflib.SearchResultModel;
using System.Runtime.Serialization.Json;

namespace viadf.Controllers
{
    public class ServiceController : ControllerBase
    {
        public static List<Street> StreetIndex;
        public static List<POI> POIIndex;

        static ServiceController()
        {
            StreetIndex = new DataContext().Streets.OrderBy(x => x.FullName).ToList();
            POIIndex = new DataContext().POIs.ToList();
            using (DataContext context = new DataContext())
            {
                var dlo = new System.Data.Linq.DataLoadOptions();
                dlo.LoadWith<RoutePiece>(x => x.Route);
                dlo.LoadWith<Route>(x => x.Type);
                context.LoadOptions = dlo;
                List<RoutePiece> pieces = new DataContext().RoutePieces.Where(x => x.Name != null && x.Route.Status == (int)StatusEnum.ActiveAndIndexed && (x.Route.TypeID == (int)TypeEnum.Metro || x.Route.TypeID == (int)TypeEnum.Metrobus || x.Route.TypeID == (int)TypeEnum.TrenLigero)).ToList();
                foreach (var piece in pieces)
                {
                    string name = (piece.Route.Type.Name + " " + piece.Name).ToUpper().Replace("Á", "A").Replace("É", "E").Replace("Í", "I").Replace("Ó", "O").Replace("Ú", "U");
                    if (!POIIndex.Exists(x => x.Name == name))
                    {                        
                        POIIndex.Add(new POI { Name = name, Lat = piece.Lat, Lng = piece.Lng });
                    }
                }
            }
            POIIndex = POIIndex.OrderBy(x => x.Name).ToList();
        }

        [OutputCache(Duration = 3600, VaryByParam = "term")]
        public ActionResult GetLocations(string term)
        {
            List<AutoCompleteResult> results = new List<AutoCompleteResult>();

            if (!string.IsNullOrWhiteSpace(term))
            {
                string[] tokens = term.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < tokens.Length; i++)
                {
                    tokens[i] = tokens[i].ToUpper();
                    tokens[i] = tokens[i].Replace("Á", "A").Replace("É", "E").Replace("Í", "I").Replace("Ó", "O").Replace("Ú", "U");
                }

                foreach (var poi in POIIndex)
                {
                    bool poiOK = true;
                    foreach (var token in tokens)
                    {
                        if (token != null && !poi.Name.Contains(token))
                        {
                            poiOK = false;
                            break;
                        }
                    }
                    if (poiOK)
                    {
                        results.Add(new AutoCompleteResult { id = Utils.FormatCoordinates(poi.Lat, poi.Lng), label = viadflib.Utils.Capitalize(poi.Name), value = viadflib.Utils.Capitalize(poi.Name) });
                    }
                    if (results.Count >= 10)
                    {
                        break;
                    }
                }

                foreach (var street in StreetIndex)
                {
                    bool streetOK = true;
                    foreach (var token in tokens)
                    {
                        if (token != null && !street.FullName.Contains(token))
                        {
                            streetOK = false;
                            break;
                        }
                    }
                    if (streetOK)
                    {
                        results.Add(new AutoCompleteResult { id = street.ID.ToString(), label = viadflib.Utils.Capitalize(street.FullName), value = viadflib.Utils.Capitalize(street.FullName) });
                    }
                    if (results.Count >= 10)
                    {
                        break;
                    }
                }
            }
            return Json(results, JsonRequestBehavior.AllowGet);
        }

        [OutputCache(Duration = 3600, VaryByParam = "id")]
        public ActionResult GetCrossings(int? id)
        {
            using (var context = new DataContext())
            {
                var results = context.StreetCrossings.Where(x => x.StreetID == (id ?? 0)).OrderBy(x => x.Name).ToList().Select(x => new AutoCompleteResult { label = x.Name + "", value = Utils.FormatCoordinates(x.Lat, x.Lng) }).ToList();
                results.ForEach(x => { x.label = viadflib.Utils.Capitalize(x.label); });
                return Json(results, JsonRequestBehavior.AllowGet);
            }
        }

        [OutputCache(Duration = 3600, VaryByParam = "de;a;count;format")]
        public ActionResult Search(string de, string a, int? count, string format)
        {
            SearchResultModel model = new SearchResultModel();
            try
            {
                Searcher searcher = new Searcher(ViaDFGraph.Instance);

                SearchPosition spFrom = SearchPosition.CreateSearchPosition(de, null);
                SearchPosition spTo = SearchPosition.CreateSearchPosition(a, null);

                if (spFrom != null && spTo != null)
                {
                    SearchParams sp = new SearchParams();
                    sp.StartSearch = spFrom;
                    sp.StartSearch.LoadNameFromDB();
                    sp.EndSearch = spTo;
                    sp.EndSearch.LoadNameFromDB();
                    sp.NrOfResults = count.HasValue ? (count.Value > 5 ? 5 : count.Value) : 1;

                    DataHandler.LogSearch(sp, Request.UserHostAddress);

                    List<SearchResult> results = searcher.DoSearch(sp);
                    model.Results = results;
                }
                else
                {
                    model.Results = new List<SearchResult>();
                }
            } 
            catch 
            {                
                model.Results = new List<SearchResult>();               
            }

            if (format == "xml")
            {
                using (var stream = new MemoryStream())
                {
                    new DataContractSerializer(typeof(SearchResultModel)).WriteObject(stream, model);
                    return Content(Encoding.UTF8.GetString(stream.ToArray()), "application/xml", Encoding.UTF8);                    
                }
            }
            else
            {
                using (var stream = new MemoryStream())
                {
                    new DataContractJsonSerializer(typeof(SearchResultModel)).WriteObject(stream, model);
                    return Content(Encoding.UTF8.GetString(stream.ToArray()), "application/json", Encoding.UTF8);
                }                
            }
        }

        [OutputCache(Duration = 3600, VaryByParam = "lat;lng;t")]
        public ActionResult GetMap(double lat, double lng, double t)
        {
            if (t > 10) t = 10;
            if (t < 0) t = 0;

            LatLng startPosition = new LatLng(lat, lng);

            var algorithm = new TravelTimeAlgorithm(new ViaDfDataProvider(startPosition));
            var result = algorithm.Calculcate(new LatLng(lat, lng), t);

            MapResultModel model = new MapResultModel();

            model.Stops = new List<MapResultModelItem>();
            model.Stops.Add(new MapResultModelItem() { Lat = result.Start.LatLng.Lat, Lng = result.Start.LatLng.Lng, Radius = (t / 60) * 5 * 1000 });
            foreach (var resultItem in result.Items)
            {
                var item = new MapResultModelItem()
                {
                    Lat = resultItem.LatLng.Lat,
                    Lng = resultItem.LatLng.Lng,
                    Radius = ((t - resultItem.TimePassed) / 60) * 5 * 1000,
                    Name = resultItem.TravelType,
                    PrevLat = resultItem.FromLatLng != null ? (double?)resultItem.FromLatLng.Lat : null,
                    PrevLng = resultItem.FromLatLng != null ? (double?)resultItem.FromLatLng.Lng : null
                };

                if (!model.Stops.Any(x => Math.Abs(x.Lat - item.Lat) + Math.Abs(x.Lng - item.Lng) < 0.0004))
                {
                    model.Stops.Add(item);
                }
            }


            return Json(model, JsonRequestBehavior.AllowGet);
        }
    }
}
