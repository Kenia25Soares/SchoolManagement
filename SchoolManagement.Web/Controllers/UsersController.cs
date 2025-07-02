using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Helpers;
using SchoolManagement.Web.Models;

/// <summary>
/// Controlador responsável pela gestão de usuários Admins no painel do administrador.
/// Inclui ações para listar, criar, editar, excluir e visualizar utilizador.
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
    /// Define a imagem de perfil do utilizador logado para exibição no layout.
    /// </summary>
    private async Task SetUserProfilePictureAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        ViewData["ProfilePictureUrl"] = user?.ProfilePictureUrl;
    }

    /// <summary>
    /// Lista todos os utilizadores com perfil de Admin ou Funcionário.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        await SetUserProfilePictureAsync();
        var users = await _userManager.Users.ToListAsync();
        var model = new List<UserListViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin") || roles.Contains("Employee"))
            {
                model.Add(new UserListViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = roles.FirstOrDefault() ?? "N/A",
                    ProfilePictureUrl = user.ProfilePictureUrl
                });
            }
        }

        return View("/Views/AdminDashboard/Users/Index.cshtml", model);
    }

    /// <summary>
    /// Deleta um utilizador com base no ID fornecido.
    /// </summary>
    [HttpPost("Delete/{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user != null)
        {
            await _userManager.DeleteAsync(user);
            TempData["SuccessMessage"] = "Utilizador removido com sucesso.";
        }
        else
        {
            TempData["ErrorMessage"] = "Utilizador não encontrado.";
        }

        return RedirectToAction("Index");
    }

    /// <summary>
    /// Exibe o formulário para criação de um novo utilizador.
    /// </summary>
    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        await SetUserProfilePictureAsync();
        var model = new CreateUserViewModel
        {
            Roles = new List<string> { "Admin", "Employee" }
        };
        return View("/Views/AdminDashboard/Users/Create.cshtml", model);
    }

    /// <summary>
    /// Cria um novo utilizador com base nos dados enviados do formulário.
    /// </summary>
    [HttpPost("Create")]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        model.Roles = new List<string> { "Admin", "Employee" };
        await SetUserProfilePictureAsync();

        if (!ModelState.IsValid)
            return View("/Views/AdminDashboard/Users/Create.cshtml", model);

        Guid blobId = Guid.Empty;
        if (model.ProfilePicture != null)
        {
            blobId = await _blobHelper.UploadBlobAsync(model.ProfilePicture, "projetspictures");
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            PhoneNumber = model.PhoneNumber,
            ProfilePictureUrl = blobId == Guid.Empty ? null : blobId.ToString()
        };

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

        var response = _mailHelper.SendEmail(user.Email, "Definir password", $@"
            <h1>Bem-vindo ao School Management!</h1>
            <p>Para definir sua senha, clique no link abaixo:</p>
            <p><a href='{resetLink}'>Definir senha</a></p>
        ");

        if (!response.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, "Utilizador criado, mas falha ao enviar email.");
            return View("/Views/AdminDashboard/Users/Create.cshtml", model);
        }

        TempData["SuccessMessage"] = "Utilizador criado com sucesso! Um email foi enviado.";
        return RedirectToAction("Index");
    }

    /// <summary>
    /// Exibe o formulário para editar um utilizador existente.
    /// </summary>
    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(string id)
    {
        await SetUserProfilePictureAsync();

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "Utilizador não encontrado.";
            return RedirectToAction("Index");
        }

        var roles = await _userManager.GetRolesAsync(user);

        var model = new EditUserViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            ProfilePictureUrl = user.ProfilePictureUrl,
            PhoneNumber = user.PhoneNumber,
            Role = roles.FirstOrDefault(),
            Roles = new List<string> { "Admin", "Employee" }
        };

        return View("/Views/AdminDashboard/Users/Edit.cshtml", model);
    }

    /// <summary>
    /// Salva as alterações feitas em um utilizador.
    /// </summary>
    [HttpPost("Edit/{id}")]
    public async Task<IActionResult> Edit(EditUserViewModel model)
    {
        model.Roles = new List<string> { "Admin", "Employee" };
        await SetUserProfilePictureAsync();

        if (!ModelState.IsValid)
            return View("/Views/AdminDashboard/Users/Edit.cshtml", model);

        var user = await _userManager.FindByIdAsync(model.Id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "Utilizador não encontrado.";
            return RedirectToAction("Index");
        }

        user.FullName = model.FullName;
        user.Email = model.Email;
        user.UserName = model.Email;
        user.PhoneNumber = model.PhoneNumber;

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

            return View("/Views/AdminDashboard/Users/Edit.cshtml", model);
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, model.Role);

        TempData["SuccessMessage"] = "Utilizador atualizado com sucesso!";
        return RedirectToAction("Index");
    }

    /// <summary>
    /// Exibe os detalhes específicos de um utilizador.
    /// </summary>
    [HttpGet("Details/{id}")]
    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrEmpty(id))
            return NotFound();

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "Utilizador não encontrado.";
            return RedirectToAction("Index");
        }

        var roles = await _userManager.GetRolesAsync(user);

        var model = new CreateUserViewModel
        {
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = roles.FirstOrDefault() ?? "",
            ProfilePictureUrl = user.ProfilePictureUrl,
            Roles = new List<string> { "Admin", "Employee" }
        };

        return View("/Views/AdminDashboard/Users/Details.cshtml", model);
    }
}
