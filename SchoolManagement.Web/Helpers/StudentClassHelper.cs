using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data;
using SchoolManagement.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolManagement.Web.Helpers
{
    public class StudentClassHelper : IStudentClassHelper
    {
        private readonly DataContext _context;

        public StudentClassHelper(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<StudentClassViewModel>> GetAllAsync()
        {
            var list = await _context.StudentClasses
                .Include(sc => sc.Course)
                .ToListAsync();

            return list.Select(sc => new StudentClassViewModel
            {
                Id = sc.Id,
                Name = sc.Name,
                AcademicYear = sc.AcademicYear,
                Shift = sc.Shift,
                CourseId = sc.CourseId,
                CourseName = sc.Course?.Name
            });
        }

        public async Task<StudentClassViewModel> GetByIdAsync(int id)
        {
            var sc = await _context.StudentClasses
                .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sc == null) return null;

            return new StudentClassViewModel
            {
                Id = sc.Id,
                Name = sc.Name,
                AcademicYear = sc.AcademicYear,
                Shift = sc.Shift,
                CourseId = sc.CourseId,
                CourseName = sc.Course?.Name
            };
        }

        public async Task<IEnumerable<SelectListItem>> GetCoursesSelectListAsync(int? selectedCourseId = null)
        {
            var courses = await _context.Courses.OrderBy(c => c.Name).ToListAsync();
            return courses.Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString(),
                Selected = selectedCourseId.HasValue && selectedCourseId.Value == c.Id
            });
        }

        public async Task<IEnumerable<StudentUserViewModel>> GetAllStudentsAsync()
        {
            return await _context.StudentProfiles
                .Include(sp => sp.User)
                .Select(sp => new StudentUserViewModel
                {
                    Id = sp.User.Id,
                    FullName = sp.User.FullName,
                    StudentClassId = sp.StudentClassId
                })
                .ToListAsync();
        }
    }
}
