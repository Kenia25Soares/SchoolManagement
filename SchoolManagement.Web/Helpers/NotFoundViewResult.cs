using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace SchoolManagement.Web.Helpers
{
    /// <summary>
    /// Custom ViewResult that returns 404 (Not Found).
    /// </summary>
    public class NotFoundViewResult : ViewResult
    {
        public NotFoundViewResult(string viewName)
        {
            ViewName = viewName;
            StatusCode = (int)HttpStatusCode.NotFound;
        }
    }
}
