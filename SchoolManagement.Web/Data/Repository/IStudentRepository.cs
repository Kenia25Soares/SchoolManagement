using SchoolManagement.Web.Data.Entities;

namespace SchoolManagement.Web.Data.Repository
{
    public interface IStudentRepository
    {
        Task<IEnumerable<Student>> GetAllAsync();
        Task<Student> GetByIdAsync(int id);
        Task AddAsync(Student student);

        Task<Student?> GetByUserIdAsync(string userId);
        Task UpdateAsync(Student student);

    }
}
