using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Repository;

namespace SchoolManagement.Data.Repositories
{
    public interface ICourseRepository : IGenericRepository<Course>
    {
        Task<Course> GetByIdWithAllRelationsAsync(int id);
        Task RemoveCourseSubjectsAsync(IEnumerable<CourseSubject> courseSubjects);
        Task RemoveStudentClassesAsync(IEnumerable<StudentClass> studentClasses);
    }
}
