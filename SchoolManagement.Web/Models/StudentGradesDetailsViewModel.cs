using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolManagement.Web.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Models
{
    public class StudentGradesDetailsViewModel
    {
        public string StudentId { get; set; } = null!;
        public string StudentName { get; set; } = null!;
        public List<SubjectGradesViewModel> SubjectGrades { get; set; } = new();
        public double TotalAverage { get; internal set; }
    }
}
