using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data;
using SchoolManagement.Web.Data.Repository;

public class StudentProfileRepository : GenericRepository<StudentProfile>, IStudentProfileRepository
{
    public StudentProfileRepository(DataContext context) : base(context) { }

    public async Task<StudentProfile?> GetByUserIdAsync(string userId)
    {
        return await _context.StudentProfiles
            .Include(p => p.StudentClass)
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task<StudentProfile?> GetByIdWithClassAsync(int id)
    {
        return await _context.StudentProfiles
            .Include(p => p.StudentClass)
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}
