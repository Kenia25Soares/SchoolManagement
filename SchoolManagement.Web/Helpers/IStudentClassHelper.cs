using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolManagement.Web.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SchoolManagement.Web.Helpers
{
    public interface IStudentClassHelper
    {
        Task<IEnumerable<StudentClassViewModel>> GetAllAsync();

        Task<StudentClassViewModel> GetByIdAsync(int id);

        Task<IEnumerable<SelectListItem>> GetCoursesSelectListAsync(int? selectedCourseId = null);

        Task<IEnumerable<StudentUserViewModel>> GetAllStudentsAsync();
    }
}
