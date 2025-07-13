using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data.Entities;

namespace SchoolManagement.Web.Data.Repository
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
    }
}
