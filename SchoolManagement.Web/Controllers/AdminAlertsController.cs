using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Models;

namespace SchoolManagement.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("AdminDashboard/Alerts")]
    public class AdminAlertsController : Controller
    {

        private readonly DataContext _context;


        public AdminAlertsController(DataContext context)
        {
            _context = context;
        }



        [HttpGet]
        public IActionResult Index()
        {
            var alerts = _context.Alerts
                 .Include(a => a.CreatedBy)
                 .OrderByDescending(a => a.CreatedAt)
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
            var alert = await _context.Alerts.FindAsync(id);
            if (alert == null) return NotFound();

            alert.IsResolved = true;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "AdminDashboard");
        }
    }
}
