using System.Web.Mvc;
using System.Web.Routing;

namespace viadfweb
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
               "Delegacion",
               "Directorio/Distrito-Federal/{name}/{id}",
               new { controller = "Home", action = "Delegacion", estadoId = 1, name = UrlParameter.Optional, id = UrlParameter.Optional },
               new { id = @"\d*" }
            );

            routes.MapRoute(
               "Delegacion2",
               "Directorio/Estado-de-Mexico/{name}/{id}",
               new { controller = "Home", action = "Delegacion", estadoId = 2, name = UrlParameter.Optional, id = UrlParameter.Optional },
               new { id = @"\d*" }
            );

            routes.MapRoute(
                "Colonia",
                "Directorio/Distrito-Federal/{delegacion}/{name}/{id}",
                new { controller = "Home", action = "Colonia", estadoId = 1, id = UrlParameter.Optional },
                new { id = @"\d*" }
            );

            routes.MapRoute(
                "Colonia2",
                "Directorio/Estado-de-Mexico/{delegacion}/{name}/{id}",
                new { controller = "Home", action = "Colonia", estadoId = 2, id = UrlParameter.Optional },
                new { id = @"\d*" }
            );

            routes.MapRoute(
               "RouteList",
               "Directorio/{type}",
               new { controller = "Home", action = "RouteList", type = UrlParameter.Optional }
            );

            routes.MapRoute(
                "RouteOld",
                "Directorio/Ruta/{type}/{name}/{id}",
                new { controller = "Home", action = "Route" },
                new { id = @"\d+" }
            );

            routes.MapRoute(
                "RouteNew",
                "Directorio/{type}/{name}",
                new { controller = "Home", action = "Route" }
            );

            routes.MapRoute(
                "RoutePieceOld",
                "Directorio/Estacion/{type}/{route}/{name}/{id}",
                new { controller = "Home", action = "RoutePiece" },
                new { id = @"\d+" }
            );

            routes.MapRoute(
                "RoutePieceNew",
                "Directorio/{type}/{route}/{name}",
                new { controller = "Home", action = "RoutePiece" }
            );

            routes.MapRoute(
               "Mapa",
               "Mapa/{type}",
               new { controller = "Home", action = "Mapa", type = UrlParameter.Optional }
            );

            routes.MapRoute(
               "Contacto",
               "Contacto",
               new { controller = "Home", action = "Contacto" }
            );

            routes.MapRoute(
              "Privacidad",
              "Privacidad",
              new { controller = "Home", action = "Privacidad" }
            );

            routes.MapRoute(
              "Condiciones",
              "Condiciones",
              new { controller = "Home", action = "Condiciones" }
            );

            routes.MapRoute(
              "PoligonosTiempoRecorrido",
              "PoligonosTiempoRecorrido",
              new { controller = "Home", action = "PoligonosTiempoRecorrido" }
            );

            routes.MapRoute(
              "WebService",
              "WebService",
              new { controller = "Home", action = "WebService" }
            );

            routes.MapRoute(
              "Api",
              "Api",
              new { controller = "Home", action = "Api" }
            );            

            routes.MapRoute(
              "OpenData",
              "OpenData",
              new { controller = "Home", action = "OpenData" }
            );

            routes.MapRoute(
              "AgregarRuta",
              "AgregarRuta",
              new { controller = "Home", action = "AgregarRuta" }
            );

            routes.MapRoute(
              "RutaRegistrada",
              "RutaRegistrada",
              new { controller = "Home", action = "RutaRegistrada" }
            );

            routes.MapRoute(
             "LegacySearch",
             "Find.aspx",
             new { controller = "Search", action = "LegacySearch" }
            );

            routes.MapRoute(
             "Search2",
             "Busqueda",
             new { controller = "Search", action = "Search" }
            );

            routes.MapRoute("LegacyRouteList", "Lista/{type}.aspx", new { controller = "Home", action = "LegacyRouteList" });
            routes.MapRoute("LegacyRouteList2", "ListaRutas.aspx", new { controller = "Home", action = "LegacyRouteList2" });

            routes.MapRoute("LegacyDefault", "Default.aspx", new { controller = "Home", action = "LegacyDefault" });
            routes.MapRoute("LegacyDefault2", "Buscar.aspx", new { controller = "Home", action = "LegacyDefault" });

            routes.MapRoute("Deleted1", "{name}.aspx", new { controller = "Home", action = "ContentDeleted" });


            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
