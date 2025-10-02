using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Repositories;
using SchoolManagement.Web.Helpers;
using SchoolManagement.Web.Models;
using SchoolManagement.Web.Data.Enums;

namespace SchoolManagement.Web.Controllers
{
    [Authorize(Roles = "Employee")]
    public class EmployeeAlertsController : Controller
    {
        private readonly IAlertRepository _alertRepository;
        private readonly IUserHelper _userHelper;

        public EmployeeAlertsController(IAlertRepository alertRepository, 
            IUserHelper userHelper)
        {
            _alertRepository = alertRepository;
            _userHelper = userHelper;
        }

        /// <summary>
        /// Displays the form to create a new alert.
        /// </summary>
        [HttpGet("Create")]
        public IActionResult Create()
        {
            var model = new CreateAlertViewModel();
            return View("~/Views/EmployeeDashboard/Alerts/Create.cshtml", model);
        }


        /// <summary>
        /// Handles the submission of a new alert form.
        /// </summary>
        /// <param name="model">The alert data submitted by the user.</param>
        /// <returns>Redirects to dashboard on success, or reloads form on failure.</returns>
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAlertViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please correct the form and try again.";
                return View("~/Views/EmployeeDashboard/Alerts/Create.cshtml", model);
            }

                var user = await _userHelper.GetUserByEmailAsync(User.Identity?.Name ?? string.Empty);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Unable to identify the logged-in user.";
                return RedirectToAction("Index", "EmployeeDashboard");
            }

            var alert = new Alert
            {
                Title = model.Title,
                Message = model.Description,
                Type = AlertType.GeneralNotification,
                CreatedById = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            await _alertRepository.CreateAsync(alert);

            TempData["SuccessMessage"] = "Alert successfully created.";
            return RedirectToAction("Index", "EmployeeDashboard");
        }
    }
}
