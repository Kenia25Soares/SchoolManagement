using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SchoolManagement.Web.Helpers;
using SchoolManagement.Web.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SchoolManagement.Web.Controllers.API
{
    /// <summary>
    /// API controller responsible for user authentication and JWT token generation.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AccountAPIController : ControllerBase
    {
        private readonly IUserHelper _userHelper;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Constructor that injects user helper and configuration.
        /// </summary>
        /// <param name="userHelper">Service for user operations</param>
        /// <param name="configuration">Application configuration</param>
        public AccountAPIController(IUserHelper userHelper, IConfiguration configuration)
        {
            _userHelper = userHelper;
            _configuration = configuration;
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token if successful.
        /// </summary>
        /// <param name="model">Login credentials</param>
        /// <returns>A JWT token and expiration date</returns>
        /// <response code="200">Returns the token and expiration</response>
        /// <response code="400">If the model is invalid</response>
        /// <response code="401">If authentication fails</response>
        [HttpPost("Login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid login data.");

            var user = await _userHelper.GetUserByEmailAsync(model.Email);
            if (user == null)
                return Unauthorized("User not found.");

            var result = await _userHelper.ValidatePasswordAsync(user, model.Password);
            if (!result.Succeeded)
                return Unauthorized("Invalid credentials.");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, "Employee") // replace with actual role if necessary
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _configuration["JWT:Issuer"],
                _configuration["JWT:Audience"],
                claims,
                expires: DateTime.UtcNow.AddDays(15),
                signingCredentials: credentials
            );

            var results = new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo
            };

            return Ok(results);
        }
    }
}
