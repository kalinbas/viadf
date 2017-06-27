using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using viadflib;
using System.Text;
using System.IO;
using viadflib.TravelTime;
using SearchResultModel = viadflib.SearchResultModel;
using System.Runtime.Serialization.Json;
using viadflib.TravelTime.Utils;
using System.Web.Http.Cors;
using System.Net;
using Newtonsoft.Json;
using viadfweb.Models;

namespace viadf.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
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

        [OutputCache(Duration = 3600, VaryByParam = "de;a;count;format;key")]
        public ActionResult Search(string de, string a, int? count, string key)
        {
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            try
            {
                var error = DataHandler.ChargeServiceCall("/Service/Search", key, Request.UserHostAddress, de + "|" + a + "|" + count);

                if (error == null)
                {
                    int c = count ?? 1;
                    if (c > 5) c = 5;
                    if (c < 1) c = 1;

                    SearchResultModel model = new SearchResultModel();

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
                        sp.NrOfResults = c;

                        DataHandler.LogSearch(sp, Request.UserHostAddress);

                        List<SearchResult> results = searcher.DoSearch(sp);
                        model.Results = results;
                    }
                    else
                    {
                        model.Results = new List<SearchResult>();
                    }

                    using (var stream = new MemoryStream())
                    {
                        new DataContractJsonSerializer(typeof(SearchResultModel)).WriteObject(stream, model);
                        return Content(Encoding.UTF8.GetString(stream.ToArray()), "application/json", Encoding.UTF8);
                    }
                }
                else
                {
                    return Json(new { error = error }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                DataHandler.WriteException(ex, Request.UserHostAddress);
                return Json(new { error = "Internal server error" }, JsonRequestBehavior.AllowGet);
            }
        }


        [OutputCache(Duration = 3600, VaryByHeader = "start;time;type;key")]
        public ActionResult TravelTimePolygon(string start, double time, TravelTimeType type, string key)
        {
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            try
            {
                var error = DataHandler.ChargeServiceCall("/Service/TravelTimePolygon", key, Request.UserHostAddress, start + "|" + time + "|" + (int)type);

                if (error == null)
                {
                    LatLng startLatLng = new LatLng(start);

                    if (time < 0) time = 0;
                    if (time > 45) time = 45;
                    if (type != TravelTimeType.PublicTransport && type != TravelTimeType.Walking)
                    {
                        type = TravelTimeType.Walking;
                    }

                    var result = Singleton<TravelTimeEngine>.Instance.GetPolygon(type, startLatLng, time);
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { error = error }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                DataHandler.WriteException(ex, Request.UserHostAddress);
                return Json(new { error = "Internal server error" }, JsonRequestBehavior.AllowGet);
            }
        }

        [OutputCache(Duration = 3600, VaryByHeader = "start;points;type;key")]
        public ActionResult OrderByTravelTime(string start, string points, TravelTimeType type, string key)
        {
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            try
            {
                var error = DataHandler.ChargeServiceCall("/Service/OrderByTravelTime", key, Request.UserHostAddress, start + "|" + points + "|" + (int)type);

                if (error == null)
                {
                    LatLng startLatLng = new LatLng(start);
                    List<LatLng> pointsLatLng = points.Split(';').Select(ll => new LatLng(ll)).ToList();

                    // max points allowed -> 100
                    if (pointsLatLng.Count > 100)
                    {
                        pointsLatLng = pointsLatLng.Take(100).ToList();
                    }

                    var result = Singleton<TravelTimeEngine>.Instance.GetOrderPointList(type, startLatLng, pointsLatLng);

                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { error = error }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                DataHandler.WriteException(ex, Request.UserHostAddress);
                return Json(new { error = "Internal server error" }, JsonRequestBehavior.AllowGet);
            }
        }

        [OutputCache(Duration = 3600, VaryByHeader = "start;key")]
        public ActionResult GetCloseRoutes(string start, string key)
        {
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            try
            {
                var error = DataHandler.ChargeServiceCall("/Service/GetCloseRoutes", key, Request.UserHostAddress, start);

                if (error == null)
                {
                    LatLng startLatLng = new LatLng(start);

                    var routes = DataHandler.GetConnectionRoutes(startLatLng.Lat, startLatLng.Lng, 0.5);

                    var jsonRoutes = routes.Select(r => new { ID = r.ID, TypeName = r.Type.Name, FromName = r.FromName, ToName = r.ToName, Name = r.Name });

                    return Json(jsonRoutes, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { error = error }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                DataHandler.WriteException(ex, Request.UserHostAddress);
                return Json(new { error = "Internal server error" }, JsonRequestBehavior.AllowGet);
            }
        }

        // only cache short time 
        [OutputCache(Duration = 30, VaryByHeader = "start;key")]
        public ActionResult GetCloseRoutesRealTime(string start, string key)
        {
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            try
            {
                var error = DataHandler.ChargeServiceCall("/Service/GetCloseRoutesRealTime", key, Request.UserHostAddress, start);

                if (error == null)
                {
                    LatLng startLatLng = new LatLng(start);

                    var routes = DataHandler.GetConnectionRoutes(startLatLng.Lat, startLatLng.Lng, 0.25);

                    routes = routes.Where(x => x.TypeID == (int)TypeEnum.Metro || x.TypeID == (int)TypeEnum.Metrobus).ToList();

                    var results = new List<GetCloseRoutesRealTimeResult>();

                    foreach (var route in routes)
                    {
                        var pieces = DataHandler.GetRoutePieces(route.ID);

                        // get closest piece
                        var piece = pieces.Aggregate((curMin, x) => ((curMin == null || Math.Abs(startLatLng.Lat - x.Lat) + Math.Abs(startLatLng.Lng - x.Lng) < Math.Abs(startLatLng.Lat - curMin.Lat) + Math.Abs(startLatLng.Lng - curMin.Lng)) ? x : curMin));

                        // get next & previous piece
                        var previousPiece = route.SplitRoutePieceID.HasValue ? null : pieces.Aggregate((RoutePiece)null, (curMin, x) => x.ID < piece.ID && (curMin == null || x.ID > curMin.ID) ? x : curMin);
                        var previous2Piece = route.SplitRoutePieceID.HasValue ? null : pieces.Aggregate((RoutePiece)null, (curMin, x) => x.ID < piece.ID - 1 && (curMin == null || x.ID > curMin.ID) ? x : curMin);
                        var previous3Piece = route.SplitRoutePieceID.HasValue ? null : pieces.Aggregate((RoutePiece)null, (curMin, x) => x.ID < piece.ID - 2 && (curMin == null || x.ID > curMin.ID) ? x : curMin);
                        if (previous2Piece != null) previousPiece = previous2Piece;
                        if (previous3Piece != null) previousPiece = previous3Piece;

                        var nextPiece = pieces.Aggregate((RoutePiece)null, (curMin, x) => x.ID > piece.ID && (curMin == null || x.ID < curMin.ID) ? x : curMin);
                        var next2Piece = pieces.Aggregate((RoutePiece)null, (curMin, x) => x.ID > piece.ID + 1 && (curMin == null || x.ID < curMin.ID) ? x : curMin);
                        var next3Piece = pieces.Aggregate((RoutePiece)null, (curMin, x) => x.ID > piece.ID + 2 && (curMin == null || x.ID < curMin.ID) ? x : curMin);
                        if (next2Piece != null) nextPiece = next2Piece;
                        if (next3Piece != null) nextPiece = next3Piece;

                        if (piece.ID == pieces.Max(x => x.ID) && route.SplitRoutePieceID.HasValue)
                        {
                            nextPiece = pieces.First();
                        }

                        if (previousPiece != null)
                        {
                            var times = CrawlNextMoovitTimes(piece.Lat, piece.Lng, previousPiece.Lat, previousPiece.Lng);
                            if (times != null && times.Length > 1)
                            {
                                var closest = times[0];
                                while (closest - (times[1] - times[0]) > GetCurrentMillis())
                                {
                                    closest -= (times[1] - times[0]);
                                }
                                results.Add(new GetCloseRoutesRealTimeResult() { route = route.Type.Name + " " + route.Name, direction = route.FromName, time = closest, timeLeft = ToLocalTime(closest) });
                            }
                        }
                        if (nextPiece != null)
                        {
                            var times = CrawlNextMoovitTimes(piece.Lat, piece.Lng, nextPiece.Lat, nextPiece.Lng);
                            if (times != null && times.Length > 1)
                            {
                                var closest = times[0];
                                while (closest - (times[1] - times[0]) > GetCurrentMillis())
                                {
                                    closest -= (times[1] - times[0]);
                                }
                                results.Add(new GetCloseRoutesRealTimeResult() { route = route.Type.Name + " " + route.Name, direction = route.SplitRoutePieceID.HasValue && nextPiece.ID > route.SplitRoutePieceID.Value ? route.FromName : route.ToName, time = closest, timeLeft = ToLocalTime(closest) });
                            }
                        }
                    }

                    return Json(results.OrderBy(x => x.time).ToList(), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { error = error }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                DataHandler.WriteException(ex, Request.UserHostAddress);
                return Json(new { error = "Internal server error" }, JsonRequestBehavior.AllowGet);
            }
        }

        [OutputCache(Duration = 3600, VaryByHeader = "id;key")]
        public ActionResult GetRoutePath(int id, string key)
        {
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            try
            {
                var error = DataHandler.ChargeServiceCall("/Service/GetRoutePath", key, Request.UserHostAddress, id + "");

                if (error == null)
                {
                    var pieces = DataHandler.GetRoutePieces(id);
                    var route = pieces[0].Route;
                    var jsonRoute = new
                    {
                        Path = pieces.Where(x => !route.SplitRoutePieceID.HasValue || x.ID < route.SplitRoutePieceID).Select(x => x.Lat + "," + x.Lng).ToList(),
                        ReturnPath = pieces.Where(x => route.SplitRoutePieceID.HasValue && x.ID >= route.SplitRoutePieceID).Select(x => x.Lat + "," + x.Lng).ToList(),
                    };

                    return Json(jsonRoute, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { error = error }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                DataHandler.WriteException(ex, Request.UserHostAddress);
                return Json(new { error = "Internal server error" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetCredits(string key)
        {
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            try
            {
                var error = DataHandler.ChargeServiceCall("/Service/GetCredits", key, Request.UserHostAddress, null);

                if (error == null)
                {
                    using (var context = new DataContext())
                    {
                        var serviceClient = context.ServiceClients.FirstOrDefault(x => x.ApiKey == key);
                        return Json(new { availableCredits = serviceClient.Credits }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { error = error }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                DataHandler.WriteException(ex, Request.UserHostAddress);
                return Json(new { error = "Internal server error" }, JsonRequestBehavior.AllowGet);
            }
        }

        [OutputCache(Duration = 3600, VaryByParam = "minLatLng;maxLatLng")]
        public ActionResult GetRoutesInArea(string minLatLng, string maxLatLng)
        {
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            try
            {
                SearchPosition min = SearchPosition.CreateSearchPosition(minLatLng, null);
                SearchPosition max = SearchPosition.CreateSearchPosition(maxLatLng, null);

                if (min != null && max != null)
                {
                    var routes = DataHandler.GetRoutesInArea(min.Lat, min.Lng, max.Lat, max.Lng);
                    var jsonRoutes = routes.Select(r => new { ID = r.ID, TypeName = r.Type.Name, FromName = r.FromName, ToName = r.ToName, Name = r.Name });

                    return Json(jsonRoutes, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { error = "Wrong parameters" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                DataHandler.WriteException(ex, Request.UserHostAddress);
                return Json(new { error = "Internal server error" }, JsonRequestBehavior.AllowGet);
            }
        }


        private long[] CrawlNextMoovitTimes(double fromLat, double fromLng, double toLat, double toLng)
        {
            using (WebClient client = new WebClient())
            {
                client.Headers.Add("MOOVIT_APP_TYPE", "WEB_TRIP_PLANNER");
                client.Headers.Add("MOOVIT_CLIENT_VERSION", "4.14.0.0");
                client.Headers.Add("MOOVIT_METRO_ID", "822");
                client.Headers.Add("MOOVIT_USER_KEY", "F39338");


                var url = string.Format("https://moovitapp.com/api/route/search?fromLocation_latitude={0}&fromLocation_longitude={1}&fromLocation_type=6&isCurrentTime=true&routeTypes=3,2,1,0,5&timeType=2&toLocation_latitude={2}&toLocation_longitude={3}&toLocation_type=6&tripPlanPref=2", CoordsToInt(fromLat), CoordsToInt(fromLng), CoordsToInt(toLat), CoordsToInt(toLng)); // start 10 minutes before
                var resp = client.DownloadString(url);
                dynamic json = JsonConvert.DeserializeObject(resp);
                if (json.token != null)
                {
                    var url2 = string.Format("https://moovitapp.com/api/route/result?offset=0&token={0}", json.token);
                    var resp2 = client.DownloadString(url2);
                    dynamic json2 = JsonConvert.DeserializeObject(resp2);

                    if (json2.results != null)
                    {
                        foreach (dynamic res in json2.results)
                        {
                            if (res.result.itinerary != null && res.result.itinerary.legs != null)
                            {
                                foreach (dynamic leg in res.result.itinerary.legs)
                                {
                                    if (leg.waitToLineLeg != null && leg.waitToLineLeg.futureDepartureTimes != null)
                                    {
                                        // TODO return content of leg.waitToLineLeg.serviceAlert as well

                                        return leg.waitToLineLeg.futureDepartureTimes.ToObject<long[]>();
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        private int CoordsToInt(double coord)
        {
            return (int)(coord * 1000000);
        }   

        private string ToLocalTime(long localMs)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified).AddMilliseconds(localMs);

            TimeZoneInfo centralStandardTime = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)");
            DateTime utcTime = TimeZoneInfo.ConvertTimeToUtc(dtDateTime, centralStandardTime);

            //DateTimeOffset timeInCST = TimeZoneInfo.ConvertTime(dtDateTime, centralStandardTime);
            double mins = utcTime.Subtract(DateTime.UtcNow).TotalMinutes;
            return (mins < 1 ? "< " : "") + Math.Max(1, (int)Math.Floor(mins)) + " minuto" + (mins >= 2 ? "s" : "");
        }

        public long GetCurrentMillis()
        {
            TimeZoneInfo centralStandardTime = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)");
            DateTime dateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, centralStandardTime);

            return (long)(dateTime - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)).TotalMilliseconds;
        }
    }
}
