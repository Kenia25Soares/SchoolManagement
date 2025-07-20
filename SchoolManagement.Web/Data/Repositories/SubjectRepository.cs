using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data.Entities;

namespace SchoolManagement.Web.Data.Repositories
{
    public class SubjectRepository : GenericRepository<Subject>, ISubjectRepository
    {
        private readonly DataContext _context;

        public SubjectRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> IsSubjectInUseAsync(int subjectId)
        {
            return await _context.CourseSubjects.AnyAsync(cs => cs.SubjectId == subjectId);
        }

        //public async Task<bool> HasGradesAsync(int subjectId)
        //{
        //    return await _context.StudentGrades.AnyAsync(g => g.SubjectId == subjectId);
        //}

        public new async Task DeleteAsync(Subject subject)  // crio uma nova implementação do DeleteAsync para verificar se a disciplina tem notas associadas antes de excluir
        {
            // Se a disciplina ainda está em uso, impedir exclusão
            bool isInUse = await IsSubjectInUseAsync(subject.Id);
            if (isInUse)
            {
                throw new InvalidOperationException(
                    "Cannot delete this subject because it is associated with courses.");
            }

            //if (subject == null)
            //    throw new ArgumentNullException(nameof(subject));

            //// Verifica se o subject tem notas
            //var hasGrades = await _context.StudentGrades.AnyAsync(g => g.SubjectId == subject.Id);
            //if (hasGrades)
            //{
            //    throw new InvalidOperationException("Cannot delete this subject because it has grades assigned.");
            //}

            //// Remove CourseSubjects associados antes de excluir a disciplina
            //var courseSubjects = await _context.CourseSubjects
            //    .Where(cs => cs.SubjectId == subject.Id)
            //    .ToListAsync();

            //if (courseSubjects.Any())
            //    _context.CourseSubjects.RemoveRange(courseSubjects);


            _context.Subjects.Remove(subject);
            await _context.SaveChangesAsync();
        }

    }
}
