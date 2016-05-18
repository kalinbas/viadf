using System.Web.Mvc;

namespace viadf.Controllers
{
    public class ErrorController : ControllerBase
    {
        public ActionResult Index()
        {
            SetSEO("Sucedió un error", "", "");            
            return View();
        } 

        public ActionResult Error404()
        {
            SetSEO("La pagina no se encontró", "", "");
            return View();
        }       
      
    }
}
