using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Models;

namespace SchoolManagement.Web.Helpers
{
    public class ConverterHelper : IConverterHelper
    {
        public CourseViewModel ToCourseViewModel(Course course)
        {
            return new CourseViewModel
            {
                Id = course.Id,
                Name = course.Name,
                SubjectsCount = course.CourseSubjects?.Count ?? 0,
                Subjects = course.CourseSubjects?.Where(cs => cs.Subject != null).Select(cs => cs.Subject.Name).ToList() ?? new List<string>()
            };
        }

        public Course ToCourseEntity(CourseViewModel model, bool isNew)
        {
            return new Course
            {
                Id = isNew ? 0 : model.Id,
                Name = model.Name
            };
        }
        public SubjectViewModel ToSubjectViewModel(Subject subject)
        {
            return new SubjectViewModel
            {
                Id = subject.Id,
                Name = subject.Name,
                Workload = subject.Workload,
                AllowedAbsences = subject.AllowedAbsences
            };
        }

        public Subject ToSubjectEntity(SubjectViewModel model, bool isNew)
        {
            return new Subject
            {
                Id = isNew ? 0 : model.Id,
                Name = model.Name,
                Workload = model.Workload,
                AllowedAbsences = model.AllowedAbsences
            };
        }

        public StudentClassViewModel ToStudentClassViewModel(StudentClass studentClass)
        {
            return new StudentClassViewModel
            {
                Id = studentClass.Id,
                Name = studentClass.Name,
                AcademicYear = studentClass.AcademicYear,
                Shift = studentClass.Shift,
                CourseId = studentClass.CourseId
            };
        }

        public StudentClass ToStudentClassEntity(StudentClassViewModel model, bool isNew)
        {
            return new StudentClass
            {
                Id = isNew ? 0 : model.Id,
                Name = model.Name,
                AcademicYear = model.AcademicYear,
                Shift = model.Shift,
                CourseId = model.CourseId
            };
        }
    }
}
