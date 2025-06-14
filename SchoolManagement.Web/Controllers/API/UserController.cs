using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Repository;
using SchoolManagement.Web.Helpers;
using SchoolManagement.Web.Models;
using SchoolManagement.Web.Models.ViewModels;

namespace SchoolManagement.Web.Controllers.API
{
    [Authorize(Roles = "Admin")]
    [Route("AdminDashboard/Users")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IStudentRepository _studentRepository;
        private readonly IMailHelper _mailHelper;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IStudentRepository studentRepository,
            IMailHelper mailHelper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _studentRepository = studentRepository;
            _mailHelper = mailHelper;
        }

        private async Task SetUserProfilePictureAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewData["ProfilePictureUrl"] = user?.ProfilePictureUrl;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            await SetUserProfilePictureAsync();

            var users = await _userManager.Users.ToListAsync();
            var model = new List<UserListViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                model.Add(new UserListViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = roles.FirstOrDefault() ?? "N/A"
                });
            }

            return View("/Views/AdminDashboard/Users/Index.cshtml", model);
        }

        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }

            return RedirectToAction("Index");
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            await SetUserProfilePictureAsync();

            var model = new CreateUserViewModel
            {
                Roles = new List<string> { "Admin", "Employee", "Student" }
            };
            return View("/Views/AdminDashboard/Users/Create.cshtml", model);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            model.Roles = new List<string> { "Admin", "Employee", "Student" };

            await SetUserProfilePictureAsync();

            if (!ModelState.IsValid)
                return View("/Views/AdminDashboard/Users/Create.cshtml", model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                ProfilePictureUrl = model.ProfilePictureUrl
            };

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View("/Views/AdminDashboard/Users/Create.cshtml", model);
            }

            await _userManager.AddToRoleAsync(user, model.Role);

            if (model.Role == "Student")
            {
                var student = new Student
                {
                    UserId = user.Id,
                    Contact = model.Contact,
                    DateOfBirth = model.DateOfBirth ?? DateTime.MinValue,
                    Address = model.Address,
                    OfficialPhotoUrl = model.OfficialPhotoUrl,
                    Absences = 0,
                    IsExcludedDueToAbsences = false
                };

                await _studentRepository.AddAsync(student);
            }

            string token = await _userManager.GeneratePasswordResetTokenAsync(user);
            string resetLink = Url.Action("ResetPassword", "Account", new { token, email = user.Email }, protocol: HttpContext.Request.Scheme);

            var response = _mailHelper.SendEmail(user.Email, "Set your password", $@"
                <h1>Welcome to School Management!</h1>
                <p>To set your password click the link below:</p>
                <p><a href='{resetLink}'>Set Password</a></p>
            ");

            if (!response.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, "Could not send email.");
                return View("/Views/AdminDashboard/Users/Create.cshtml", model);
            }

            TempData["SuccessMessage"] = "User created successfully!";
            return RedirectToAction("Index");
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            await SetUserProfilePictureAsync();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var student = await _studentRepository.GetByUserIdAsync(user.Id);

            var model = new EditUserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Role = roles.FirstOrDefault(),
                Roles = new List<string> { "Admin", "Employee", "Student" },

                Contact = student?.Contact,
                DateOfBirth = student?.DateOfBirth,
                Address = student?.Address,
                OfficialPhotoUrl = student?.OfficialPhotoUrl
            };

            return View("/Views/AdminDashboard/Users/Edit.cshtml", model);
        }

        [HttpPost("Edit/{id}")]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            model.Roles = new List<string> { "Admin", "Employee", "Student" };

            await SetUserProfilePictureAsync();

            if (!ModelState.IsValid)
                return View("/Views/AdminDashboard/Users/Edit.cshtml", model);

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
                return NotFound();

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.ProfilePictureUrl = model.ProfilePictureUrl;

            if (model.Role == "Student")
            {
                var student = await _studentRepository.GetByUserIdAsync(user.Id);
                if (student == null)
                {
                    student = new Student
                    {
                        UserId = user.Id
                    };
                    await _studentRepository.AddAsync(student);
                }

                student.Contact = model.Contact!;
                student.DateOfBirth = model.DateOfBirth ?? DateTime.MinValue;
                student.Address = model.Address;
                student.OfficialPhotoUrl = model.OfficialPhotoUrl;

                await _studentRepository.UpdateAsync(student);
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return View("/Views/AdminDashboard/Users/Edit.cshtml", model);
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, model.Role);

            TempData["SuccessMessage"] = "User updated successfully!";
            return RedirectToAction("Index");
        }
    }
}
