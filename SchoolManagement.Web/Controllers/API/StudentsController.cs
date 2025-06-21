using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Helpers;
using SchoolManagement.Web.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolManagement.Web.Controllers.API
{
    [Authorize(Roles = "Employee")]
    [Route("EmployeeDashboard/Students")]
    public class StudentsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBlobHelper _blobHelper;
        private readonly DataContext _context;

        public StudentsController(UserManager<ApplicationUser> userManager, IBlobHelper blobHelper, DataContext context)
        {
            _userManager = userManager;
            _blobHelper = blobHelper;
            _context = context;
        }

        private async Task SetUserProfilePictureAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewData["ProfilePictureUrl"] = user?.ProfilePictureUrl;
        }

        private async Task LoadClassesAsync(object selected = null)
        {
            var classes = await _context.StudentClasses.OrderBy(c => c.Name).ToListAsync();
            ViewBag.Classes = new SelectList(classes, "Id", "Name", selected);
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            await SetUserProfilePictureAsync();

            var students = await _userManager.Users.OfType<StudentUser>().ToListAsync();
            var model = students.Select(s => new UserListViewModel
            {
                Id = s.Id,
                FullName = s.FullName,
                Email = s.Email,
                Role = "Student",
                ProfilePictureUrl = s.ProfilePictureUrl
            }).ToList();

            return View("/Views/EmployeeDashboard/Students/Index.cshtml", model);
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            await SetUserProfilePictureAsync();
            await LoadClassesAsync();
            return View("/Views/EmployeeDashboard/Students/Create.cshtml", new CreateStudentViewModel());
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateStudentViewModel model)
        {
            await SetUserProfilePictureAsync();
            await LoadClassesAsync(model.StudentClassId);

            if (!ModelState.IsValid)
                return View("/Views/EmployeeDashboard/Students/Create.cshtml", model);

            Guid blobId = Guid.Empty;
            if (model.ProfilePicture != null)
                blobId = await _blobHelper.UploadBlobAsync(model.ProfilePicture, "projetspictures");

            var student = new StudentUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                DateOfBirth = model.DateOfBirth ?? DateTime.MinValue,
                Address = model.Address,
                OfficialPhotoUrl = model.OfficialPhotoUrl,
                StudentClassId = model.StudentClassId,
                ProfilePictureUrl = blobId == Guid.Empty ? null : blobId.ToString(),
                IsExcludedDueToAbsences = false
            };

            var result = await _userManager.CreateAsync(student);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return View("/Views/EmployeeDashboard/Students/Create.cshtml", model);
            }

            await _userManager.AddToRoleAsync(student, "Student");

            TempData["SuccessMessage"] = "Student created successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            await SetUserProfilePictureAsync();

            var user = await _userManager.FindByIdAsync(id) as StudentUser;
            if (user == null) return NotFound();

            await LoadClassesAsync(user.StudentClassId);

            var model = new EditStudentProfileViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                DateOfBirth = user.DateOfBirth,
                Address = user.Address,
                OfficialPhotoUrl = user.OfficialPhotoUrl,
                StudentClassId = user.StudentClassId,
                ProfilePictureUrl = user.ProfilePictureUrl
            };

            return View("/Views/EmployeeDashboard/Students/Edit.cshtml", model);
        }

        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditStudentProfileViewModel model)
        {
            await SetUserProfilePictureAsync();
            await LoadClassesAsync(model.StudentClassId);

            if (!ModelState.IsValid)
                return View("/Views/EmployeeDashboard/Students/Edit.cshtml", model);

            var user = await _userManager.FindByIdAsync(model.Id) as StudentUser;
            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.DateOfBirth = model.DateOfBirth;
            user.Address = model.Address;
            user.OfficialPhotoUrl = model.OfficialPhotoUrl;
            user.StudentClassId = model.StudentClassId;

            if (model.ProfilePicture != null)
            {
                Guid blobId = await _blobHelper.UploadBlobAsync(model.ProfilePicture, "projetspictures");
                user.ProfilePictureUrl = blobId.ToString();
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return View("/Views/EmployeeDashboard/Students/Edit.cshtml", model);
            }

            TempData["SuccessMessage"] = "Student updated successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
