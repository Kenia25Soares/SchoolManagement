using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Helpers;
using SchoolManagement.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[Authorize(Roles = "Admin")]
[Route("AdminDashboard/Users")]
public class UsersController : Controller
{
    private readonly IUserHelper _userHelper;
    private readonly IMailHelper _mailHelper;
    private readonly IBlobHelper _blobHelper;

    public UsersController(
        IUserHelper userHelper,
        IMailHelper mailHelper,
        IBlobHelper blobHelper)
    {
        _userHelper = userHelper;
        _mailHelper = mailHelper;
        _blobHelper = blobHelper;
    }

    private async Task SetUserProfilePictureAsync()
    {
        var currentUser = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
        ViewData["ProfilePictureUrl"] = currentUser?.ProfilePictureUrl;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        await SetUserProfilePictureAsync();
        var users = await _userHelper.GetAllUsersAsync();

        var model = new List<UserListViewModel>();
        foreach (var user in users)
        {
            var roles = await _userHelper.GetUserRolesAsync(user);
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

    [HttpPost("Delete/{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userHelper.GetUserByIdAsync(id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction("Index");
        }

        var roles = await _userHelper.GetUserRolesAsync(user);
        if (roles.Any())
        {
            await _userHelper.RemoveUserFromRolesAsync(user, roles);
        }

        var result = await _userHelper.DeleteUserAsync(user);
        TempData["SuccessMessage"] = result.Succeeded
            ? "User successfully removed."
            : "Error deleting user.";
        return RedirectToAction("Index");
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        await SetUserProfilePictureAsync();
        var model = new CreateUserViewModel { Roles = new List<string> { "Admin", "Employee" } };
        return View("/Views/AdminDashboard/Users/Create.cshtml", model);
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        model.Roles = new List<string> { "Admin", "Employee" };
        await SetUserProfilePictureAsync();

        if (!ModelState.IsValid)
            return View("/Views/AdminDashboard/Users/Create.cshtml", model);

        Guid blobId = Guid.Empty;
        if (model.ProfilePicture != null)
            blobId = await _blobHelper.UploadBlobAsync(model.ProfilePicture, "projetspictures");

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            PhoneNumber = model.PhoneNumber,
            ProfilePictureUrl = blobId == Guid.Empty ? null : blobId.ToString()
        };

        var result = await _userHelper.AddUserAsync(user, "Default123!");
        if (!result.Succeeded)
        {
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View("/Views/AdminDashboard/Users/Create.cshtml", model);
            }
        }

        await _userHelper.AddUserToRoleAsync(user, model.Role);

        var token = await _userHelper.GeneratePasswordResetTokenAsync(user);
        var resetLink = Url.Action("ResetPassword", "Account", new { token, email = user.Email }, Request.Scheme);

        var response = _mailHelper.SendEmail(user.Email, "Set your password",
            $@"<h1>Welcome to School Management!</h1><p>To set your password, click the link below:</p><p><a href='{resetLink}'>Set Password</a></p>");

        if (!response.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, "User created, but failed to send email.");
            return View("/Views/AdminDashboard/Users/Create.cshtml", model);
        }

        TempData["SuccessMessage"] = "User successfully created! An email has been sent.";
        return RedirectToAction("Index");
    }

    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(string id)
    {
        await SetUserProfilePictureAsync();
        var user = await _userHelper.GetUserByIdAsync(id);

        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction("Index");
        }

        var roles = await _userHelper.GetUserRolesAsync(user);

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

    [HttpPost("Edit/{id}")]
    public async Task<IActionResult> Edit(EditUserViewModel model)
    {
        model.Roles = new List<string> { "Admin", "Employee" };
        await SetUserProfilePictureAsync();

        if (!ModelState.IsValid)
            return View("/Views/AdminDashboard/Users/Edit.cshtml", model);

        var user = await _userHelper.GetUserByIdAsync(model.Id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
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

        var currentRoles = await _userHelper.GetUserRolesAsync(user);
        await _userHelper.RemoveUserFromRolesAsync(user, currentRoles);
        await _userHelper.AddUserToRoleAsync(user, model.Role);

        var result = await _userHelper.UpdateUserAsync(user);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Failed to update user.");
            return View("/Views/AdminDashboard/Users/Edit.cshtml", model);
        }

        TempData["SuccessMessage"] = "User successfully updated!";
        return RedirectToAction("Index");
    }

    [HttpGet("Details/{id}")]
    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrEmpty(id)) return NotFound();

        var user = await _userHelper.GetUserByIdAsync(id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction("Index");
        }

        var roles = await _userHelper.GetUserRolesAsync(user);

        var model = new CreateUserViewModel
        {
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = roles.FirstOrDefault() ?? string.Empty,
            ProfilePictureUrl = user.ProfilePictureUrl,
            Roles = new List<string> { "Admin", "Employee" }
        };

        return View("/Views/AdminDashboard/Users/Details.cshtml", model);
    }
}
