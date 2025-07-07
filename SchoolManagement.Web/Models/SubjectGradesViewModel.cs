using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolManagement.Web.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Models
{
    public class SubjectGradesViewModel
    {
        public string SubjectName { get; set; } = null!;
        public List<GradeTypeGroupViewModel> GradesByType { get; set; } = new();
        public double WeightedAverage { get; set; }

        public int AllowedAbsences { get; set; }
        public int TotalAbsences { get; set; }
        public bool FailedDueToAbsences { get; set; }
    }
}
