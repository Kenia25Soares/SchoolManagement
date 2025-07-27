using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Repositories;
using SchoolManagement.Web.Helpers;
using SchoolManagement.Web.Models;

namespace SchoolManagement.Web.Controllers
{
    [Authorize(Roles = "Employee")]
    [Route("EmployeeDashboard/Students")]
    public class StudentsController : Controller
    {
        private readonly IStudentClassRepository _classRepository;
        private readonly IGradesRepository _gradeRepository;
        private readonly IStudentProfileRepository _studentProfileRepository;
        private readonly IBlobHelper _blobHelper;
        private readonly IMailHelper _mailHelper;
        private readonly IUserHelper _userHelper;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentsController(
            IUserHelper userHelper,
            IStudentClassRepository classRepository,
            IGradesRepository gradeRepository,
            IStudentProfileRepository studentProfileRepository,
            IBlobHelper blobHelper,
            IMailHelper mailHelper,
            UserManager<ApplicationUser> userManager)
        {
            _userHelper = userHelper;
            _classRepository = classRepository;
            _gradeRepository = gradeRepository;
            _studentProfileRepository = studentProfileRepository;
            _blobHelper = blobHelper;
            _mailHelper = mailHelper;
            _userManager = userManager;
        }

        /// <summary>
        /// Sets the current user's profile picture in the view data.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task SetUserProfilePictureAsync()
        {
            var user = await _userHelper.GetUserByEmailAsync(User.Identity?.Name ?? string.Empty);
            ViewData["ProfilePictureUrl"] = user?.ProfilePictureUrl;
        }

        /// <summary>
        /// Loads the list of classes to populate the view's dropdown.
        /// </summary>
        /// <param name="selected">The selected class ID (optional).</param>
        private async Task LoadClassesAsync(object? selected = null)
        {
            var classes = await _classRepository.GetAllOrderedByNameAsync();
            ViewBag.Classes = new SelectList(classes, "Id", "Name", selected);
        }


        /// <summary>
        /// Displays a list of all student profiles with calculated average grades.
        /// </summary>
        /// <returns>The student list view.</returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await SetUserProfilePictureAsync();

            var students = await _studentProfileRepository.GetAll()
                .Include(s => s.User)
                .Include(s => s.StudentClass)
                .ToListAsync();

