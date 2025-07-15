using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Models
{
    public class GradeInputModel
    {
        [Required(ErrorMessage = "Subject is required.")]
        public int SubjectId { get; set; }

        [Required(ErrorMessage = "Grade type is required.")]
        public int GradeTypeId { get; set; }

        [Range(0, 20, ErrorMessage = "Grade must be between 0 and 20.")]
        public double? Grade { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Absences cannot be negative.")]
        public int Absences { get; set; }
    }
}
