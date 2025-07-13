using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Repository;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class StudentClassRepository : GenericRepository<StudentClass>, IStudentClassRepository
{

    public StudentClassRepository(DataContext context) : base(context)
    {
    }

    public async Task<List<StudentClass>> GetAllOrderedByNameAsync()
    {
        return await _context.StudentClasses
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<StudentClass> GetByIdWithDetailsAsync(int id)
    {
        return await _context.StudentClasses
            .Include(c => c.Students)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<StudentUser>> GetAllStudentEntitiesAsync()
    {
        return await _context.Users
            .OfType<StudentUser>()
            .ToListAsync();
    }

    public async Task<StudentUser> GetStudentByIdAsync(string studentId)
    {
        return await _context.Users
            .OfType<StudentUser>()
            .FirstOrDefaultAsync(s => s.Id == studentId);
    }

    public async Task<StudentProfile?> GetStudentProfileByUserIdAsync(string userId)
    {
        return await _context.StudentProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
    }
}
