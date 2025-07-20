using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Helpers;
using SchoolManagement.Web.Models;
using SchoolManagement.Web.Data.Repositories;


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


        /// <summary>
        /// Displays the login view.
        /// </summary>
        public IActionResult Login() => View();


        /// <summary>
        /// Processes the login form and authenticates the user.
        /// </summary>
        /// <param name="model">Login form data.</param>
        /// <returns>Redirects based on user role or redisplays the form on failure.</returns>
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


        /// <summary>
        /// Logs the user out and redirects to the login page.
        /// </summary>
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Login");
        }


        /// <summary>
        /// Displays the password reset form.
        /// </summary>
        /// <param name="token">Password reset token.</param>
        /// <param name="email">User email address.</param>
        /// <returns>Password reset view.</returns>
        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null)
                return BadRequest("Token and email are required.");

            return View(new ResetPasswordViewModel { Token = token, Email = email });
        }


        /// <summary>
        /// Processes the password reset form.
        /// </summary>
        /// <param name="model">Password reset form data.</param>
        /// <returns>Redirects on success or redisplays the form with errors.</returns>
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


        /// <summary>
        /// Displays the recover password form.
        /// </summary>
        [HttpGet]
        public IActionResult RecoverPassword() => View();


        /// <summary>
        /// Sends a password reset link to the user's email.
        /// </summary>
        /// <param name="model">Recover password form data.</param>
        /// <returns>Redirects or redisplays the form based on result.</returns>
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


        /// <summary>
        /// Displays the profile edit form for general users.
        /// </summary>
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


        /// <summary>
        /// Updates the user's profile information.
        /// </summary>
        /// <param name="model">Profile data to update.</param>
        /// <returns>Redirects or redisplays the form with errors.</returns>
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


        /// <summary>
        /// Displays the student profile edit form.
        /// </summary>
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


        /// <summary>
        /// Updates the student profile with personal and academic info.
        /// </summary>
        /// <param name="model">Student profile data.</param>
        /// <returns>Redirects or redisplays the form with validation errors.</returns>
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


        /// <summary>
        /// Displays the 403 forbidden error page.
        /// </summary>
        public IActionResult NotAuthorized()
        {
            return View("~/Views/Errors/403.cshtml");
        }
    }
}
