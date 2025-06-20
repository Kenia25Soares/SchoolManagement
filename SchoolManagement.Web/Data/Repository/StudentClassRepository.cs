using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data;
using SchoolManagement.Web.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SchoolManagement.Web.Data.Repository
{
    public class StudentClassRepository : GenericRepository<StudentClass>, IStudentClassRepository
    {
        private readonly DataContext _context;

        public StudentClassRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<StudentClass> GetByIdWithDetailsAsync(int id)
        {
            return await _context.StudentClasses
                .Include(sc => sc.Course)
                .Include(sc => sc.Students)
                .FirstOrDefaultAsync(sc => sc.Id == id);
        }

        public async Task<List<StudentUser>> GetAllStudentEntitiesAsync()
        {
            return await _context.Users
                .OfType<StudentUser>()
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task<StudentUser> GetStudentByIdAsync(string studentId)
        {
            return await _context.Users.OfType<StudentUser>().FirstOrDefaultAsync(s => s.Id == studentId);
        }
    }
}
