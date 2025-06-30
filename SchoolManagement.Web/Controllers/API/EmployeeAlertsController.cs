using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Web.Data;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Helpers;
using SchoolManagement.Web.Models;

namespace SchoolManagement.Web.Controllers.API
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


        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View("/Views/EmployeeDashboard/Alerts/Create.cshtml");
        }


        [HttpPost("Create")]
        public async Task<IActionResult> Create(CreateAlertViewModel model)
        {
            if (!ModelState.IsValid)
                return View("/Views/EmployeeDashboard/Alerts/Create.cshtml", model);

            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);

            var alert = new Alert
            {
                Title = model.Title,
                Description = model.Description,
                Priority = model.Priority,
                CreatedById = user.Id
            };

            _context.Alerts.Add(alert);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "EmployeeDashboard");
        }
    }
}
