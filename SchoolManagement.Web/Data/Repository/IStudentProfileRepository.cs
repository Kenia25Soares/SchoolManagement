using SchoolManagement.Web.Data.Repository;

public interface IStudentProfileRepository : IGenericRepository<StudentProfile>
{
    Task<StudentProfile?> GetByUserIdAsync(string userId);
    Task<StudentProfile?> GetByIdWithClassAsync(int id);
}
