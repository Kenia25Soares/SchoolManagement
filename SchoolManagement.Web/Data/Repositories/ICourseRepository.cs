using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Repositories;

namespace SchoolManagement.Data.Repositories
{
    public interface ICourseRepository : IGenericRepository<Course>
    {
        Task<Course?> GetByIdWithAllRelationsAsync(int id);

        new Task DeleteAsync(Course course);

        Task RemoveCourseSubjectsAsync(IEnumerable<CourseSubject> courseSubjects);


        Task RemoveStudentClassesAsync(IEnumerable<StudentClass> studentClasses);
    }
}
