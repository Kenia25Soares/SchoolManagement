using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Models
{
    public class CourseManagementViewModel
    {
        public int CourseId { get; set; }

        [Display(Name = "Course Name")]
        public string CourseName { get; set; } = string.Empty;

        public List<StudentAssignmentViewModel> AvailableStudents { get; set; } = new();
        public List<StudentAssignmentViewModel> AssignedStudents { get; set; } = new();
        public string? AssignedStudentsHidden { get; set; }

        public List<SubjectAssignmentViewModel> AvailableSubjects { get; set; } = new();
        public List<SubjectAssignmentViewModel> AssignedSubjects { get; set; } = new();
        public string? AssignedSubjectsHidden { get; set; }
    }
}
