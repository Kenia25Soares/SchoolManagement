using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SchoolManagement.Web.Helpers
{
    public interface IStudentClassHelper
    {
        Task<List<StudentClassViewModel>> GetAllAsync();
        Task<StudentClassViewModel?> GetByIdAsync(int id);
        Task<IEnumerable<SelectListItem>> GetCoursesSelectListAsync(int? selectedCourseId = null);
        Task<List<ApplicationUser>> GetAllStudentsAsync();
        Task<StudentProfile?> GetStudentProfileByUserIdAsync(string userId);
        Task AssignStudentToClassAsync(string studentId, int classId);
        Task RemoveStudentFromClassAsync(string studentId);
    }
}
