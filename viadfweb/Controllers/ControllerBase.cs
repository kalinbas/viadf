using System.Web.Mvc;
using viadflib;

namespace viadf.Controllers
{
    public class ControllerBase : Controller
    {
        protected override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.Exception != null)
            {
                DataHandler.WriteException(filterContext.Exception, filterContext.HttpContext.Request.UserHostAddress);
            }
            base.OnException(filterContext);
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
        }

        protected void SetSEO(string title, string description, string keywords)
        {
            ViewBag.Title = "ViaDF - " + title;
            ViewBag.Description = description;
            ViewBag.Keywords = keywords;
        }

        protected void DisableAds()
        {
            ViewData["DisableAds"] = true;
        }
    }
}