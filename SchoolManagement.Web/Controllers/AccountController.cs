using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Repository;
using SchoolManagement.Web.Helpers;
using SchoolManagement.Web.Models;
using System.Threading.Tasks;

namespace SchoolManagement.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IBlobHelper _blobHelper;
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;
        private readonly IStudentProfileRepository _studentProfileRepository;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IMailHelper mailHelper,
            IUserHelper userHelper,
            IBlobHelper blobHelper,
            IStudentProfileRepository studentProfileRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userHelper = userHelper;
            _blobHelper = blobHelper;
            _mailHelper = mailHelper;
            _studentProfileRepository = studentProfileRepository;
        }

        private async Task SetUserProfilePictureAsync()
        {
            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            ViewData["ProfilePictureUrl"] = user?.ProfilePictureUrl;
        }

        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
            if (result.Succeeded)
            {
                var user = await _userHelper.GetUserByEmailAsync(model.Email);
                if (await _userHelper.IsUserInRoleAsync(user, "Admin")) return RedirectToAction("Index", "AdminDashboard");
                if (await _userHelper.IsUserInRoleAsync(user, "Employee")) return RedirectToAction("Index", "EmployeeDashboard");
                if (await _userHelper.IsUserInRoleAsync(user, "Student")) return RedirectToAction("Index", "StudentDashboard");

                return RedirectToAction("Public", "Home");
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null)
                return BadRequest("Token and email are required.");

            return View(new ResetPasswordViewModel { Token = token, Email = email });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userHelper.GetUserByEmailAsync(model.Email);
            if (user == null) return RedirectToAction("ResetPasswordConfirmation");

            var result = await _userHelper.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                if (!user.EmailConfirmed)
                {
                    user.EmailConfirmed = true;
                    await _userManager.UpdateAsync(user);
                }

                TempData["SuccessMessage"] = "Password reset successfully.";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        [HttpGet]
        public IActionResult RecoverPassword() => View();

        [HttpPost]
        public async Task<IActionResult> RecoverPassword(RecoverPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userHelper.GetUserByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Email address not found.");
                return View(model);
            }

            var token = await _userHelper.GeneratePasswordResetTokenAsync(user);
            var link = Url.Action("ResetPassword", "Account", new { token, email = user.Email }, Request.Scheme);

            var response = _mailHelper.SendEmail(user.Email, "Reset Your Password", $@"
                <h2>Password Recovery</h2>
                <p>Click the link below to reset your password:</p>
                <p><a href='{link}'>Reset Password</a></p>");

            if (response.IsSuccess)
            {
                TempData["SuccessMessage"] = "Password reset instructions sent.";
                return RedirectToAction("Login");
            }

            ModelState.AddModelError("", "Error sending the email.");
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            await SetUserProfilePictureAsync();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            return View(new EditProfileViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                ProfilePictureUrl = user.ProfilePictureUrl
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

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

        [Authorize(Roles = "Student")]
        [HttpGet]
        public async Task<IActionResult> EditStudentProfile()
        {
            await SetUserProfilePictureAsync();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var profile = await _studentProfileRepository.GetByUserIdAsync(user.Id);
            if (profile == null) return NotFound();

            var model = new EditStudentProfileViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ProfilePictureUrl = user.ProfilePictureUrl,
                DateOfBirth = profile.DateOfBirth,
                Address = profile.Address,
                StudentClassId = profile.StudentClassId,
                OfficialPhotoUrl = profile.OfficialPhotoUrl
            };

            ViewBag.Classes = await _userHelper.GetClassesSelectListAsync(profile.StudentClassId);
            return View(model);
        }

        [Authorize(Roles = "Student")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStudentProfile(EditStudentProfileViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            var profile = await _studentProfileRepository.GetByUserIdAsync(user.Id);
            if (profile == null) return NotFound();

            if (model.OfficialPhoto == null && string.IsNullOrEmpty(profile.OfficialPhotoUrl))
            {
                ModelState.AddModelError(nameof(model.OfficialPhoto), "Official photo is required.");
            }

            if (!ModelState.IsValid)
            {
                model.OfficialPhotoUrl = profile.OfficialPhotoUrl;

                ViewBag.Classes = await _userHelper.GetClassesSelectListAsync(model.StudentClassId);
                return View(model);
            }

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            profile.DateOfBirth = model.DateOfBirth;
            profile.Address = model.Address;
            profile.StudentClassId = model.StudentClassId;

            if (model.ProfilePicture != null)
            {
                var blobId = await _blobHelper.UploadBlobAsync(model.ProfilePicture, "projetspictures");
                user.ProfilePictureUrl = blobId.ToString();
            }

            if (model.OfficialPhoto != null)
            {
                var blobId = await _blobHelper.UploadBlobAsync(model.OfficialPhoto, "projetspictures");
                profile.OfficialPhotoUrl = blobId.ToString();
            }

            await _userManager.UpdateAsync(user);
            await _studentProfileRepository.UpdateAsync(profile);

            TempData["SuccessMessage"] = "Student profile updated successfully.";
            await _signInManager.RefreshSignInAsync(user);
            return RedirectToAction(nameof(EditStudentProfile));
        }

        public IActionResult NotAuthorized()
        {
            return View("~/Views/Errors/403.cshtml");
        }
    }
}
