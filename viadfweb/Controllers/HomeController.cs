using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using viadflib;
using viadf.Models;
using viadflib.TravelTime;
using viadflib.TravelTime.Utils;

namespace viadf.Controllers
{
    public class AutoCompleteResult
    {
        public string id { get; set; }
        public string label { get; set; }
        public string value { get; set; }
    }


    public class HomeController : ControllerBase
    {
        //
        // GET: /Default/

        public ActionResult Index()
        {
            SetSEO("¿Cómo llegar en transporte público? - Ciudad de México", "Busca conexiones de toda la red del Metro, Metrobús, Tren Ligero, Trolebús, RTP, Autobús, Microbús, Pumabús y Tren Suburbano.", "buscador, buscar rutas, planeador, como llego, como llegar, como voy, transporte publico, mexico, distrito federal, metro, metrobus, trolebus, pesero, microbus, tren ligero,  RTP, autobus, suburbano, pumabus, por donde");
            return View();
        }

        public ActionResult LegacyRouteList(string type)
        {
            return new PermanentRedirectResult(Url.Action("RouteList", new { type = type }));
        }
        public ActionResult LegacyRouteList2(string code)
        {
            return new PermanentRedirectResult(Url.Action("RouteList", new { type = code }));
        }
        public ActionResult LegacyDefault()
        {
            return new PermanentRedirectResult(Url.Action("Index"));
        }
        public ActionResult ContentDeleted()
        {
            return new ContentDeletedResult();
        }

        public ActionResult AgregarRuta()
        {
            SetSEO("Crea una nueva ruta para ViaDF", "", "");
            ViewBag.NumberRoutes = DataHandler.GetRouteCount();
            return View();
        }

        public ActionResult CrearRuta()
        {
            return new PermanentRedirectResult(Url.Action("AgregarRuta"));
        }

        public ActionResult PoligonosTiempoRecorrido()
        {
            SetSEO("Polígonos de tiempo de recorrido", "Crear polígonos de tiempo de recorrido en un simple mapa con los datos de ViaDF.", "polígonos de tiempo");
            return View();
        }

        public ActionResult RutaRegistrada()
        {
            SetSEO("Ruta registrada", "", "");
            return View();
        }

        [OutputCache(Duration = 3600, VaryByParam = "type")]
        public ActionResult RouteList(string type)
        {
            RouteListModel model = new RouteListModel();
            using (var context = new viadflib.DataContext())
            {
                model.AllTypes = context.Types.Where(x => x.ShowInWeb).ToList();
                if (type != null)
                {
                    model.SelectedType = model.AllTypes.FirstOrDefault(x => x.SeoName == (type ?? "").ToLower());
                    if (model.SelectedType != null)
                    {
                        model.Routes = context.Routes.Where(x => x.TypeID == model.SelectedType.ID && x.Status > (int)StatusEnum.New).OrderBy(x => x.Name).ToList();
                    }
                    else
                    {
                        return HttpNotFound();
                    }
                }
                else
                {
                    model.Routes = new List<Route>();
                }
            }

            if (model.SelectedType != null)
            {
                SetSEO("Lista de rutas del " + model.SelectedType.Name + " de la Ciudad de México.", "Lista de todas las rutas del " + model.SelectedType.Name + "en la Ciudad de México y en el Estado de México.", model.SelectedType.Name + ", como llegar, como llego, lista rutas, transporte público, méxico, ciudad de méxico, cdmx, estado de méxico");
            }
            else
            {
                SetSEO("Directorio del transporte público de la Ciudad de México.", "Directorio de las rutas y estaciones del transporte público en la Ciudad de México y en el Estado de México.", "directorio, como llegar, como llego, lista rutas, transporte público, méxico, ciudad de méxico, cdmx, estado de méxico");
            }

            return View(model);
        }

