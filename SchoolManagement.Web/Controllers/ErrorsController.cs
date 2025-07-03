using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Web.Models;
using System.Diagnostics;

namespace SchoolManagement.Web.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorsController : Controller
    {
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        [Route("errors/{code:int}")]
        public IActionResult HandleStatusCode(int code)
        {
            if (code == 404)
                return View("404");

            if (code == 403)
                return View("403");

            return View("Generic");
        }

        [HttpGet]
        [Route("Errors")]
        public IActionResult HandleException()
        {
            return View("500");
        }
    }
}
