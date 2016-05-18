using System;
using System.Collections.Generic;
using System.Linq;

namespace viadflib
{
    public class DataHandler
    {
        public static List<Route> GetConnectionRoutes(RoutePiece piece, double maxDistanceKm)
        {
            double maxDistanceDegrees = maxDistanceKm * ViaDFGraph.KM_IN_DEGREES;

            using (DataContext context = new DataContext())
            {
                var dlo = new System.Data.Linq.DataLoadOptions();
                dlo.LoadWith<Route>(x => x.Type);
                context.LoadOptions = dlo;

                List<Route> routes = context.RoutePieces.Where(x => x.RouteID != piece.RouteID && Math.Abs(x.Lat - piece.Lat) + Math.Abs(x.Lng - piece.Lng) < maxDistanceDegrees).Select(x => x.Route).Distinct().ToList();
                return routes;
            }
        }

        public static Route GetNextRouteToAccept()
        {
            using (DataContext context = new DataContext())
            {
                return context.Routes.FirstOrDefault(x => x.Status == (int)StatusEnum.New);
            }
        }
       

        public static Route GetRoute(int id)
        {
            using (DataContext context = new DataContext())
            {
                var dlo = new System.Data.Linq.DataLoadOptions();
                dlo.LoadWith<Route>(x => x.Type);
                context.LoadOptions = dlo;

                return context.Routes.FirstOrDefault(x => x.ID == id);
            }
        }

        public static RoutePiece GetRoutePiece(int id)
        {
            using (var context = new DataContext())
            {
                var dlo = new System.Data.Linq.DataLoadOptions();
                dlo.LoadWith<RoutePiece>(x => x.Route);
                dlo.LoadWith<Route>(x => x.Type);
                context.LoadOptions = dlo;

                return context.RoutePieces.FirstOrDefault(x => x.ID == id);
            }
        }

        

        public static List<RoutePiece> GetRoutePieces(int routeID)
        {
            using (var context = new DataContext())
            {
                var dlo = new System.Data.Linq.DataLoadOptions();
                dlo.LoadWith<RoutePiece>(x => x.Route);
                dlo.LoadWith<Route>(x => x.Type);
                context.LoadOptions = dlo;

                return context.RoutePieces.Where(x => x.RouteID == routeID).OrderBy(x => x.ID).ToList();
            }
        }

        

        public static string GetNameAtPosition(double lat, double lng, double maxDistance = ViaDFGraph.M100_IN_DEGREES)
        {
            string poiName = GetPOIName(lat, lng);
            if (!string.IsNullOrWhiteSpace(poiName))
            {
                return poiName;
            }

            StreetCrossing crossing = GetCrossing(lat, lng, maxDistance);
            if (crossing != null)
            {
                return Utils.Capitalize(crossing.Street.Name) + " y " + Utils.Capitalize(crossing.Street1.Name);
            }
            return null;
        }

        private static List<StreetCrossing> AllCrossings;
        public static StreetCrossing GetCrossingInMemory(double lat, double lng, double maxDistance = ViaDFGraph.M100_IN_DEGREES)
        {
            if (AllCrossings == null)
            {
                using (var context = new DataContext())
                {
                    var dlo = new System.Data.Linq.DataLoadOptions();
                    dlo.LoadWith<StreetCrossing>(x => x.Street);
                    dlo.LoadWith<StreetCrossing>(x => x.Street1);
                    context.LoadOptions = dlo;
                    AllCrossings = new DataContext().StreetCrossings.ToList();
                }
            }

            StreetCrossing crossing = AllCrossings.Aggregate((curmin, x) => (curmin == null || Math.Abs(x.Lat - lat) + Math.Abs(x.Lng - lng) < Math.Abs(curmin.Lat - lat) + Math.Abs(curmin.Lng - lng) ? x : curmin));
            if (crossing != null && Math.Abs(crossing.Lat - lat) + Math.Abs(crossing.Lng - lng) < maxDistance)
            {
                return crossing;
            }

            return null;
        }

