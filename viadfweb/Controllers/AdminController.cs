using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using viadflib;
using viadf.Models;

namespace viadf.Controllers
{
    public class AdminController : ControllerBase
    {
        public AdminController()
        {
            DisableAds();
        }

        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult FindDuplicates()
        {
            var duplicateList = DataHandler.FindDuplicates();
            return View(duplicateList);
        }

        [Authorize]
        public ActionResult ClearIndex()
        {
            Indexer.ClearIndex();
            return Content("Index cleared");
        }

        [Authorize]
        public ActionResult RebuildIndex(int timeout)
        {
            Indexer.RebuildIndex(timeout);
            return Content("Index rebuilt");
        }

        [Authorize]
        public ActionResult EnrichRoutePieces()
        {
            DataHandler.EnrichRoutePieces();
            return Content("Finished");
        }

        [Authorize]
        public ActionResult ActivateRoute(int id)
        {
            DataHandler.ActivateRoute(id);
            return RedirectToAction("Routes");
        }

        [Authorize]
        public ActionResult DeactivateRoute(int id)
        {
            DataHandler.DeactivateRoute(id);
            return RedirectToAction("Routes");
        }

        [Authorize]
        public ActionResult RefreshIndex()
        {
            DataHandler.RefreshIndex();
            return RedirectToAction("Index");
        }

        [Authorize]
        public ActionResult DuplicateRoute(int id)
        {
            DataHandler.DuplicateRoute(id);
            return RedirectToAction("Routes");
        }

        [Authorize]
        public ActionResult DeleteRoute(int id)
        {
            DataHandler.DeleteRoute(id);
            return RedirectToAction("Routes");
        }

        [Authorize]
        public ActionResult Routes(StatusEnum status = StatusEnum.New)
        {
            return View(DataHandler.GetAllRoutes(status));
        }


