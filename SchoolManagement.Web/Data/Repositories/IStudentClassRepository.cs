using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolManagement.Web.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SchoolManagement.Web.Data.Repositories
{
    public interface IStudentClassRepository : IGenericRepository<StudentClass>
    {
        Task<StudentClass?> GetByIdWithDetailsAsync(int id);
        Task<List<ApplicationUser>> GetAllStudentEntitiesAsync();
        Task<ApplicationUser?> GetStudentByIdAsync(string studentId);
        Task<List<StudentClass>> GetAllOrderedByNameAsync();
        Task<StudentProfile?> GetStudentProfileByUserIdAsync(string userId);

        Task UpdateStudentProfileAsync(StudentProfile profile);
        //Task<bool> SaveAllAsync();

        Task<List<SelectListItem>> GetClassesSelectListAsync(int? selectedClassId);
    }
}