        public static string GetPOIName(double lat, double lng)
        {
            double e = 0.00000001;
            using (var context = new DataContext())
            {
                var dlo = new System.Data.Linq.DataLoadOptions();
                dlo.LoadWith<RoutePiece>(x => x.Route);
                dlo.LoadWith<Route>(x => x.Type);
                context.LoadOptions = dlo;

                var poi = context.POIs.FirstOrDefault(x => x.Lat > lat - e && x.Lat < lat + e && x.Lng > lng - e && x.Lng < lng + e);
                if (poi == null)
                {
                    var piece = context.RoutePieces.FirstOrDefault(x => x.Name != null && x.Lat > lat - e && x.Lat < lat + e && x.Lng > lng - e && x.Lng < lng + e);
                    if (piece != null)
                    {
                        if (piece.Route.Type.HasNamedStationList)
                        {
                            return piece.Route.Type.Name + " " + piece.Name;
                        }
                        else
                        {
                            return piece.Name;
                        }
                    }
                }
                else
                {
                    return poi.Name;
                }
            }
            return null;
        }

        public static void MarkRoute(int id)
        {
            using (var context = new DataContext())
            {
                Mail mail = new Mail();
                mail.Subject = "MarkRoute";
                mail.Name = id + "";
                context.Mails.InsertOnSubmit(mail);
                context.SubmitChanges();            
            }
        }

        public static StreetCrossing GetCrossing(double lat, double lng, double maxDistance = ViaDFGraph.M100_IN_DEGREES)
        {
            using (var context = new DataContext())
            {
                var dlo = new System.Data.Linq.DataLoadOptions();
                dlo.LoadWith<StreetCrossing>(x => x.Street);
                dlo.LoadWith<StreetCrossing>(x => x.Street1);
                context.LoadOptions = dlo;

                StreetCrossing crossing = context.ExecuteQuery<StreetCrossing>("SELECT TOP 1 * FROM StreetCrossing ORDER BY ABS(Lat - {0}) + ABS(Lng - {1}) ", lat, lng).FirstOrDefault();
                if (crossing != null && Math.Abs(crossing.Lat - lat) + Math.Abs(crossing.Lng - lng) < maxDistance)
                {
                    return crossing;
                }
                return null;
            }
        }

        public static void EnrichRoutePieces()
        {
            Random rand = new Random();

            using (var context = new DataContext())
            {
                foreach (var item in context.RoutePieces)
                {
                    var crossing = GetCrossingInMemory(item.Lat, item.Lng);
                    item.StreetCrossingID = crossing == null ? (int?)null : crossing.ID;

                    //save each 100 changes
                    if (rand.NextDouble() < 0.01)
                    {
                        context.SubmitChanges();
                    }
                }
                context.SubmitChanges();
            }
        }

