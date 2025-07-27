using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data.Entities;

namespace SchoolManagement.Web.Data.Repositories
{
    public class StudentClassRepository : GenericRepository<StudentClass>, IStudentClassRepository
    {
        private readonly DataContext _context;
        public StudentClassRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<StudentClass>> GetAllOrderedByNameAsync()
        {
            return await _context.StudentClasses
                .Where(c => !c.IsClosed)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<StudentClass?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.StudentClasses
            .Include(sc => sc.Students)
            .ThenInclude(sp => sp.User)
            .FirstOrDefaultAsync(sc => sc.Id == id);
        }

        public async Task<List<ApplicationUser>> GetAllStudentEntitiesAsync()
        {
            return await _context.Users
                .OfType<ApplicationUser>()
                .ToListAsync();
        }

        public async Task<ApplicationUser?> GetStudentByIdAsync(string studentId)
        {
            return await _context.Users
                .OfType<ApplicationUser>()
                .FirstOrDefaultAsync(s => s.Id == studentId);
        }

        public async Task<StudentProfile?> GetStudentProfileByUserIdAsync(string userId)
        {
            return await _context.StudentProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task UpdateStudentProfileAsync(StudentProfile profile)
        {
            _context.StudentProfiles.Update(profile);
            await _context.SaveChangesAsync();
        }

        public async Task<List<SelectListItem>> GetClassesSelectListAsync(int? selectedClassId)
        {
            return await _context.StudentClasses
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name,
                    Selected = selectedClassId.HasValue && selectedClassId.Value == c.Id
                })
                .ToListAsync();
        }

        public async Task<object?> GetClassWithStudentsAsync(int classId)
        {
            return await _context.StudentClasses
                .Include(sc => sc.Students)
                    .ThenInclude(sp => sp.User)
                .Where(sc => sc.Id == classId)
                .Select(sc => new
                {
                    ClassId = sc.Id,
                    ClassName = sc.Name,
                    AcademicYear = sc.AcademicYear,
                    Students = sc.Students.Select(s => new
                    {
                        s.User.Id,
                        s.User.FullName,
                        s.User.Email,
                        s.User.PhoneNumber,
                        s.DateOfBirth
                    })
                })
                .FirstOrDefaultAsync();
        }

    }
}