            var studentIds = students.Select(s => s.User.Id).ToList();
            var grades = await _gradeRepository.GetGradesByStudentIdsAsync(studentIds);

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
                Id = s.User.Id,
                FullName = s.User.FullName,
                Email = s.User.Email ?? string.Empty,
                Role = "Student",
                ProfilePictureUrl = s.User.ProfilePictureUrl,
                AverageGrade = averages.ContainsKey(s.User.Id) ? averages[s.User.Id] : (double?)null,
                ClassName = s.StudentClass?.Name ?? "--"
            }).ToList();

            return View(model);
            //Views/EmployeeDashboard/Students/Index
        }


        /// <summary>
        /// Displays the form to create a new student.
        /// </summary>
        /// <returns>The create student form view.</returns>
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            await LoadClassesAsync();
            return View(new CreateStudentViewModel());
            //Views/EmployeeDashboard/Students/Create
        }


        /// <summary>
        /// Handles the creation of a new student profile and user account.
        /// </summary>
        /// <param name="model">The form model for creating the student.</param>
        /// <returns>Redirects to Index or reloads form on error.</returns>
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateStudentViewModel model)
        {
            ModelState.Remove(nameof(CreateStudentViewModel.ProfilePictureUrl));
            ModelState.Remove(nameof(CreateStudentViewModel.OfficialPhotoUrl));
            await LoadClassesAsync(model.StudentClassId);

            if (!ModelState.IsValid)
                return View(model);
            //Views/EmployeeDashboard/Students/Create

            var profileBlobId = model.ProfilePicture != null
                ? await _blobHelper.UploadBlobAsync(model.ProfilePicture, "projetspictures")
                : Guid.Empty;

            var officialBlobId = model.OfficialPhoto != null
                ? await _blobHelper.UploadBlobAsync(model.OfficialPhoto, "projetspictures")
                : Guid.Empty;

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                ProfilePictureUrl = profileBlobId == Guid.Empty ? null : profileBlobId.ToString()
            };

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = $"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}";
                //Views/EmployeeDashboard/Students/Create
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, "Student");

            var student = new StudentProfile
            {
                UserId = user.Id,
                Address = model.Address,
                DateOfBirth = model.DateOfBirth ?? DateTime.MinValue,
                StudentClassId = model.StudentClassId,
                OfficialPhotoUrl = officialBlobId == Guid.Empty ? null : officialBlobId.ToString(),
                IsExcludedDueToAbsences = false
            };

            await _studentProfileRepository.CreateAsync(student);

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = Url.Action("ResetPassword", "Account", new { token, email = user.Email }, Request.Scheme);

            var emailBody = $@"
                <h2>Welcome to the School Management System</h2>
                <p>To set your password, please click the link below:</p>
                <p><a href='{resetLink}'>Set your password</a></p>";

            var mailResponse = _mailHelper.SendEmail(user.Email, "Set your password", emailBody);
            if (!mailResponse.IsSuccess)
            {
                TempData["ErrorMessage"] = "Student created, but failed to send password setup email.";
            }

            TempData["SuccessMessage"] = "Student created successfully! A password setup email was sent.";
            return RedirectToAction(nameof(Index));
        }


        /// <summary>
        /// Deletes a student profile and user, if no grades are associated.
        /// </summary>
        /// <param name="id">The ID of the student to delete.</param>
        /// <returns>Redirects to Index with result message.</returns>
        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var student = await _studentProfileRepository.GetAll()
                .FirstOrDefaultAsync(s => s.UserId == id);

            if (student == null)
            {
                TempData["ErrorMessage"] = "Student not found.";
                return RedirectToAction(nameof(Index));
            }

            if (await _gradeRepository.GetGradesByStudentIdsAsync(new List<string> { id }) is { Count: > 0 })
            {
                TempData["ErrorMessage"] = "Cannot delete student. There are grades associated.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(id);

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, roles);
            }

            await _studentProfileRepository.DeleteAsync(student);

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = "Failed to delete the user from Identity.";
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = "Student and user deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Displays the edit form for a student profile.
        /// </summary>
        /// <param name="id">The student user ID.</param>
        /// <returns>The edit student form view.</returns>
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            var student = await _studentProfileRepository.GetAll()
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.User.Id == id);

            if (student == null)
            {
                TempData["ErrorMessage"] = "Student not found.";
                return RedirectToAction(nameof(Index));
            }

            await LoadClassesAsync(student.StudentClassId);

            var model = new EditStudentProfileViewModel
            {
                Id = student.User.Id,
                FullName = student.User.FullName,
                Email = student.User.Email ?? string.Empty,
                PhoneNumber = student.User.PhoneNumber,
                DateOfBirth = student.DateOfBirth,
                Address = student.Address,
                StudentClassId = student.StudentClassId,
                ProfilePictureUrl = student.User.ProfilePictureUrl,
                OfficialPhotoUrl = student.OfficialPhotoUrl
            };

            //Views/EmployeeDashboard/Students/Edit
            return View(model);
        }



        /// <summary>
        /// Handles the update of student profile and user data.
        /// </summary>
        /// <param name="model">The edited student data.</param>
        /// <returns>Redirects to Index or reloads form on error.</returns>
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditStudentProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadClassesAsync(model.StudentClassId);
                //Views/EmployeeDashboard/Students/Edit
                return View(model);
            }

            var student = await _studentProfileRepository.GetAll()
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.User.Id == model.Id);

            if (student == null)
            {
                TempData["ErrorMessage"] = "Student not found.";
                return RedirectToAction(nameof(Index));
            }

            student.User.FullName = model.FullName;
            student.User.Email = model.Email;
            student.User.UserName = model.Email;
            student.User.PhoneNumber = model.PhoneNumber;
            student.DateOfBirth = model.DateOfBirth;
            student.Address = model.Address;

            if (!User.IsInRole("Student"))
            {
                student.StudentClassId = model.StudentClassId;
            }

            if (model.ProfilePicture != null)
            {
                var blobId = await _blobHelper.UploadBlobAsync(model.ProfilePicture, "projetspictures");
                student.User.ProfilePictureUrl = blobId.ToString();
            }

            if (model.OfficialPhoto != null)
            {
                var blobId = await _blobHelper.UploadBlobAsync(model.OfficialPhoto, "projetspictures");
                student.OfficialPhotoUrl = blobId.ToString();
            }

            await _studentProfileRepository.UpdateAsync(student);

            TempData["SuccessMessage"] = "Student updated successfully!";
            return RedirectToAction(nameof(Index));
        }


        /// <summary>
        /// Displays detailed information about a specific student.
        /// </summary>
        /// <param name="id">The student user ID.</param>
        /// <returns>The student detail view.</returns>
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(string id)
        {
            var student = await _studentProfileRepository.GetAll()
                .Include(s => s.User)
                .Include(s => s.StudentClass)
                .FirstOrDefaultAsync(s => s.User.Id == id);

            if (student == null)
                return NotFound();

            var model = new StudentDetailsViewModel
            {
                FullName = student.User.FullName,
                Email = student.User.Email ?? string.Empty,
                PhoneNumber = student.User.PhoneNumber ?? string.Empty,
                DateOfBirth = student.DateOfBirth,
                Address = student.Address ?? string.Empty,
                ClassName = student.StudentClass?.Name ?? "--",
                ProfilePictureUrl = student.User.ProfilePictureUrl,
                OfficialPhotoUrl = student.OfficialPhotoUrl
            };

            return View(model);
            //Views/EmployeeDashboard/Students/Details
        }
    }
}