        public static List<string> GetRouteStreets(int id)
        {
            List<string> result = new List<string>();

            using (var context = new DataContext())
            {
                var dlo = new System.Data.Linq.DataLoadOptions();
                dlo.LoadWith<RoutePiece>(x => x.StreetCrossing);
                dlo.LoadWith<StreetCrossing>(x => x.Street);
                dlo.LoadWith<StreetCrossing>(x => x.Street1);
                context.LoadOptions = dlo;


                Street lastStreet1 = null;
                Street lastStreet2 = null;
                int lastRoutePieceId = 0;
                int lastValidRoutePieceId = 0;

                Street sameStreet = null;
                int sameCount = 0;
                int sameStartRoutePieceID = 0;

                //deep load pieces with street information
                List<RoutePiece> pieces = context.RoutePieces.Where(x => x.RouteID == id).OrderBy(x => x.ID).ToList();

                foreach (RoutePiece piece in pieces)
                {
                    var crossing = piece.StreetCrossing;
                    if (crossing != null)
                    {
                        Street street1 = crossing.Street;
                        Street street2 = crossing.Street1;

                        if (lastStreet1 != null && lastStreet2 != null)
                        {
                            Street newSameStreet = null;
                            if (street1.Name == lastStreet1.Name || street1.Name == lastStreet2.Name)
                            {
                                newSameStreet = street1;
                            }
                            else if (street2.Name == lastStreet1.Name || street2.Name == lastStreet2.Name)
                            {
                                newSameStreet = street2;
                            }

                            if (sameStreet != null && newSameStreet != null && newSameStreet.Name == sameStreet.Name)
                            {
                                sameCount++;
                                lastValidRoutePieceId = piece.ID;
                            }
                            else
                            {
                                if (newSameStreet != null)
                                {
                                    if (sameCount >= 3)
                                    {
                                        //RouteStreetIndex index = new RouteStreetIndex();
                                        //index.RouteID = id;
                                        //index.StreetID = sameStreet.ID;
                                        //index.FromRoutePieceID = sameStartRoutePieceID;
                                        //index.ToRoutePieceID = lastValidRoutePieceId;
                                        if (result.Count == 0 || result.Last() != sameStreet.Name)
                                        {
                                            result.Add(sameStreet.Name);
                                        }
                                    }
                                    sameStartRoutePieceID = lastRoutePieceId;
                                    lastValidRoutePieceId = 0;
                                    sameStreet = newSameStreet;
                                    sameCount = 2;
                                }
                            }
                        }

                        lastStreet1 = street1;
                        lastStreet2 = street2;
                        lastRoutePieceId = piece.ID;
                    }
                }

                if (sameCount >= 3)
                {
                    result.Add(sameStreet.Name);
                }
            }
            return result;
        }

        public static List<Colonia> GetRouteColonias(int id)
        {
            using (var context = new DataContext())
            {
                var dlo = new System.Data.Linq.DataLoadOptions();
                dlo.LoadWith<Colonia>(x => x.Delegacion);
                context.LoadOptions = dlo;

                return context.ExecuteQuery<Colonia>("SELECT DISTINCT Colonia.* FROM ((Colonia INNER JOIN Street ON Street.ColoniaID = Colonia.ID) INNER JOIN StreetCrossing ON StreetCrossing.StreetID = Street.ID) INNER JOIN RoutePiece ON RoutePiece.StreetCrossingID = StreetCrossing.ID WHERE RouteID = {0}", id).ToList().OrderBy(x => x.Delegacion.Name).ThenBy(x => x.Name).ToList();
            }
        }

        public static void ClearIndex()
        {
            using (var context = new DataContext())
            {
                context.ExecuteCommand("TRUNCATE TABLE SearchIndex");
                foreach (var route in context.Routes.Where(x => x.Status == (int)StatusEnum.ActiveAndIndexed))
                {
                    route.Status = (int)StatusEnum.Active;
                    context.SubmitChanges();
                }
            }
            ViaDFGraph.SetReloadFlag();
        }

        public static void RefreshIndex()
        {
            ViaDFGraph.SetReloadFlag();
        }

        public static int GetRouteCount()
        {
            using (var context = new DataContext())
            {
                return context.Routes.Where(x => x.Status != (int)StatusEnum.New).Count();
            }
        }

        public static void DeleteRoutePiecesAndDeactivate(int id)
        {
            DeactivateRoute(id);

            using (var context = new DataContext())
            {
                var route = context.Routes.FirstOrDefault(x => x.ID == id);
                if (route != null)
                {
                    var pieces = route.RoutePieces.ToList();
                    while (pieces.Count > 0)
                    {
                        var piecesToDelete = pieces.Take(50).ToList();
                        pieces.RemoveAll(x => piecesToDelete.Contains(x));
                        context.RoutePieces.DeleteAllOnSubmit(piecesToDelete);
                        context.SubmitChanges();
                    }                    
                }
            }
        }

