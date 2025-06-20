using SchoolManagement.Web.Models;

namespace SchoolManagement.Web.Helpers
{
    public interface ICourseHelper
    {
        Task<CourseManagementViewModel> GetCourseManagementAsync(int courseId);
        Task AssignSubjectToCourseAsync(int courseId, int subjectId);
        Task RemoveSubjectFromCourseAsync(int courseId, int subjectId);
    }
}
