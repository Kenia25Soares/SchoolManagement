using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Repositories;

namespace SchoolManagement.Data.Repositories
{
    public class CourseRepository : GenericRepository<Course>, ICourseRepository
    {
        private readonly DataContext _context;

        public CourseRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Course?> GetByIdWithAllRelationsAsync(int id)
        {
            return await _context.Courses
                .Include(c => c.CourseSubjects)
                    .ThenInclude(cs => cs.Subject)
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

        public new async Task DeleteAsync(Course course)
        {
            // Bloqueia se existirem turmas associadas
            if (course.StudentClasses != null && course.StudentClasses.Any())
                throw new InvalidOperationException("Cannot delete a course that has student classes assigned.");

            // Apagar as relações CourseSubjects 
            if (course.CourseSubjects != null && course.CourseSubjects.Any())
            {
                _context.CourseSubjects.RemoveRange(course.CourseSubjects);
                await _context.SaveChangesAsync();
            }

            // Bloqueia se houver notas diretamente ligadas ao curso
            var hasGrades = await _context.StudentGrades.AnyAsync(g => g.CourseId == course.Id);
            if (hasGrades)
                throw new InvalidOperationException("Cannot delete a course that has student grades.");

            // Bloqueia se houver notas ligadas às disciplinas do curso
            var subjectIds = course.CourseSubjects.Select(cs => cs.SubjectId).ToList();
            var hasSubjectGrades = await _context.StudentGrades.AnyAsync(g => subjectIds.Contains(g.SubjectId));
            if (hasSubjectGrades)
                throw new InvalidOperationException("Cannot delete this course because some of its subjects have student grades assigned.");

            // Remover o curso
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
          
        }
    }
}
