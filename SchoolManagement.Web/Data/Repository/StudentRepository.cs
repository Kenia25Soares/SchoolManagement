using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data.Entities;

namespace SchoolManagement.Web.Data.Repository
{
    public class StudentRepository : IStudentRepository
    {
        private readonly DataContext _context;

        public StudentRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<StudentUser>> GetAllAsync()
        {
            return await _context.Users
                .OfType<StudentUser>()
                .Include(s => s.Course)
                .ToListAsync();
        }

        public async Task<StudentUser?> GetByIdAsync(string userId)
        {
            return await _context.Users
                .OfType<StudentUser>()
                .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.Id == userId);
        }

        public async Task AddAsync(StudentUser student)
        {
            _context.Users.Add(student);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(StudentUser student)
        {
            _context.Users.Update(student);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string userId)
        {
            var student = await GetByIdAsync(userId);
            if (student != null)
            {
                _context.Users.Remove(student);
                await _context.SaveChangesAsync();
            }
        }
    }
}