        [OutputCache(Duration = 3600, VaryByParam = "type;name")]
        public ActionResult Route(int? id, string type, string name)
        {
            // legacy seo url redirect
            if (id.HasValue)
            {
                RouteData.Values.Remove("id");
                return new PermanentRedirectResult(Url.Action("Route", new { type = Utils.Capitalize(type), name = Utils.Capitalize(name) }));
            }

            // load type
            var typeObj = new viadflib.DataContext().Types.Where(x => x.SeoName == type.ToLower()).FirstOrDefault();

            if (typeObj != null)
            {
                // load route
                var routeObj = new viadflib.DataContext().Routes.Where(x => x.Status > (int)StatusEnum.New && (x.SeoName == name.ToLower() || x.OldSeoName == name.ToLower()) && x.TypeID == typeObj.ID).FirstOrDefault();

                if (routeObj != null)
                {
                    if (routeObj.OldSeoName == name.ToLower() && routeObj.SeoName != name.ToLower())
                    {
                        return RedirectToActionPermanent("Route", new { type = Utils.Capitalize(typeObj.SeoName), name = Utils.Capitalize(routeObj.SeoName) });
                    }

                    RouteModel model = new RouteModel();

                    model.Route = DataHandler.GetRoute(routeObj.ID);
                    model.RoutePieces = DataHandler.GetRoutePieces(routeObj.ID);
                    if (!model.Route.Type.HasNamedStationList)
                    {
                        model.RouteStreets = DataHandler.GetRouteStreets(routeObj.ID);
                        model.RouteColonias = DataHandler.GetRouteColonias(routeObj.ID);
                    }
                    if (model.Route.SplitRoutePieceID.HasValue)
                    {
                        model.RoutePieces1 = model.RoutePieces.Where(x => x.ID <= model.Route.SplitRoutePieceID).OrderBy(x => x.ID).ToList();
                        model.RoutePieces2 = model.RoutePieces.Where(x => x.ID >= model.Route.SplitRoutePieceID).OrderBy(x => x.ID).ToList();
                    }
                    else
                    {
                        model.RoutePieces1 = model.RoutePieces;
                    }

                    var routeName = model.Route.TypeID == (int)TypeEnum.Microbus ? model.Route.FromName + " a " + model.Route.ToName : "(" + model.Route.Name + ")";

                    SetSEO(model.Route.Type.Name + " " + routeName + " - ¿Cómo llegar en transporte público?", "", model.Route.Type.Name + "," + model.Route.Name);

                    return View(model);
                }
            }
            return HttpNotFound();
        }

        [OutputCache(Duration = 3600, VaryByParam = "type;route;name")]
        public ActionResult RoutePiece(int? id, string type, string route, string name)
        {
            // legacy seo url redirect
            if (id.HasValue)
            {
                RouteData.Values.Remove("id");
                return new PermanentRedirectResult(Url.Action("RoutePiece", new { type = type, route = route, name = name }));
            }

            // load type
            var typeObj = new viadflib.DataContext().Types.Where(x => x.SeoName == type.ToLower()).FirstOrDefault();

            if (typeObj != null)
            {
                // load route
                var routeObj = new viadflib.DataContext().Routes.Where(x => (x.SeoName == route.ToLower() || x.OldSeoName == route.ToLower()) && x.TypeID == typeObj.ID).FirstOrDefault();

                if (routeObj != null)
                {
                    var routePieceObj = new viadflib.DataContext().RoutePieces.Where(x => x.SeoName == name.ToLower() && x.RouteID == routeObj.ID).FirstOrDefault();

                    if (routePieceObj != null)
                    {
                        if (routeObj.OldSeoName == name.ToLower() && routeObj.SeoName != name.ToLower())
                        {
                            return RedirectToActionPermanent("RoutePiece", new { type = Utils.Capitalize(typeObj.SeoName), route = Utils.Capitalize(routeObj.SeoName), name = Utils.Capitalize(routePieceObj.SeoName) });
                        }

                        RoutePieceModel model = new RoutePieceModel();
                        model.RoutePiece = DataHandler.GetRoutePiece(routePieceObj.ID);
                        model.ConnectingRoutes = DataHandler.GetConnectionRoutes(model.RoutePiece, 0.5);

                        string routeName = model.RoutePiece.Route.Type.Name + " " + model.RoutePiece.Name;

                        model.SmallSearchBoxModel = new SmallSearchBoxModel { Name = routeName, Coordinates = Utils.FormatCoordinates(model.RoutePiece.Lat, model.RoutePiece.Lng) };

                        routeName += " (" + model.RoutePiece.Route.Name + ")";
                        SetSEO(routeName + " - ¿Cómo llegar en transporte público?", "", model.RoutePiece.Route.Type.Name + "," + model.RoutePiece.Name + "," + model.RoutePiece.Route.Name);

                        return View(model);
                    }
                }
            }

            return HttpNotFound();
        }

