using SchoolManagement.Web.Data.Entities;

namespace SchoolManagement.Web.Data.Repository
{
    public interface IStudentRepository
    {
        Task<IEnumerable<StudentUser>> GetAllAsync();
        Task<StudentUser?> GetByIdAsync(string userId);
        Task AddAsync(StudentUser student);
        Task UpdateAsync(StudentUser student);
        Task DeleteAsync(string userId);
    }
}
