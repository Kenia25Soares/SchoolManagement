using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Web.Models;
using System.Diagnostics;

namespace SchoolManagement.Web.Controllers
{
    public class ErrorsController : Controller
    {
        [Route("Errors/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            switch (statusCode)
            {
                case 404:
                    return View("404");
                case 403:
                    return View("403");
                default:
                    return View("500");
            }
        }

        [Route("Errors/Error")]
        public IActionResult Error()
        {
            return View("500", new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
