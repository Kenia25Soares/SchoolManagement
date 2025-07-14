using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Repository;

namespace SchoolManagement.Data.Repositories
{
    public class CourseRepository : GenericRepository<Course>, ICourseRepository
    {
        private readonly DataContext _context;

        public CourseRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Course> GetByIdWithAllRelationsAsync(int id)
        {
            return await _context.Courses
                .Include(c => c.CourseSubjects)
                .Include(c => c.StudentClasses)
                .Include(c => c.StudentGrades)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task RemoveCourseSubjectsAsync(IEnumerable<CourseSubject> courseSubjects)
        {
            _context.CourseSubjects.RemoveRange(courseSubjects);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveStudentClassesAsync(IEnumerable<StudentClass> studentClasses)
        {
            _context.StudentClasses.RemoveRange(studentClasses);
            await _context.SaveChangesAsync();
        }
    }
}
