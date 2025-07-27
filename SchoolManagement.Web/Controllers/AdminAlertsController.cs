using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Web.Data.Repositories;
using SchoolManagement.Web.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolManagement.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("AdminDashboard/Alerts")]
    public class AdminAlertsController : Controller
    {
        private readonly IAlertRepository _alertRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminAlertsController"/> class.
        /// </summary>
        /// <param name="alertRepository">Repository for accessing alert data.</param>
        public AdminAlertsController(IAlertRepository alertRepository)
        {
            _alertRepository = alertRepository;
        }


        /// <summary>
        /// Displays a list of all alerts for administrators.
        /// </summary>
        /// <returns>Returns the alerts view with a list of alerts.</returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            //var alerts = _alertRepository.GetAll()
            var alerts = await _alertRepository.GetAllWithCreatorAsync();

            var model = alerts.Select(a => new AlertViewModel
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                Priority = a.Priority,
                CreatedBy = a.CreatedBy?.FullName ?? "Unknown",
                CreatedAt = a.CreatedAt,
                IsResolved = a.IsResolved
            }).ToList();

            return View(model);
            //Views/AdminDashboard/Alerts/Index

        }


        /// <summary>
        /// Marks a specific alert as resolved.
        /// </summary>
        /// <param name="id">The ID of the alert to resolve.</param>
        /// <returns>Redirects to the admin dashboard with a success or error message.</returns>
        [HttpPost("Resolve/{id}")]
        public async Task<IActionResult> Resolve(int id)
        {
            var alert = await _alertRepository.GetByIdAsync(id);
            if (alert == null)
            {
                TempData["ErrorMessage"] = "Alert not found.";
                return RedirectToAction("Index", "AdminDashboard");
            }

            alert.IsResolved = true;
            await _alertRepository.UpdateAsync(alert);

            TempData["SuccessMessage"] = "Alert successfully marked as resolved.";
            return RedirectToAction("Index", "AdminDashboard");
        }
    }
}
