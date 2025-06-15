using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Models;

namespace SchoolManagement.Web.Helpers
{
    public interface IConverterHelper
    {
        CourseViewModel ToCourseViewModel(Course course);
        Course ToCourseEntity(CourseViewModel model, bool isNew);

        SubjectViewModel ToSubjectViewModel(Subject subject);
        Subject ToSubjectEntity(SubjectViewModel model, bool isNew);
    }
}
