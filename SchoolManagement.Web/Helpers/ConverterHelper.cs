using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Models;

namespace SchoolManagement.Web.Helpers
{
    public class ConverterHelper : IConverterHelper
    {
        /// <summary>
        /// Converte uma entidade Course em CourseViewModel.
        /// </summary>
        public CourseViewModel ToCourseViewModel(Course course)
        {
            return new CourseViewModel
            {
                Id = course.Id,
                Name = course.Name,
                AcademicYear = course.AcademicYear,
                Shift = course.Shift
            };
        }

        /// <summary>
        /// Converte um CourseViewModel em entidade Course.
        /// </summary>
        public Course ToCourseEntity(CourseViewModel model, bool isNew)
        {
            return new Course
            {
                Id = isNew ? 0 : model.Id,
                Name = model.Name,
                AcademicYear = model.AcademicYear,
                Shift = model.Shift
            };
        }

        /// <summary>
        /// Converte uma entidade Subject em SubjectViewModel.
        /// </summary>
        public SubjectViewModel ToSubjectViewModel(Subject subject)
        {
            return new SubjectViewModel
            {
                Id = subject.Id,
                Name = subject.Name,
                Workload = subject.Workload
            };
        }

        /// <summary>
        /// Converte um SubjectViewModel em entidade Subject.
        /// </summary>
        public Subject ToSubjectEntity(SubjectViewModel model, bool isNew)
        {
            return new Subject
            {
                Id = isNew ? 0 : model.Id,
                Name = model.Name,
                Workload = model.Workload
            };
        }
    }
}
