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
}
