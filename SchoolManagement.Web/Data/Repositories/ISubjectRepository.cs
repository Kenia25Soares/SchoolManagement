using SchoolManagement.Web.Data.Entities;

namespace SchoolManagement.Web.Data.Repositories
{
    public interface ISubjectRepository : IGenericRepository<Subject>
    {
        new Task DeleteAsync(Subject subject);

        Task<bool> IsSubjectInUseAsync(int subjectId);

    }
}

