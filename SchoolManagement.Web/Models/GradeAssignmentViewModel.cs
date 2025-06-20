using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Models
{
    public class GradeAssignmentViewModel
    {
        public string StudentId { get; set; }
        public string StudentName { get; set; }

        public int CourseId { get; set; }
        public string CourseName { get; set; }

        public List<SubjectGradeInput> Subjects { get; set; } = new();
    }

    public class SubjectGradeInput
    {
        public int SubjectId { get; set; }

        [BindNever]
        [Display(Name = "Subject")]
        public string SubjectName { get; set; } = null!;

        [Range(0, 20, ErrorMessage = "Grade must be between 0 and 20.")]
        public double? Grade { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Absences must be a positive number.")]
        public int Absences { get; set; }
    }
}
