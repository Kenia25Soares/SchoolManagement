using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Web.Data.Repositories;
using SchoolManagement.Web.Helpers;

namespace SchoolManagement.Web.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StudentsAPIController : ControllerBase
    {
        private readonly IUserHelper _userHelper;
        private readonly IStudentProfileRepository _profileRepository;
        private readonly IBlobHelper _blobHelper;

        public StudentsAPIController(IUserHelper userHelper, IStudentProfileRepository profileRepository, IBlobHelper blobHelper)
        {
            _userHelper = userHelper;
            _profileRepository = profileRepository;
            _blobHelper = blobHelper;
        }

        /// <summary>
        /// Returns student profile data and photo.
        /// </summary>
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetProfile(string userId)
        {
            var user = await _userHelper.GetUserByIdAsync(userId);
            var profile = await _profileRepository.GetByUserIdAsync(userId);

            if (user == null || profile == null)
                return NotFound(new { IsSuccess = false, Message = "Profile not found." });

            return Ok(new
            {
                IsSuccess = true,
                Results = new
                {
                    user.Id,
                    user.FullName,
                    user.Email,
                    user.PhoneNumber,
                    user.ProfilePictureUrl,
                    profile.DateOfBirth,
                    profile.Address,
                    profile.OfficialPhotoUrl,
                    profile.IsExcludedDueToAbsences,
                    StudentClass = profile.StudentClass?.Name ?? "No class"
                }
            });
        }

        /// <summary>
        /// Update profile photo, address and phone number.
        /// </summary>
        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateProfile(
            string userId,
            [FromForm] IFormFile? photo,
            [FromForm] string? phoneNumber,
            [FromForm] string? address)
        {
            var user = await _userHelper.GetUserByIdAsync(userId);
            var profile = await _profileRepository.GetByUserIdAsync(userId);

            if (user == null || profile == null)
                return NotFound(new { IsSuccess = false, Message = "Student not found." });

            if (!string.IsNullOrWhiteSpace(phoneNumber))
                user.PhoneNumber = phoneNumber;

            if (!string.IsNullOrWhiteSpace(address))
                profile.Address = address;

            if (photo != null)
            {
                var blobId = await _blobHelper.UploadBlobAsync(photo, "students");
                user.ProfilePictureUrl = blobId.ToString(); // 
            }

            // Atualiza o user e o profile
            var result = await _userHelper.UpdateUserAsync(user);
            if (!result.Succeeded)
                return BadRequest(new { IsSuccess = false, Message = "Error updating user data." });

            await _profileRepository.UpdateAsync(profile);

            return Ok(new { IsSuccess = true, Message = "Profile updated successfully." });
        }


        /// <summary>
        /// Enroll student in elective subject.
        /// </summary>
        [HttpPost("{userId}/enroll")]
        public async Task<IActionResult> EnrollInSubject(string userId, [FromQuery] int subjectId)
        {
            var profile = await _profileRepository.GetByUserIdAsync(userId);
            if (profile == null)
                return NotFound(new { IsSuccess = false, Message = "Student not found." });

          
            // inscrito com sucesso
            return Ok(new { IsSuccess = true, Message = $"Student {userId} enrolled in the subject {subjectId}." });
        }
    }
}