        [OutputCache(Duration = 3600, VaryByParam = "estadoId;name")]
        public ActionResult Delegacion(int estadoId, string name, int? id)
        {
            // legacy seo url redirect
            if (id.HasValue)
            {
                RouteData.Values.Remove("id");
                return new PermanentRedirectResult(Url.Action("Delegacion", new { estadoId = estadoId, name = name }));
            }

            // if none loaded - redirect to main page of state
            if (name == null)
            {
                if (estadoId == 1)
                {
                    return RedirectToAction("Delegacion", new { estadoId = estadoId, name = "Alvaro-Obregon" });
                }
                else
                {
                    return RedirectToAction("Delegacion", new { estadoId = estadoId, name = "Atizapan-De-Zaragoza" });
                }
            }

            // load delegacion
            var delegacion = new viadflib.DataContext().Delegacions.Where(x => x.SeoName == name.ToLower()).FirstOrDefault();

            if (delegacion != null)
            {
                DelegacionListModel model = new DelegacionListModel();

                model.Delegacion = delegacion;
                model.All = new viadflib.DataContext().Delegacions.Where(x => x.EstadoID == model.Delegacion.EstadoID).OrderBy(x => x.Name).ToList();
                model.Colonias = new viadflib.DataContext().Colonias.Where(x => x.DelegacionID == model.Delegacion.ID).OrderBy(x => x.Name).ToList();

                SetSEO("Transporte público en " + Utils.Capitalize(model.Delegacion.Name) + " - ¿Cómo llegar en transporte público?", "", Utils.Capitalize(model.Delegacion.Name));
                return View(model);
            }

            return HttpNotFound();
        }

        [OutputCache(Duration = 3600, VaryByParam = "estadoId;delegacion;name")]
        public ActionResult Colonia(int estadoId, string delegacion, string name, int? id)
        {
            // legacy seo url redirect
            if (id.HasValue)
            {
                RouteData.Values.Remove("id");
                return new PermanentRedirectResult(Url.Action("Colonia", new { estadoId = estadoId, delegacion = delegacion, name = name }));
            }

            // load delegacion
            var delegacionObj = new viadflib.DataContext().Delegacions.Where(x => x.SeoName == delegacion.ToLower()).FirstOrDefault();

            if (delegacionObj != null)
            {
                // load colonia
                var colonia = new viadflib.DataContext().Colonias.Where(x => x.SeoName == name.ToLower() && x.DelegacionID == delegacionObj.ID).FirstOrDefault();

                if (colonia != null)
                {
                    ColoniaRouteListModel model = new ColoniaRouteListModel();
                    model.Colonia = colonia;
                    model.Routes = new viadflib.DataContext().Routes.Where(x => x.RoutePieces.Count(y => y.StreetCrossing.Street.ColoniaID == model.Colonia.ID) > 0 && x.Status != (int)StatusEnum.New).ToList();
                    StreetCrossing firstCrossing = new viadflib.DataContext().StreetCrossings.Where(x => x.Street.ColoniaID == model.Colonia.ID).FirstOrDefault();
                    if (firstCrossing != null)
                    {
                        model.SmallSearchBoxModel = new SmallSearchBoxModel() { Name = Utils.Capitalize(model.Colonia.Name) + " en " + Utils.Capitalize(model.Colonia.Delegacion.Name), Coordinates = Utils.FormatCoordinates(firstCrossing.Lat, firstCrossing.Lng) }; ;
                    }

                    SetSEO("Transporte público en " + Utils.Capitalize(model.Colonia.Name) + " (" + Utils.Capitalize(model.Colonia.Delegacion.Name) + ") - ¿Cómo llegar en transporte público?", "", Utils.Capitalize(model.Colonia.Delegacion.Name) + "," + Utils.Capitalize(model.Colonia.Name));

                    return View(model);
                }
            }

            return HttpNotFound();
        }

        public ActionResult Condiciones()
        {
            SetSEO("Condiciones de uso", "", "");
            return View();
        }

        public ActionResult Privacidad()
        {
            SetSEO("Políticas de privacidad", "", "");
            return View();
        }

        public ActionResult WebService()
        {
            return RedirectToActionPermanent("Api");
        }

        public ActionResult Api()
        {
            SetSEO("REST API - Servicios web", "", "");
            return View();
        }

        public ActionResult OpenData()
        {
            SetSEO("Datos abiertos", "", "");
            return View();
        }

    }
}
