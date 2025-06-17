using SchoolManagement.Web.Models;

namespace SchoolManagement.Web.Helpers
{
    public interface ICourseHelper
    {
        Task<CourseManagementViewModel> GetCourseManagementAsync(int courseId);
        Task UpdateCourseAssignmentsAsync(CourseManagementViewModel model);
    }
}
