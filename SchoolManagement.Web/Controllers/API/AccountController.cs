using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Helpers;
using SchoolManagement.Web.Models;

namespace SchoolManagement.Web.Controllers.API
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;

        public AccountController(SignInManager<ApplicationUser> signInManager, IMailHelper mailHelper, IUserHelper userHelper)
        {
            _signInManager = signInManager;
            _userHelper = userHelper;
            _mailHelper = mailHelper;
        }


        /// <summary>
        /// Abre o formulário de login.
        /// </summary>
        // GET
        public IActionResult Login() => View();



        /// <summary>
        /// Faz o login do utilizador.
        /// </summary>
        // POST
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


        /// <summary>
        /// Faz o logout do utilizador.
        /// </summary>
        // GET
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }


        /// <summary>
        /// Abre o formulário de reset de password.
        /// </summary>
        // GET
        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null)
            {
                return BadRequest("Token and email are required.");
            }

            var model = new ResetPasswordViewModel
            {
                Token = token,
                Email = email
            };

            return View(model);
        }


        /// <summary>
        /// Submete o reset de password.
        /// </summary>
        // POST
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userHelper.GetUserByEmailAsync(model.Email);
            if (user == null)
            {
                return RedirectToAction("ResetPasswordConfirmation");
            }

            var result = await _userHelper.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Password definida com sucesso. Pode agora iniciar sessão.";
                return RedirectToAction("Login", "Account");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }


        /// <summary>
        /// Abre o formulário de recuperação de password.
        /// </summary>
        // GET
        [HttpGet]
        public IActionResult RecoverPassword()
        {
            return View();
        }


        /// <summary>
        /// Submete a recuperação de password e envia o email.
        /// </summary>
        // POST
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
            {
                ViewBag.Message = "As instruções foram enviadas para o seu email.";
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Erro ao enviar o email.");
            }

            return View();
        }
    }
}