        [Authorize]
        public ActionResult AcceptarRuta(int? id)
        {
            CreateRouteModel model = new CreateRouteModel();
            model.TypeList = DataHandler.GetEditableTypes();

            Route route;
            if (id.HasValue)
            {
                route = DataHandler.GetRoute(id.Value);
            }
            else
            {
                route = DataHandler.GetNextRouteToAccept();
            }

            if (route != null)
            {
                FillRouteModel(route, model);
                return View(model);
            }
            else
            {
                return Content("Route not found...");
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult AcceptarRuta(CreateRouteModel model)
        {
            if (Request.Form["command"] == "accept")
            {
                DataHandler.AcceptRoute(model.id.Value);
            }
            else if (Request.Form["command"] == "delete")
            {
                DataHandler.DeleteRoute(model.id.Value);
            }

            return RedirectToAction("AcceptarRuta");
        }

        public ActionResult CrearRuta(int? id)
        {
            CreateRouteModel model = new CreateRouteModel();
            model.Valid = true;
            model.TypeList = DataHandler.GetEditableTypes();

            // fill value from existing route
            if (id.HasValue)
            {
                Route route = DataHandler.GetRoute(id.Value);
                if (route != null)
                {
                    FillRouteModel(route, model);
                }
                else
                {
                    return RedirectToAction("CrearRuta");
                }
            }

            return View(model);
        }


        private void FillRouteModel(Route route, CreateRouteModel model)
        {
            model.routename = route.Name;
            model.description = route.Description;
            model.origin = route.FromName;
            model.destination = route.ToName;
            model.parentId = route.ParentRouteID;
            model.status = route.Status;

            if (Request.IsAuthenticated)
            {
                model.email = route.Email;
            }

            model.id = route.ID;
            model.type = route.TypeID;

            List<RoutePiece> pieces = DataHandler.GetRoutePieces(route.ID);
            foreach (var piece in pieces)
            {
                if (route.SplitRoutePieceID.HasValue && route.SplitRoutePieceID.Value <= piece.ID)
                {
                    model.mapdata2 += Math.Round(piece.Lat, 10) + "," + Math.Round(piece.Lng, 10) + " ";
                }
                else
                {
                    model.mapdata1 += Math.Round(piece.Lat, 10) + "," + Math.Round(piece.Lng, 10) + " ";
                }
                if (!string.IsNullOrEmpty(piece.Name))
                {
                    model.mapnames += Math.Round(piece.Lat, 10) + "," + Math.Round(piece.Lng, 10) + "_" + piece.Name + "|";
                }
            }
        }

        [HttpPost]
        public ActionResult MarcarRuta(int id)
        {
            DataHandler.MarkRoute(id);
            return Content("");
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CrearRuta(CreateRouteModel model)
        {
            bool valid = !string.IsNullOrWhiteSpace(model.routename);
            valid &= !string.IsNullOrWhiteSpace(model.origin);
            valid &= !string.IsNullOrWhiteSpace(model.destination);
            valid &= model.type > 0;
            valid &= !string.IsNullOrWhiteSpace(model.mapdata1);
            valid &= !string.IsNullOrWhiteSpace(model.email);

            if (valid)
            {
                bool routeWasActiveBefore = false;

                // before saving deactivate this route and delete routepieces
                if (model.id.HasValue && Request.IsAuthenticated)
                {
                    DataHandler.DeleteRoutePiecesAndDeactivate(model.id.Value);
                }

                using (DataContext context = new DataContext())
                {
                    Route route;
                    if (model.id.HasValue && Request.IsAuthenticated)
                    {
                        route = context.Routes.FirstOrDefault(x => x.ID == model.id.Value);
                    }
                    else
                    {
                        route = new Route();
                        route.ParentRouteID = model.id;
                        context.Routes.InsertOnSubmit(route);
                    }
                    route.FromName = model.origin;
                    route.ToName = model.destination;
                    route.Name = model.routename;

                    if (route.TypeID == (int)TypeEnum.Microbus)
                    {
                        route.SeoName = Utils.FormatSEO(route.FromName + " a " + route.ToName);
                    }
                    else
                    {
                        route.SeoName = Utils.FormatSEO(route.Name);
                    }

                    route.TypeID = model.type;
                    route.Email = model.email;
                    route.Description = model.description;
                    route.Status = (int)StatusEnum.New;
                    context.SubmitChanges();

                    model.id = route.ID;

                    List<RoutePiece> pieces = new List<RoutePiece>();
                    foreach (var coords in model.Positions1)
                    {
                        var latLng = coords.Split(',');
                        var name = model.GetMapNameAtPosition(coords);
                        var routePiece = new RoutePiece { Lat = double.Parse(latLng[0]), Lng = double.Parse(latLng[1]), RouteID = route.ID, Name = name, SeoName = Utils.FormatSEO(name) };
                        pieces.Add(routePiece);
                    }
                    context.RoutePieces.InsertAllOnSubmit(pieces);
                    context.SubmitChanges();

                    if (!string.IsNullOrWhiteSpace(model.mapdata2))
                    {
                        List<RoutePiece> pieces2 = new List<RoutePiece>();
                        foreach (var coords in model.Positions2)
                        {
                            var latLng = coords.Split(',');
                            var name = model.GetMapNameAtPosition(coords);
                            var routePiece = new RoutePiece { Lat = double.Parse(latLng[0]), Lng = double.Parse(latLng[1]), RouteID = route.ID, Name = name, SeoName = Utils.FormatSEO(name) };
                            pieces2.Add(routePiece);
                        }
                        context.RoutePieces.InsertAllOnSubmit(pieces2);
                        context.SubmitChanges();

                        route.SplitRoutePieceID = pieces2.First().ID;
                        context.SubmitChanges();
                    }
                }

                if (Request.IsAuthenticated)
                {
                    if (routeWasActiveBefore)
                    {
                        DataHandler.ActivateRoute(model.id.Value);
                    }
                    return RedirectToAction("Routes");
                }
                else
                {
                    return RedirectToAction("RutaRegistrada", "Home");
                }
            }

            model.Valid = valid;
            model.TypeList = DataHandler.GetEditableTypes();
            return View(model);
        }
    }
}
