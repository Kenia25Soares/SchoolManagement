using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Helpers;
using SchoolManagement.Web.Models;
using System.Threading.Tasks;

namespace SchoolManagement.Web.Controllers.API
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IBlobHelper _blobHelper;
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IMailHelper mailHelper,
            IUserHelper userHelper,
            IBlobHelper blobHelper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userHelper = userHelper;
            _blobHelper = blobHelper;
            _mailHelper = mailHelper;
        }

        // GET: Login
        public IActionResult Login() => View();

        // POST: Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
            if (result.Succeeded)
            {
                var user = await _userHelper.GetUserByEmailAsync(model.Email);

                if (await _userHelper.IsUserInRoleAsync(user, "Admin"))
                    return RedirectToAction("Index", "AdminDashboard");

                if (await _userHelper.IsUserInRoleAsync(user, "Employee"))
                    return RedirectToAction("Index", "EmployeeDashboard");

                if (await _userHelper.IsUserInRoleAsync(user, "Student"))
                    return RedirectToAction("Index", "StudentDashboard");

                return RedirectToAction("Public", "Home");
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }

        // GET: Logout
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        // GET: ResetPassword
        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null)
                return BadRequest("Token and email are required.");

            var model = new ResetPasswordViewModel
            {
                Token = token,
                Email = email
            };

            return View(model);
        }

        // POST: ResetPassword
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userHelper.GetUserByEmailAsync(model.Email);
            if (user == null)
                return RedirectToAction("ResetPasswordConfirmation");

            var result = await _userHelper.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Password definida com sucesso. Pode agora iniciar sessão.";
                return RedirectToAction("Login", "Account");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        // GET: RecoverPassword
        [HttpGet]
        public IActionResult RecoverPassword()
        {
            return View();
        }

        // POST: RecoverPassword
        [HttpPost]
        public async Task<IActionResult> RecoverPassword(RecoverPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userHelper.GetUserByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Email não encontrado.");
                return View(model);
            }

            var token = await _userHelper.GeneratePasswordResetTokenAsync(user);
            var link = Url.Action("ResetPassword", "Account", new { token, email = user.Email }, Request.Scheme);

            var response = _mailHelper.SendEmail(user.Email, "Recuperar Password", $@"
                <h2>Recuperar Password</h2>
                <p>Clique no link abaixo para definir uma nova password:</p>
                <p><a href='{link}'>Resetar Password</a></p>
            ");

            if (response.IsSuccess)
                ViewBag.Message = "As instruções foram enviadas para o seu email.";
            else
                ModelState.AddModelError(string.Empty, "Erro ao enviar o email.");

            return View();
        }

        // GET: Edit profile (Admin/Employee)
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var model = new EditProfileViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                ProfilePictureUrl = user.ProfilePictureUrl
            };

            return View(model);
        }

        // POST: Edit profile (Admin/Employee)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UserName = model.Email;

            if (model.ProfilePicture != null)
            {
                var blobId = await _blobHelper.UploadBlobAsync(model.ProfilePicture, "projetspictures");
                user.ProfilePictureUrl = blobId.ToString();
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError("", e.Description);
                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);

            TempData["SuccessMessage"] = "Profile updated successfully.";
            return RedirectToAction(nameof(EditProfile));
        }

        // GET: Edit student profile (StudentUser)
        [HttpGet]
        public async Task<IActionResult> EditStudentProfile()
        {
            var user = await _userManager.GetUserAsync(User) as StudentUser;
            if (user == null) return NotFound();

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

            ViewBag.Classes = await _userHelper.GetClassesSelectListAsync(user.StudentClassId);

            return View(model);
        }

        // POST: Edit student profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStudentProfile(EditStudentProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Classes = await _userHelper.GetClassesSelectListAsync(model.StudentClassId);
                return View(model);
            }

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
                var blobId = await _blobHelper.UploadBlobAsync(model.ProfilePicture, "projetspictures");
                user.ProfilePictureUrl = blobId.ToString();
            }

            if (model.OfficialPhoto != null)
            {
                var blobId = await _blobHelper.UploadBlobAsync(model.OfficialPhoto, "projetspictures");
                user.OfficialPhotoUrl = blobId.ToString();
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError("", e.Description);

                ViewBag.Classes = await _userHelper.GetClassesSelectListAsync(model.StudentClassId);
                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);

            TempData["SuccessMessage"] = "Profile updated successfully.";
            return RedirectToAction(nameof(EditStudentProfile));
        }
    }
}
