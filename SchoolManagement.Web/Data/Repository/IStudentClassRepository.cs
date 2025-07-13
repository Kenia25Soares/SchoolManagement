using SchoolManagement.Web.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SchoolManagement.Web.Data.Repository
{
    public interface IStudentClassRepository : IGenericRepository<StudentClass>
    {
        Task<StudentClass> GetByIdWithDetailsAsync(int id);
        Task<List<StudentUser>> GetAllStudentEntitiesAsync();
        Task<StudentUser> GetStudentByIdAsync(string studentId);
        Task<List<StudentClass>> GetAllOrderedByNameAsync();
        Task<StudentProfile?> GetStudentProfileByUserIdAsync(string userId);
        Task<bool> SaveAllAsync();
    }
}
