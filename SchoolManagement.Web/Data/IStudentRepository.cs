using SchoolManagement.Web.Data.Entities;

namespace SchoolManagement.Web.Data
{
    public interface IStudentRepository
    {
        Task<IEnumerable<Student>> GetAllAsync();
        Task<Student> GetByIdAsync(int id);
  
    }
}