        public static void DuplicateRoute(int id)
        {
            using (var context = new DataContext())
            {
                context.ObjectTrackingEnabled = false;
                var route = context.Routes.FirstOrDefault(x => x.ID == id);
                if (route != null)
                {
                    var routePieces = context.RoutePieces.Where(x => x.RouteID == route.ID).ToList();

                    using (var insertContext = new DataContext())
                    {
                        route.Name = route.Name + " copia";
                        route.Status = (int)StatusEnum.Active;
                        insertContext.Routes.InsertOnSubmit(route);
                        insertContext.SubmitChanges();

                        foreach (var piece in routePieces)
                        {
                            bool isSplitPiece = route.SplitRoutePieceID.HasValue && route.SplitRoutePieceID.Value == piece.ID;

                            piece.RouteID = route.ID;
                            insertContext.RoutePieces.InsertOnSubmit(piece);
                            insertContext.SubmitChanges();

                            if (isSplitPiece)
                            {
                                route.SplitRoutePieceID = piece.ID;
                                insertContext.SubmitChanges();
                            }
                        }
                    }
                }
            }

            
        }

        public static void DeleteRoute(int id)
        {
            DeleteRoutePiecesAndDeactivate(id);

            using (var context = new DataContext())
            {
                var route = context.Routes.FirstOrDefault(x => x.ID == id);
                if (route != null)
                {                    
                    context.Routes.DeleteOnSubmit(route);
                    context.SubmitChanges();
                }
            }
        }

        public static void DeactivateRoute(int id)
        {
            bool needsReindex = false;

            using (var context = new DataContext())
            {
                var route = context.Routes.FirstOrDefault(x => x.ID == id);
                if (route != null)
                {                  
                    if (route.Status == (int)StatusEnum.ActiveAndIndexed)
                    {
                        // remove from search index
                        needsReindex = true;

                        var routePieces = route.RoutePieces.ToList();
                        while (routePieces.Count > 0)
                        {
                            var routePiecesToProcess = routePieces.Take(200).ToList();
                            routePieces.RemoveAll(x => routePiecesToProcess.Contains(x));
                            List<SearchIndex> indexes = context.SearchIndexes.Where(x => routePiecesToProcess.Select(y => y.ID).Contains(x.RoutePieceID) || routePiecesToProcess.Select(y => y.ID).Contains(x.RoutePiece2ID)).ToList();
                            context.SearchIndexes.DeleteAllOnSubmit(indexes);
                            context.SubmitChanges();
                        }
                    }

                    route.Status = (int)StatusEnum.New;
                    context.SubmitChanges();
                }
            }

            if (needsReindex)
            {
                ViaDFGraph.SetReloadFlag();
            }
        }

        public static void AcceptRoute(int id)
        {
            using (var context = new DataContext())
            {
                var route = context.Routes.FirstOrDefault(x => x.ID == id);
                if (route.ParentRouteID.HasValue)
                {
                    DeleteRoute(route.ParentRouteID.Value);                    
                }
                ActivateRoute(id);
            }
        }

        /// <summary>
        /// Sets route active on web - doesn't do indexing!!
        /// </summary>
        /// <param name="id"></param>
        public static void ActivateRoute(int id)
        {
            using (var context = new DataContext())
            {
                var dlo = new System.Data.Linq.DataLoadOptions();
                dlo.LoadWith<Route>(x => x.RoutePieces);
                dlo.LoadWith<Route>(x => x.Type);
                context.LoadOptions = dlo;

                var route = context.Routes.FirstOrDefault(x => x.ID == id);
                if (route != null && route.Status != (int)StatusEnum.ActiveAndIndexed)
                {
                    // recalc streetcrossing ids
                    foreach (var routePiece in route.RoutePieces)
                    {
                        StreetCrossing crossing = DataHandler.GetCrossing(routePiece.Lat, routePiece.Lng);
                        routePiece.StreetCrossingID = crossing == null ? (int?)null : crossing.ID;
                    }

                    route.Status = (int)StatusEnum.Active;

                    context.SubmitChanges();
                }
            }
        }

        public static List<Type> GetEditableTypes()
        {
            using (var context = new viadflib.DataContext())
            {
                return context.Types.Where(x => x.ShowInWeb && !x.IsWalkingType).OrderBy(x => x.ID).ToList();
            }
        }

