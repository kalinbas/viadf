using System;
using System.Web.Mvc;

namespace viadf.Controllers
{
    public class ContentDeletedResult : ActionResult
    {
       
        public ContentDeletedResult()
        {            
        }

        public override void ExecuteResult(ControllerContext context)
        {     
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            context.HttpContext.Response.Clear(); 
            context.HttpContext.Response.StatusCode = 410;
            context.HttpContext.Response.End();
        }
    } 
}