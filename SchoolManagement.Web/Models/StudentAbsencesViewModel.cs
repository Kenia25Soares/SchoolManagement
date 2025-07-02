using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolManagement.Web.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Models
{
    public class StudentAbsencesViewModel
    {
        public string StudentId { get; set; } = null!;
        public string StudentName { get; set; } = null!;
        public List<AbsenceSummaryViewModel> Absences { get; set; } = new();
    }
}