        public static List<Route> GetAllRoutes(StatusEnum status)
        {
            using (var context = new viadflib.DataContext())
            {
                return context.Routes.Where(x => x.Status == (int)status).OrderBy(x => x.ID).ToList();
            }
        }

        public static List<Route> GetAllRoutesWithPieces(Type type)
        {
            using (var context = new viadflib.DataContext())
            {
                var dlo = new System.Data.Linq.DataLoadOptions();
                dlo.LoadWith<Route>(x => x.RoutePieces);
                context.LoadOptions = dlo;
                return context.Routes.Where(x => x.Status > (int)StatusEnum.New && x.TypeID == type.ID).OrderBy(x => x.ID).ToList();
            }
        }

        public static List<Route> GetAllRoutesForIds(List<int> routeIds)
        {
            if (routeIds.Count == 0)
            {
                return new List<Route>();
            }

            using (var context = new viadflib.DataContext())
            {
                var dlo = new System.Data.Linq.DataLoadOptions();
                dlo.LoadWith<Route>(x => x.Type);
                context.LoadOptions = dlo;
                return context.Routes.Where(x => routeIds.Contains(x.ID)).ToList();
            }
        }

        public static void LogSearch(SearchParams sp, string ipAddress)
        {
            using (var context = new DataContext())
            {
                SearchHistory history = new SearchHistory();
                history.CreateDate = DateTime.Now;
                history.FromLat = sp.StartSearch.Lat;
                history.FromLng = sp.StartSearch.Lng;
                history.FromName = sp.StartSearch.Name;
                history.ToLat = sp.EndSearch.Lat;
                history.ToLng = sp.EndSearch.Lng;
                history.ToName = sp.EndSearch.Name;
                history.IpAdress = ipAddress ?? "";
                context.SearchHistories.InsertOnSubmit(history);
                context.SubmitChanges();
            }
        }

        public static void WriteException(Exception ex, string ipAddress)
        {
            using (var context = new DataContext())
            {
                SystemException exception = new SystemException();
                exception.CreateDate = DateTime.Now;
                exception.IpAdress = ipAddress;
                exception.Name = ex.Message ?? ""; ;
                exception.StackTrace = ex.StackTrace ?? "";
                context.SystemExceptions.InsertOnSubmit(exception);
                context.SubmitChanges();
            }
        }

        public static List<List<Route>> FindDuplicates()
        {
            List<List<Route>> lists = new List<List<Route>>();
            using (var context = new DataContext())
            {
                var dlo = new System.Data.Linq.DataLoadOptions();
                dlo.LoadWith<Route>(x => x.RoutePieces);
                context.LoadOptions = dlo;

                List<Route> allRoutes = context.Routes.ToList();

                foreach (Route route in allRoutes)
                {
                    if (!lists.Any(x => x.Any(y => y.ID == route.ID)))
                    {
                        List<Route> duplicates = new List<Route>();
                        duplicates.Add(route);
                        foreach (Route route2 in allRoutes)
                        {
                            if (route.ID != route2.ID && route.TypeID == route2.TypeID) {
                                double similarity = CalculateSimilarity(route, route2);
                                if (similarity > 0.95) {
                                    duplicates.Add(route2);
                                }
                            }
                        }
                        if (duplicates.Count > 1)
                        {
                            lists.Add(duplicates);
                        }
                    }                    
                }
            }
            return lists;
        }

        public static double CalculateSimilarity(Route route1, Route route2)
        {
            double countMatch = 0;
            foreach (var rp1 in route1.RoutePieces)
            {
                double closestMatch = double.MaxValue;
                foreach (var rp2 in route2.RoutePieces)
                {
                    double distance = Math.Abs(rp1.Lat - rp2.Lat) + Math.Abs(rp1.Lng - rp2.Lng);
                    if (distance < closestMatch)
                    {
                        closestMatch = distance;
                    }

                }
                if (closestMatch < ViaDFGraph.M100_IN_DEGREES)
                {
                    countMatch++;
                }
            }

            return countMatch / route1.RoutePieces.Count;
        }
    }
}
