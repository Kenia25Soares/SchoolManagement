using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Web.Models;
using System.Diagnostics;

namespace SchoolManagement.Web.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorsController : Controller
    {
        /// <summary>
        /// Handles general unhandled errors and returns the error view with request diagnostics.
        /// </summary>
        /// <returns>An error view with request ID for troubleshooting.</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        /// <summary>
        /// Handles specific HTTP status code errors like 404 and 403.
        /// </summary>
        /// <param name="code">The HTTP status code.</param>
        /// <returns>The corresponding error view based on the status code.</returns>
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


        /// <summary>
        /// Handles unhandled exceptions and shows the generic 500 error view.
        /// </summary>
        /// <returns>The 500 error view.</returns>
        [HttpGet]
        [Route("Errors")]
        public IActionResult HandleException()
        {
            return View("500");
        }
    }
}
