using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using viadflib;

namespace viadf.Controllers
{
    public class SearchController : ControllerBase
    {

        public ActionResult LegacySearch(string fromll, string toll)
        {
            return new PermanentRedirectResult(Url.Action("Search", new { de = fromll, a = toll }));
        }

        [OutputCache(Duration = 3600, VaryByParam = "de;a;origen;destino")]
        public ActionResult Search(string de, string a, string origen, string destino)
        {   
            Response.Cache.SetCacheability(HttpCacheability.Private);
            Response.Cache.SetLastModified(DateTime.Now);

            // check if available on client cache
            if (!string.IsNullOrEmpty(Request.Headers["If-Modified-Since"]))
            {
                Response.StatusCode = 304;
                Response.StatusDescription = "Not Modified";
                return null;
            }

            // try to get coordinates from strings
            if ((string.IsNullOrWhiteSpace(de) || string.IsNullOrWhiteSpace(a)) && !string.IsNullOrWhiteSpace(origen) && !string.IsNullOrWhiteSpace(destino))
            {
                SearchPosition from = Utils.GeoCodeSearchPosition(origen.Trim());
                SearchPosition to = Utils.GeoCodeSearchPosition(destino.Trim());

                if (from != null && to != null)
                {
                    de = from.Lat.ToString(CultureInfo.InvariantCulture) + "," + from.Lng.ToString(CultureInfo.InvariantCulture);
                    a = to.Lat.ToString(CultureInfo.InvariantCulture) + "," + to.Lng.ToString(CultureInfo.InvariantCulture);
                }
            }

            if (!string.IsNullOrWhiteSpace(de) && !string.IsNullOrWhiteSpace(a))
            {
                SearchPosition spFrom = SearchPosition.CreateSearchPosition(de, origen);
                SearchPosition spTo = SearchPosition.CreateSearchPosition(a, destino);

                if (spFrom != null && spTo != null)
                {
                    Searcher searcher = new Searcher(ViaDFGraph.Instance);
                    SearchParams sp = new SearchParams();
                    sp.StartSearch = spFrom;
                    sp.StartSearch.LoadNameFromDB();
                    sp.EndSearch = spTo;
                    sp.EndSearch.LoadNameFromDB();
                    sp.NrOfResults = 3;

                    // log search
                    DataHandler.LogSearch(sp, Request.UserHostAddress);

                    if (string.IsNullOrWhiteSpace(sp.StartSearch.Name))
                    {
                        sp.StartSearch.Name = "Origen";
                    }
                    if (string.IsNullOrWhiteSpace(sp.EndSearch.Name))
                    {
                        sp.EndSearch.Name = "Destino";
                    }

                    List<SearchResult> results = searcher.DoSearch(sp);

                    if (results.Count > 0)
                    {
                        SearchResultModel model = new SearchResultModel();
                        model.Results = results;
                        model.CloseBusinesses = DataHandler.GetCloseBusinesses(spTo.Lat, spTo.Lng, 0.25, 30);

                        SetSEO(results[0].Start.Name + " a " + results[0].End.Name + " - ¿Cómo llegar en transporte público?", "", "");

                        return View("Search", model);
                    }
                }
            }

            return View("NotFound");
        }

        [ChildActionOnly]
        public ActionResult LastSearches(int? count)
        {
            int historyCount = count ?? 5;

            using (var context = new viadflib.DataContext())
            {
                List<SearchHistory> history = context.SearchHistories.Where(x => x.FromName != null && x.ToName != null && x.FromName.Length > 0 && x.ToName.Length > 0).OrderByDescending(x => x.ID).Take(historyCount).ToList();
                return View(history);
            }
        }
    }
}
