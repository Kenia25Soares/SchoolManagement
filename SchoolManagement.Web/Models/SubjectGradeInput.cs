using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Models
{
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
