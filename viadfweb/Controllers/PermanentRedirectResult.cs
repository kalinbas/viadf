using System;
using System.Web.Mvc;

namespace viadf.Controllers
{
    public class PermanentRedirectResult : ActionResult
    {
        public string Url { get; private set; }

        public PermanentRedirectResult(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("url is null or empty", "url");
            }
            this.Url = url;
        }

        public override void ExecuteResult(ControllerContext context)
        {           

            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            context.HttpContext.Response.Clear(); 
            context.HttpContext.Response.StatusCode = 301;
            context.HttpContext.Response.RedirectLocation = Url;
            context.HttpContext.Response.End();
        }
    } 
}