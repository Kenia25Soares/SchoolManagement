using API.SchoolManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Repositories;
using SchoolManagement.Web.Helpers;
using SchoolManagement.Web.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace API.SchoolManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IBlobHelper _blobHelper;
        private readonly IMailHelper _mailHelper;
        private readonly IStudentProfileRepository _studentProfileRepository;

        private static readonly Dictionary<string, (string Code, DateTime Expiry)> _verificationCodes = new();

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IBlobHelper blobHelper,
            IMailHelper mailHelper,
            IStudentProfileRepository studentProfileRepository,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _blobHelper = blobHelper;
            _mailHelper = mailHelper;
            _studentProfileRepository = studentProfileRepository;
            _configuration = configuration;
        }

        private readonly IConfiguration _configuration;

        /// <summary>
        /// Get all users
        /// </summary>
        /// <returns>List of users</returns>
        [HttpGet("users")]
        public async Task<ActionResult<object>> GetUsers()
        {
            try
            {
                var users = await _userManager.Users.Take(10).Select(u => new
                {
                    Id = u.Id,
                    Email = u.Email,
                    FullName = u.FullName,
                    PhoneNumber = u.PhoneNumber
                }).ToListAsync();

                return Ok(new
                {
                    Success = true,
                    Users = users,
                    Count = users.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Error getting users",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get user by email
        /// </summary>
        /// <param name="email">Email to search</param>
        /// <returns>User info if found</returns>
        [HttpGet("user/{email}")]
        public async Task<ActionResult<object>> GetUserByEmail(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return NotFound(new { Message = "User not found" });
                }

                var roles = await _userManager.GetRolesAsync(user);

                return Ok(new
                {
                    Success = true,
                    User = new
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FullName = user.FullName,
                        PhoneNumber = user.PhoneNumber,
                        Roles = roles
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Error getting user",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Login endpoint
        /// </summary>
        /// <param name="model">Login credentials</param>
        /// <returns>Login result with user info</returns>
        [HttpPost("login")]
        public async Task<ActionResult<object>> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = "Invalid model state", Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
            }

            try
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    var roles = await _userManager.GetRolesAsync(user);

                    return Ok(new
                    {
                        Success = true,
                        Message = "Login successful",
                        User = new
                        {
                            Id = user.Id,
                            Email = user.Email,
                            FullName = user.FullName,
                            PhoneNumber = user.PhoneNumber,
                            Roles = roles
                        }
                    });
                }

                return Unauthorized(new { Success = false, Message = "Invalid login attempt" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Error during login",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Mobile login endpoint - accepts LoginRequest format
        /// </summary>
        /// <param name="request">Mobile login request</param>
        /// <returns>Login result with token</returns>
        [HttpPost("mobile-login")]
        public async Task<ActionResult<object>> MobileLogin([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = "Invalid model state" });
            }

            var result = await _signInManager.PasswordSignInAsync(request.email, request.password, request.rememberMe, false);
            if (!result.Succeeded)
            {
                return Unauthorized(new { Success = false, Message = "Invalid login attempt" });
            }

            var user = await _userManager.FindByEmailAsync(request.email);
            if (user == null)
            {
                return Unauthorized(new { Success = false, Message = "User not found" });
            }

            var roles = await _userManager.GetRolesAsync(user);

            // Verifique se o user tem o papel "Estudante"
            if (!roles.Contains("Student"))
            {
                return Forbid("Access denied. Only students are allowed to use the mobile app.");
            }

            // Gere um token JWT válido
            var claims = new[]
            {
        new Claim(ClaimTypes.Name, user.Email),
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, "Student")
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Issuer"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                Success = true,
                Token = tokenString,
                User = new
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    ProfilePictureUrl = user.ProfilePictureUrl,
                    ProfilePictureFullUrl = !string.IsNullOrEmpty(user.ProfilePictureUrl)
                        ? $"https://blobainek.blob.core.windows.net/projetspictures/{user.ProfilePictureUrl}"
                        : null,
                    Roles = roles
                }
            });

        }

        /// <summary>
        /// Logout endpoint
        /// </summary>
        /// <returns>Logout confirmation</returns>
        [HttpPost("logout")]
        public async Task<ActionResult<object>> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { Success = true, Message = "Logout successful" });
        }

        /// <summary>
        /// Get current user profile
        /// </summary>
        /// <returns>Current user information</returns>
        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<object>> GetProfile()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return NotFound(new { Message = "User not found" });

                var roles = await _userManager.GetRolesAsync(user);

                return Ok(new
                {
                    Success = true,
                    User = new
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FullName = user.FullName,
                        PhoneNumber = user.PhoneNumber,
                        ProfilePictureUrl = user.ProfilePictureUrl,
                        ProfilePictureFullUrl = !string.IsNullOrEmpty(user.ProfilePictureUrl)
                            ? $"https://blobainek.blob.core.windows.net/projetspictures/{user.ProfilePictureUrl}"
                            : null,
                        Roles = roles
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Error getting profile",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Update user profile
        /// </summary>
        /// <param name="model">Profile update data</param>
        /// <returns>Update result</returns>
        [HttpPut("profile")]
        [Authorize]
        public async Task<ActionResult<object>> UpdateProfile([FromBody] EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = "Invalid model state", Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
            }

            try
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                    return NotFound(new { Message = "User not found" });

                user.FullName = model.FullName;
                user.Email = model.Email;
                user.UserName = model.Email;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Failed to update profile",
                        Errors = result.Errors.Select(e => e.Description)
                    });
                }

                return Ok(new { Success = true, Message = "Profile updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Error updating profile",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Update full profile (user + student data) and optional images in a single form
        /// </summary>
        /// <param name="model">Form data with fields and optional images</param>
        /// <returns>Updated profile summary with full image URLs</returns>
        [HttpPut("profile/full")]
        [Authorize]
        public async Task<ActionResult<object>> UpdateFullProfile([FromForm] API.SchoolManagement.Models.UpdateFullProfileRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = "Invalid model state", Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
            }

            try
            {
                var emailIdentity = User.Identity?.Name;
                if (string.IsNullOrEmpty(emailIdentity))
                {
                    return Unauthorized(new { Success = false, Message = "User not authenticated" });
                }

                var user = await _userManager.FindByEmailAsync(emailIdentity);
                if (user == null)
                {
                    return NotFound(new { Success = false, Message = "User not found" });
                }

                // Update basic user fields
                user.FullName = model.FullName;
                if (!string.IsNullOrWhiteSpace(model.PhoneNumber))
                {
                    user.PhoneNumber = model.PhoneNumber;
                }

                if (!string.IsNullOrWhiteSpace(model.Email) && !string.Equals(model.Email, user.Email, StringComparison.OrdinalIgnoreCase))
                {
                    var existing = await _userManager.FindByEmailAsync(model.Email);
                    if (existing != null)
                    {
                        return BadRequest(new { Success = false, Message = "Email already in use." });
                    }

                    var setEmail = await _userManager.SetEmailAsync(user, model.Email);
                    if (!setEmail.Succeeded)
                    {
                        return BadRequest(new { Success = false, Message = "Failed to update email.", Errors = setEmail.Errors.Select(e => e.Description) });
                    }

                    var setUserName = await _userManager.SetUserNameAsync(user, model.Email);
                    if (!setUserName.Succeeded)
                    {
                        return BadRequest(new { Success = false, Message = "Failed to update username.", Errors = setUserName.Errors.Select(e => e.Description) });
                    }
                }

                // Load or create student profile
                var studentProfile = await _studentProfileRepository.GetByUserIdAsync(user.Id);
                if (studentProfile == null)
                {
                    studentProfile = new StudentProfile
                    {
                        UserId = user.Id,
                        IsExcludedDueToAbsences = false
                    };
                    await _studentProfileRepository.CreateAsync(studentProfile);
                }

                // Update student fields
                if (model.DateOfBirth.HasValue)
                {
                    studentProfile.DateOfBirth = model.DateOfBirth.Value;
                }
                if (!string.IsNullOrWhiteSpace(model.Address))
                {
                    studentProfile.Address = model.Address;
                }

                // Blob config
                var blobBaseUrl = _configuration["Blob:BaseUrl"] ?? "https://blobainek.blob.core.windows.net";
                var blobContainer = _configuration["Blob:Container"] ?? "projetspictures";

                // Validate and upload images if provided
                bool IsValidImage(IFormFile f) => f != null && (f.ContentType == "image/jpeg" || f.ContentType == "image/png");

                if (model.ProfilePicture != null)
                {
                    if (!IsValidImage(model.ProfilePicture))
                    {
                        return BadRequest(new { Success = false, Message = "Invalid profile picture format. Only JPEG/PNG allowed." });
                    }

                    var profileBlobId = await _blobHelper.UploadBlobAsync(model.ProfilePicture, blobContainer);
                    user.ProfilePictureUrl = profileBlobId.ToString();
                }

                if (model.OfficialPhoto != null)
                {
                    if (!IsValidImage(model.OfficialPhoto))
                    {
                        return BadRequest(new { Success = false, Message = "Invalid official photo format. Only JPEG/PNG allowed." });
                    }

                    var officialBlobId = await _blobHelper.UploadBlobAsync(model.OfficialPhoto, blobContainer);
                    studentProfile.OfficialPhotoUrl = officialBlobId.ToString();
                }

                // Persist
                var userResult = await _userManager.UpdateAsync(user);
                if (!userResult.Succeeded)
                {
                    return BadRequest(new { Success = false, Message = "Failed to update user", Errors = userResult.Errors.Select(e => e.Description) });
                }
                await _studentProfileRepository.UpdateAsync(studentProfile);

                var fullProfileUrl = !string.IsNullOrEmpty(user.ProfilePictureUrl) ? $"{blobBaseUrl}/{blobContainer}/{user.ProfilePictureUrl}" : null;
                var fullOfficialUrl = !string.IsNullOrEmpty(studentProfile.OfficialPhotoUrl) ? $"{blobBaseUrl}/{blobContainer}/{studentProfile.OfficialPhotoUrl}" : null;

                return Ok(new
                {
                    Success = true,
                    Message = "Profile updated successfully",
                    User = new
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FullName = user.FullName,
                        PhoneNumber = user.PhoneNumber,
                        ProfilePictureUrl = user.ProfilePictureUrl,
                        ProfilePictureFullUrl = fullProfileUrl
                    },
                    Student = new
                    {
                        DateOfBirth = studentProfile.DateOfBirth,
                        Address = studentProfile.Address,
                        OfficialPhotoUrl = studentProfile.OfficialPhotoUrl,
                        OfficialPhotoFullUrl = fullOfficialUrl
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Error updating full profile",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Recover password endpoint (Web)
        /// </summary>
        /// <param name="model">Email for password recovery</param>
        /// <returns>Recovery result</returns>
        [HttpPost("recover-password")]
        public async Task<ActionResult<object>> RecoverPassword([FromBody] RecoverPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = "Invalid model state", Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
            }

            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return NotFound(new { Message = "Email address not found" });
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var webAppBaseUrl = _configuration["WebApp:BaseUrl"] ?? "https://localhost:7176";
                var resetLink = $"{webAppBaseUrl}/Account/ResetPassword?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(user.Email)}";

                var response = _mailHelper.SendEmail(user.Email, "Reset Your Password", $@"
                    <h2>Password Recovery</h2>
                    <p>Click the link below to reset your password:</p>
                    <p><a href='{resetLink}'>Reset Password</a></p>");

                if (response.IsSuccess)
                {
                    return Ok(new { Success = true, Message = "Password reset instructions sent to your email" });
                }

                return BadRequest(new { Success = false, Message = "Error sending the email" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Error during password recovery",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Send verification code endpoint (Mobile)
        /// </summary>
        /// <param name="model">Email for verification code</param>
        /// <returns>Send result</returns>
        [HttpPost("send-verification-code")]
        public async Task<ActionResult<object>> SendVerificationCode([FromBody] SendVerificationCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = "Invalid model state", Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
            }

            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return NotFound(new { Message = "Email address not found" });
                }

                // Generate a 6-digit verification code
                var code = new Random().Next(100000, 999999).ToString();
                var expiry = DateTime.UtcNow.AddMinutes(10);

                // Store the code with expiry
                _verificationCodes[model.Email] = (code, expiry);

                var response = _mailHelper.SendEmail(user.Email, "Verification Code", $@"
                    <h2>Password Reset Verification</h2>
                    <p>Your verification code is: <strong>{code}</strong></p>
                    <p>This code will expire in 10 minutes.</p>
                    <p>If you didn't request this, please ignore this email.</p>");

                if (response.IsSuccess)
                {
                    return Ok(new { Success = true, Message = "Verification code sent to your email" });
                }

                return BadRequest(new { Success = false, Message = "Error sending the verification code" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Error during verification code sending",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Verify code endpoint (Mobile)
        /// </summary>
        /// <param name="model">Email and code for verification</param>
        /// <returns>Verification result</returns>
        [HttpPost("verify-code")]
        public async Task<ActionResult<object>> VerifyCode([FromBody] VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = "Invalid model state", Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
            }

            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return NotFound(new { Message = "User not found" });
                }

                // Check if verification code exists and is valid
                if (!_verificationCodes.TryGetValue(model.Email, out var storedData))
                {
                    return BadRequest(new { Success = false, Message = "No verification code found. Please request a new code." });
                }

                var (storedCode, expiry) = storedData;

                // Check if code has expired
                if (DateTime.UtcNow > expiry)
                {
                    _verificationCodes.Remove(model.Email);
                    return BadRequest(new { Success = false, Message = "Verification code has expired. Please request a new code." });
                }

                // Check if code matches
                if (model.Code != storedCode)
                {
                    return BadRequest(new { Success = false, Message = "Invalid verification code" });
                }

                return Ok(new { Success = true, Message = "Code verified successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Error during code verification",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Reset password with code endpoint (Mobile)
        /// </summary>
        /// <param name="model">Email, code and new password</param>
        /// <returns>Reset result</returns>
        [HttpPost("reset-password-with-code")]
        public async Task<ActionResult<object>> ResetPasswordWithCode([FromBody] ResetPasswordWithCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = "Invalid model state", Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                return BadRequest(new { Success = false, Message = "The password and confirmation password do not match." });
            }

            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return NotFound(new { Message = "User not found" });
                }

                // Check if verification code exists and is valid
                if (!_verificationCodes.TryGetValue(model.Email, out var storedData))
                {
                    return BadRequest(new { Success = false, Message = "No verification code found. Please request a new code." });
                }

                var (storedCode, expiry) = storedData;

                // Check if code has expired
                if (DateTime.UtcNow > expiry)
                {
                    _verificationCodes.Remove(model.Email);
                    return BadRequest(new { Success = false, Message = "Verification code has expired. Please request a new code." });
                }

                // Check if code matches
                if (model.Code != storedCode)
                {
                    return BadRequest(new { Success = false, Message = "Invalid verification code" });
                }

                // Code is valid, proceed with password reset
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

                if (result.Succeeded)
                {
                    // Remove the used code
                    _verificationCodes.Remove(model.Email);
                    return Ok(new { Success = true, Message = "Password reset successfully" });
                }

                return BadRequest(new
                {
                    Success = false,
                    Message = "Failed to reset password",
                    Errors = result.Errors.Select(e => e.Description),
                    Details = "Password validation failed. Check password requirements."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Error during password reset",
                    Error = ex.Message,
                    Details = "Check if the password meets the requirements: minimum 6 characters"
                });
            }
        }

        /// <summary>
        /// Update password endpoint (Mobile) - requires authentication
        /// </summary>
        /// <param name="model">New password data</param>
        /// <returns>Update result</returns>
        [HttpPost("update-password")]
        [Authorize]
        public async Task<ActionResult<object>> UpdatePassword([FromBody] API.SchoolManagement.Models.UpdatePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = "Invalid model state", Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
            }

            try
            {
                // Get user from email , Obtem e-mail do user autenticado
                var email = User.Identity?.Name;

                if (string.IsNullOrEmpty(email))
                {
                    return Unauthorized(new { Success = false, Message = "User not authenticated." });
                }

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return NotFound(new { Success = false, Message = "User not found." });
                }

                if (model.NewPassword != model.ConfirmPassword)
                {
                    return BadRequest(new { Success = false, Message = "New password and confirmation do not match." });
                }

                var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

                if (result.Succeeded)
                {
                    return Ok(new { Success = true, Message = "Password changed successfully." });
                }

                if (result.Succeeded)
                {
                    return Ok(new
                    {
                        Success = true,
                        Message = "Password updated successfully! You can now use your new password to login.",
                        Details = "Your password has been changed successfully. Please remember to use the new password for future logins."
                    });
                }

                return BadRequest(new
                {
                    Success = false,
                    Message = "Failed to update password. Please check the requirements below.",
                    Errors = result.Errors.Select(e => e.Description),
                    Details = "Password must be at least 6 characters long. Try using a stronger password with letters, numbers, and symbols."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Error during password update. Please try again.",
                    Error = ex.Message,
                    Details = "An unexpected error occurred. Please ensure your password meets the requirements and try again."
                });
            }
        }

        /// <summary>
        /// Get complete student profile with official photo
        /// </summary>
        /// <returns>Complete student information including official photo</returns>
        [HttpGet("student-profile")]
        [Authorize]
        public async Task<ActionResult<object>> GetStudentProfile()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return NotFound(new { Message = "User not found" });

                var roles = await _userManager.GetRolesAsync(user);

                // Check if user is a student
                if (!roles.Contains("Student"))
                {
                    return Forbid("Access denied. Only students can access this endpoint.");
                }

                // Get student profile data
                var studentProfile = await _studentProfileRepository.GetByUserIdAsync(user.Id);

                return Ok(new
                {
                    Success = true,
                    Student = new
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FullName = user.FullName,
                        PhoneNumber = user.PhoneNumber,
                        ProfilePictureUrl = user.ProfilePictureUrl,
                        OfficialPhotoUrl = studentProfile?.OfficialPhotoUrl,
                        DateOfBirth = studentProfile?.DateOfBirth,
                        Address = studentProfile?.Address,
                        IsExcludedDueToAbsences = studentProfile?.IsExcludedDueToAbsences ?? false,
                        StudentClass = studentProfile?.StudentClass != null ? new
                        {
                            Id = studentProfile.StudentClass.Id,
                            Name = studentProfile.StudentClass.Name,
                            IsClosed = studentProfile.StudentClass.IsClosed
                        } : null,
                        Roles = roles
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Error getting student profile",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get complete student profile with official photo and full URLs
        /// </summary>
        /// <returns>Complete student information including full image URLs</returns>
        [HttpGet("student-profile-full")]
        [Authorize]
        public async Task<ActionResult<object>> GetStudentProfileWithFullUrls()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return NotFound(new { Message = "User not found" });

                var roles = await _userManager.GetRolesAsync(user);

                // Check if user is a student
                if (!roles.Contains("Student"))
                {
                    return Forbid("Access denied. Only students can access this endpoint.");
                }

                // Get student profile 
                var studentProfile = await _studentProfileRepository.GetByUserIdAsync(user.Id);

                // Generate full URLs for images
                var fullProfilePictureUrl = !string.IsNullOrEmpty(user.ProfilePictureUrl)
                    ? $"https://blobainek.blob.core.windows.net/projetspictures/{user.ProfilePictureUrl}"
                    : null;

                var fullOfficialPhotoUrl = !string.IsNullOrEmpty(studentProfile?.OfficialPhotoUrl)
                    ? $"https://blobainek.blob.core.windows.net/projetspictures/{studentProfile.OfficialPhotoUrl}"
                    : null;

                return Ok(new
                {
                    Success = true,
                    Student = new
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FullName = user.FullName,
                        PhoneNumber = user.PhoneNumber,
                        ProfilePictureUrl = user.ProfilePictureUrl,
                        ProfilePictureFullUrl = fullProfilePictureUrl,
                        OfficialPhotoUrl = studentProfile?.OfficialPhotoUrl,
                        OfficialPhotoFullUrl = fullOfficialPhotoUrl,
                        DateOfBirth = studentProfile?.DateOfBirth,
                        Address = studentProfile?.Address,
                        IsExcludedDueToAbsences = studentProfile?.IsExcludedDueToAbsences ?? false,
                        StudentClass = studentProfile?.StudentClass != null ? new
                        {
                            Id = studentProfile.StudentClass.Id,
                            Name = studentProfile.StudentClass.Name,
                            IsClosed = studentProfile.StudentClass.IsClosed
                        } : null,
                        Roles = roles
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Error getting student profile",
                    Error = ex.Message
                });
            }
        }


    }
}
