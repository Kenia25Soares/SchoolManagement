using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Helpers;
using SchoolManagement.Web.Models;

namespace SchoolManagement.Web.Controllers.API
{
    /// <summary>
    /// Controller de gestão de utilizadores (Apenas Admin)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [Route("AdminDashboard/Users")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMailHelper _mailHelper;
        private readonly IBlobHelper _blobHelper;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IMailHelper mailHelper,
            IBlobHelper blobHelper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mailHelper = mailHelper;
            _blobHelper = blobHelper;
        }

        /// <summary>
        /// Define foto de perfil do Admin para o layout.
        /// </summary>
        private async Task SetUserProfilePictureAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewData["ProfilePictureUrl"] = user?.ProfilePictureUrl;
        }

        // GET: Listagem de utilizadores
        [HttpGet]
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
                    Role = roles.FirstOrDefault() ?? "N/A",
                    ProfilePictureUrl = user.ProfilePictureUrl
                });
            }

            return View("/Views/AdminDashboard/Users/Index.cshtml", model);
        }

        // POST: Apagar utilizador
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

        // GET: Formulário de criação
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

        // POST: Criar novo utilizador
        [HttpPost("Create")]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            model.Roles = new List<string> { "Admin", "Employee", "Student" };
            await SetUserProfilePictureAsync();

            if (!ModelState.IsValid)
                return View("/Views/AdminDashboard/Users/Create.cshtml", model);

            Guid blobId = Guid.Empty;
            if (model.ProfilePicture != null)
            {
                blobId = await _blobHelper.UploadBlobAsync(model.ProfilePicture, "projetspictures");
                Console.WriteLine($"✅ IMAGEM GUARDADA NO BLOB: {blobId}");
            }
            else
            {
                Console.WriteLine("❌ NENHUMA IMAGEM FOI ENVIADA");
            }
            ApplicationUser user;

            if (model.Role == "Student")
            {
                user = new StudentUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    ProfilePictureUrl = blobId == Guid.Empty ? null : blobId.ToString(),
                    DateOfBirth = model.DateOfBirth ?? DateTime.MinValue,
                    Address = model.Address,
                    OfficialPhotoUrl = model.OfficialPhotoUrl,
                    IsExcludedDueToAbsences = false,
                    CourseId = model.CourseId
                };
            }
            else
            {
                user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    ProfilePictureUrl = blobId == Guid.Empty ? null : blobId.ToString()
                };
            }

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View("/Views/AdminDashboard/Users/Create.cshtml", model);
            }

            await _userManager.AddToRoleAsync(user, model.Role);

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

        // GET: Formulário de edição
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            await SetUserProfilePictureAsync();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            var model = new EditUserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                ProfilePictureUrl = user.ProfilePictureUrl,
                PhoneNumber = user.PhoneNumber,
                Role = roles.FirstOrDefault(),
                Roles = new List<string> { "Admin", "Employee", "Student" }
            };

            if (user is StudentUser student)
            {
                model.DateOfBirth = student.DateOfBirth;
                model.Address = student.Address;
                model.OfficialPhotoUrl = student.OfficialPhotoUrl;
                model.CourseId = student.CourseId;
            }

            return View("/Views/AdminDashboard/Users/Edit.cshtml", model);
        }

        // POST: Atualizar utilizador
        [HttpPost("Edit/{id}")]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            model.Roles = new List<string> { "Admin", "Employee", "Student" };
            await SetUserProfilePictureAsync();

            if (!ModelState.IsValid)
                return View("/Views/AdminDashboard/Users/Edit.cshtml", model);

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            if (model.ProfilePicture != null)
            {
                Guid blobId = await _blobHelper.UploadBlobAsync(model.ProfilePicture, "projetspictures");
                user.ProfilePictureUrl = blobId.ToString();
            }

            if (model.Role == "Student")
            {
                if (user is StudentUser student)
                {
                    student.DateOfBirth = model.DateOfBirth ?? DateTime.MinValue;
                    student.Address = model.Address;
                    student.OfficialPhotoUrl = model.OfficialPhotoUrl;
                    student.CourseId = model.CourseId;
                }
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
