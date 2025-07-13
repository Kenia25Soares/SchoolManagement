using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Repositories;
using SchoolManagement.Web.Helpers;
using SchoolManagement.Web.Models;
using System.Threading.Tasks;

namespace SchoolManagement.Web.Controllers
{
    [Authorize(Roles = "Employee")]
    [Route("EmployeeDashboard/Alerts")]
    public class EmployeeAlertsController : Controller
    {
        private readonly IAlertRepository _alertRepository;
        private readonly IUserHelper _userHelper;

        public EmployeeAlertsController(IAlertRepository alertRepository, IUserHelper userHelper)
        {
            _alertRepository = alertRepository;
            _userHelper = userHelper;
        }

        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View("/Views/EmployeeDashboard/Alerts/Create.cshtml");
        }

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

            await _alertRepository.CreateAsync(alert);

            TempData["SuccessMessage"] = "Alert successfully created.";
            return RedirectToAction("Index", "EmployeeDashboard");
        }
    }
}
