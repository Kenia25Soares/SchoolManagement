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

        public AdminAlertsController(IAlertRepository alertRepository)
        {
            _alertRepository = alertRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var alerts = _alertRepository.GetAll()
                .Select(a => new AlertViewModel
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    Priority = a.Priority,
                    CreatedBy = a.CreatedBy.FullName,
                    CreatedAt = a.CreatedAt,
                    IsResolved = a.IsResolved
                })
                .ToList();

            return View(alerts);
        }

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
