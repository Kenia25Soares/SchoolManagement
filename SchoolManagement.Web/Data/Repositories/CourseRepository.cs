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
            //// Bloquear se houver notas diretamente ligadas ao curso
            //var hasGrades = await _context.StudentGrades.AnyAsync(g => g.CourseId == course.Id);
            //if (hasGrades)
            //{
            //    throw new InvalidOperationException("Cannot delete a course that has student grades.");
            //}

            //// Bloqueia se houver notas ligadas às disciplinas do curso
            //var subjectIds = course.CourseSubjects.Select(cs => cs.SubjectId).ToList();
            //var hasSubjectGrades = await _context.StudentGrades.AnyAsync(g => subjectIds.Contains(g.SubjectId));
            //if (hasSubjectGrades)
            //{
            //    throw new InvalidOperationException("Cannot delete this course because some of its subjects have student grades assigned.");
            //}

            ////var hasClasses = await _context.StudentClasses.AnyAsync(c => c.CourseId == course.Id);
            ////if (hasClasses)
            ////{
            ////    throw new InvalidOperationException("Cannot delete a course that has student classes.");
            ////}
            ///

            // Apagar manualmente as relações CourseSubjects para evitar conflito de FK
            if (course.CourseSubjects != null && course.CourseSubjects.Any())
            {
                _context.CourseSubjects.RemoveRange(course.CourseSubjects);
                await _context.SaveChangesAsync();
            }

            // Verifica notas diretamente ligadas ao curso
            var hasGrades = await _context.StudentGrades.AnyAsync(g => g.CourseId == course.Id);
            if (hasGrades)
                throw new InvalidOperationException("Cannot delete a course that has student grades.");

            // Verifica notas ligadas às disciplinas do curso
            var subjectIds = course.CourseSubjects.Select(cs => cs.SubjectId).ToList();
            var hasSubjectGrades = await _context.StudentGrades.AnyAsync(g => subjectIds.Contains(g.SubjectId));

            if (hasSubjectGrades)
                throw new InvalidOperationException("Cannot delete this course because some of its subjects have student grades assigned.");

            //// Verifica se há turmas associadas (não permite exclusão)
            //var hasClasses = await _context.StudentClasses.AnyAsync(c => c.CourseId == course.Id);
            //if (hasClasses)
            //    throw new InvalidOperationException("Cannot delete a course that has student classes.");

            //// Remove associações manuais (CourseSubjects e StudentClasses) se permitido
            //if (course.CourseSubjects.Any())
            //    _context.CourseSubjects.RemoveRange(course.CourseSubjects);

            //if (course.StudentClasses.Any())
            //    _context.StudentClasses.RemoveRange(course.StudentClasses);

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
        }
    }
}
