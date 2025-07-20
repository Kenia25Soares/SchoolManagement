using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Repositories;

namespace SchoolManagement.Web.Data.Repositories
{
    public class StudentProfileRepository : GenericRepository<StudentProfile>, IStudentProfileRepository
    {
        private readonly DataContext _context;

        public StudentProfileRepository(DataContext context) : base(context)
        {
            _context = context;
        }

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
}
