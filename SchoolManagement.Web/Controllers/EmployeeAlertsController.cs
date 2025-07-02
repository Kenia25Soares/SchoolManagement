using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Web.Data;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Helpers;
using SchoolManagement.Web.Models;

namespace SchoolManagement.Web.Controllers
{
    [Authorize(Roles = "Employee")]
    [Route("EmployeeDashboard/Alerts")]
    public class EmployeeAlertsController : Controller
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;

        public EmployeeAlertsController(DataContext context, IUserHelper userHelper)
        {
            _context = context;
            _userHelper = userHelper;
        }

        /// <summary>
        /// Displays the form to create a new alert.
        /// </summary>
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View("/Views/EmployeeDashboard/Alerts/Create.cshtml");
        }

        /// <summary>
        /// Creates a new alert.
        /// </summary>
        [HttpPost("Create")]
        public async Task<IActionResult> Create(CreateAlertViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please correct the form and try again.";
                return View("/Views/EmployeeDashboard/Alerts/Create.cshtml", model);
            }

            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Unable to identify the logged-in user.";
                return RedirectToAction("Index", "EmployeeDashboard");
            }

            var alert = new Alert
            {
                Title = model.Title,
                Description = model.Description,
                Priority = model.Priority,
                CreatedById = user.Id
            };

            _context.Alerts.Add(alert);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Alert successfully created.";
            return RedirectToAction("Index", "EmployeeDashboard");
        }
    }
}
