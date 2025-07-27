using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Data.Repositories;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Repositories;
using SchoolManagement.Web.Models;

namespace SchoolManagement.Web.Helpers
{
    public class StudentClassHelper : IStudentClassHelper
    {
        private readonly IStudentClassRepository _studentClassRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IStudentProfileRepository _studentProfileRepository;

        public StudentClassHelper(
            IStudentClassRepository studentClassRepository,
            ICourseRepository courseRepository,
            IStudentProfileRepository studentProfileRepository)
        {
            _studentClassRepository = studentClassRepository;
            _courseRepository = courseRepository;
            _studentProfileRepository = studentProfileRepository;
        }


        public async Task<List<StudentClassViewModel>> GetAllAsync()
        {
            var classes = await _studentClassRepository.GetAll()
                                                        .Include(c => c.Course)
                                                        .Include(c => c.Students)
                                                        .OrderBy(c => c.Name)
                                                        .ToListAsync();

            return classes.Select(c => new StudentClassViewModel
            {
                Id = c.Id,
                Name = c.Name,
                AcademicYear = c.AcademicYear,
                Shift = c.Shift,
                CourseId = c.CourseId,
                CourseName = c.Course?.Name ?? "—",
                StudentCount = c.Students?.Count ?? 0,
                IsClosed = c.IsClosed
            }).ToList();
        }

        public async Task<StudentClassViewModel?> GetByIdAsync(int id)
        {
            var entity = await _studentClassRepository.GetByIdWithDetailsAsync(id);
            if (entity == null) return null;

            return new StudentClassViewModel
            {
                Id = entity.Id,
                Name = entity.Name,
                AcademicYear = entity.AcademicYear,
                Shift = entity.Shift,
                CourseId = entity.CourseId,
                Students = entity.Students?.Select(s => s.User.FullName).ToList() ?? new List<string>()
            };
        }

       
        public async Task<IEnumerable<SelectListItem>> GetCoursesSelectListAsync(int? selectedCourseId = null)
        {
            var courses = await _courseRepository.GetAll().OrderBy(c => c.Name).ToListAsync();
            return courses.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name,
                Selected = selectedCourseId.HasValue && selectedCourseId.Value == c.Id
            }).ToList();
        }

        
        public async Task<List<ApplicationUser>> GetAllStudentsAsync()
        {
            return await _studentClassRepository.GetAllStudentEntitiesAsync();
        }


        public async Task<StudentProfile?> GetStudentProfileByUserIdAsync(string userId)
        {
            return await _studentClassRepository.GetStudentProfileByUserIdAsync(userId);
        }

       
        public async Task AssignStudentToClassAsync(string studentId, int classId)
        {
            var profile = await _studentProfileRepository.GetByUserIdAsync(studentId);
            if (profile != null)
            {
                profile.StudentClassId = classId;
                await _studentProfileRepository.UpdateAsync(profile);
            }
        }

        
        public async Task RemoveStudentFromClassAsync(string studentId)
        {
            var profile = await _studentProfileRepository.GetByUserIdAsync(studentId);
            if (profile != null)
            {
                profile.StudentClassId = null;
                await _studentProfileRepository.UpdateAsync(profile);
            }
        }
    }
}
