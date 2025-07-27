using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Repositories;
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

    /// <summary>
    /// Sets the currently logged-in user's profile picture URL into the ViewData for layout display.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task SetUserProfilePictureAsync()
    {
        var currentUser = await _userHelper.GetUserByEmailAsync(User.Identity?.Name ?? string.Empty);
        ViewData["ProfilePictureUrl"] = currentUser?.ProfilePictureUrl;
    }


    /// <summary>
    /// Displays a list of all Admin and Employee users.
    /// </summary>
    /// <returns>The index view with user list.</returns>
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

        //Views/AdminDashboard/Users/Index
        return View(model);
    }


    /// <summary>
    /// Deletes a user by ID after removing all associated roles.
    /// </summary>
    /// <param name="id">The ID of the user to delete.</param>
    /// <returns>Redirects to the user index with a status message.</returns>
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
            await _userHelper.RemoveUserFromRolesAsync(user, roles);

        var result = await _userHelper.DeleteUserAsync(user);

        TempData["SuccessMessage"] = result.Succeeded
            ? "User successfully removed."
            : "Error deleting user.";

        return RedirectToAction("Index");
    }


    /// <summary>
    /// Displays the form to create a new user.
    /// </summary>
    /// <returns>The create user view.</returns>
    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        await SetUserProfilePictureAsync();
        var model = new CreateUserViewModel { Roles = new List<string> { "Admin", "Employee" } };
        //Views/AdminDashboard/Users/Create
        return View(model);
    }


    /// <summary>
    /// Handles the creation of a new Admin or Employee user.
    /// </summary>
    /// <param name="model">The model containing new user data.</param>
    /// <returns>Redirects to index or reloads the form with errors.</returns>
    [HttpPost("Create")]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        model.Roles = new List<string> { "Admin", "Employee" };
        await SetUserProfilePictureAsync();

        if (!ModelState.IsValid)
            //Views/AdminDashboard/Users/Create
            return View(model);

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
                //Views/AdminDashboard/Users/Create
                return View(model);
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
            //Views/AdminDashboard/Users/Create
            return View(model);
        }

        TempData["SuccessMessage"] = "User successfully created! An email has been sent.";
        return RedirectToAction("Index");
    }


    /// <summary>
    /// Displays the form to edit an existing user.
    /// </summary>
    /// <param name="id">The ID of the user to edit.</param>
    /// <returns>The edit user view with pre-filled data.</returns>
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

        //Views/AdminDashboard/Users/Edit
        return View(model);
    }


    /// <summary>
    /// Handles updating an existing user's data and role.
    /// </summary>
    /// <param name="model">The model with updated user info.</param>
    /// <returns>Redirects to index or reloads the form on error.</returns>
    [HttpPost("Edit/{id}")]
    public async Task<IActionResult> Edit(EditUserViewModel model)
    {
        model.Roles = new List<string> { "Admin", "Employee" };
        await SetUserProfilePictureAsync();

        if (!ModelState.IsValid)
            //Views/AdminDashboard/Users/Edit
            return View(model);

        var user = await _userHelper.GetUserByIdAsync(model.Id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction(nameof(Index));
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
            //Views/AdminDashboard/Users/Edit
            return View(model);
        }

        TempData["SuccessMessage"] = "User successfully updated!";
        return RedirectToAction(nameof(Index));
    }


    /// <summary>
    /// Displays detailed information about a specific user.
    /// </summary>
    /// <param name="id">The ID of the user to view.</param>
    /// <returns>The user details view.</returns>
    [HttpGet("Details/{id}")]
    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrEmpty(id)) return NotFound();

        var user = await _userHelper.GetUserByIdAsync(id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction(nameof(Index));
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

        //Views/AdminDashboard/Users/Details
        return View(model);
    }
}
