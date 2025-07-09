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

namespace SchoolManagement.Web.Controllers
{
    [Authorize(Roles = "Employee")]
    [Route("EmployeeDashboard/Students")]
    public class StudentsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBlobHelper _blobHelper;
        private readonly DataContext _context;
        private readonly IMailHelper _mailHelper;

        public StudentsController(
            UserManager<ApplicationUser> userManager,
            IMailHelper mailHelper,
            IBlobHelper blobHelper,
            DataContext context)
        {
            _userManager = userManager;
            _blobHelper = blobHelper;
            _context = context;
            _mailHelper = mailHelper;
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

        [HttpGet()]
        public async Task<IActionResult> Index()
        {
            await SetUserProfilePictureAsync();

            var students = await _userManager.Users
                .OfType<StudentUser>()
                .Include(s => s.StudentClass)
                .ToListAsync();

            var studentIds = students.Select(s => s.Id).ToList();

            var grades = await _context.StudentGrades
                .Where(g => studentIds.Contains(g.StudentId) && g.Grade.HasValue && g.GradeTypeId != null)
                .Include(g => g.GradeType)
                .ToListAsync();

            var averages = grades
                .GroupBy(g => g.StudentId)
                .ToDictionary(g => g.Key, g =>
                {
                    double weightedSum = 0, totalWeight = 0;
                    foreach (var group in g.GroupBy(x => x.GradeType))
                    {
                        var weight = group.Key?.Weight ?? 0;
                        if (weight > 0)
                        {
                            var avg = group.Average(x => x.Grade ?? 0);
                            weightedSum += avg * weight;
                            totalWeight += weight;
                        }
                    }
                    return totalWeight > 0 ? weightedSum / totalWeight : 0;
                });

            var model = students.Select(s => new UserListViewModel
            {
                Id = s.Id,
                FullName = s.FullName,
                Email = s.Email,
                Role = "Student",
                ProfilePictureUrl = s.ProfilePictureUrl,
                AverageGrade = averages.ContainsKey(s.Id) ? averages[s.Id] : (double?)null,
                ClassName = s.StudentClass?.Name ?? "N/A"
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
            ModelState.Remove(nameof(CreateStudentViewModel.ProfilePictureUrl));
            ModelState.Remove(nameof(CreateStudentViewModel.OfficialPhotoUrl));

            await SetUserProfilePictureAsync();
            await LoadClassesAsync(model.StudentClassId);

            if (!ModelState.IsValid)
                return View("/Views/EmployeeDashboard/Students/Create.cshtml", model);

            Guid profilePictureBlobId = Guid.Empty;
            Guid officialPhotoBlobId = Guid.Empty;

            if (model.ProfilePicture != null)
                profilePictureBlobId = await _blobHelper.UploadBlobAsync(model.ProfilePicture, "projetspictures");

            if (model.OfficialPhoto != null)
                officialPhotoBlobId = await _blobHelper.UploadBlobAsync(model.OfficialPhoto, "projetspictures");

            var student = new StudentUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                DateOfBirth = model.DateOfBirth ?? DateTime.MinValue,
                Address = model.Address,
                StudentClassId = model.StudentClassId,
                ProfilePictureUrl = profilePictureBlobId == Guid.Empty ? null : profilePictureBlobId.ToString(),
                OfficialPhotoUrl = officialPhotoBlobId == Guid.Empty ? null : officialPhotoBlobId.ToString(),
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

            var token = await _userManager.GeneratePasswordResetTokenAsync(student);
            var resetLink = Url.Action(
                "ResetPassword",
                "Account",
                new { token, email = student.Email },
                protocol: Request.Scheme);

            var emailBody = $@"
                <h2>Welcome to the School Management System</h2>
                <p>To set your password, please click the link below:</p>
                <p><a href='{resetLink}'>Set your password</a></p>";

            var mailResponse = _mailHelper.SendEmail(student.Email, "Set your password", emailBody);
            if (!mailResponse.IsSuccess)
            {
                TempData["ErrorMessage"] = "Student created, but failed to send password setup email.";
            }

            TempData["SuccessMessage"] = "Student created successfully! A password setup email was sent.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id) as StudentUser;
            if (user == null)
            {
                TempData["ErrorMessage"] = "Student not found.";
                return RedirectToAction("Index");
            }

            bool hasGrades = await _context.StudentGrades.AnyAsync(g => g.StudentId == id);
            if (hasGrades)
            {
                TempData["ErrorMessage"] = "Cannot delete student. There are grades associated with this student.";
                return RedirectToAction("Index");
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Any())
            {
                var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, roles);
                if (!removeRolesResult.Succeeded)
                {
                    TempData["ErrorMessage"] = "Failed to remove student roles.";
                    return RedirectToAction("Index");
                }
            }

            var deleteResult = await _userManager.DeleteAsync(user);
            if (!deleteResult.Succeeded)
            {
                TempData["ErrorMessage"] = "Error deleting student.";
                return RedirectToAction("Index");
            }

            TempData["SuccessMessage"] = "Student deleted successfully.";
            return RedirectToAction("Index");
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            await SetUserProfilePictureAsync();

            var user = await _userManager.FindByIdAsync(id) as StudentUser;
            if (user == null)
            {
                TempData["ErrorMessage"] = "Student not found.";
                return RedirectToAction(nameof(Index));
            }

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
            if (user == null)
            {
                TempData["ErrorMessage"] = "Student not found.";
                return RedirectToAction(nameof(Index));
            }

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
                Guid profileBlobId = await _blobHelper.UploadBlobAsync(model.ProfilePicture, "projetspictures");
                user.ProfilePictureUrl = profileBlobId.ToString();
            }

            if (model.OfficialPhoto != null)
            {
                Guid officialBlobId = await _blobHelper.UploadBlobAsync(model.OfficialPhoto, "projetspictures");
                user.OfficialPhotoUrl = officialBlobId.ToString();
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

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var student = await _userManager.Users
                .OfType<StudentUser>()
                .Include(s => s.StudentClass)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
                return NotFound();

            var model = new StudentDetailsViewModel
            {
                FullName = student.FullName,
                Email = student.Email,
                PhoneNumber = student.PhoneNumber,
                DateOfBirth = student.DateOfBirth,
                Address = student.Address,
                ClassName = student.StudentClass?.Name ?? "N/A",
                ProfilePictureUrl = student.ProfilePictureUrl,
                OfficialPhotoUrl = student.OfficialPhotoUrl
            };

            return View("/Views/EmployeeDashboard/Students/Details.cshtml", model);
        }
    }
}
