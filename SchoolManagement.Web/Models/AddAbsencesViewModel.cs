using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolManagement.Web.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Models
{
    public class AddAbsencesViewModel
    {
        public string StudentId { get; set; } = null!;

        public List<AbsenceInputModel> Absences { get; set; } = new();

        public IEnumerable<SelectListItem> Subjects { get; set; } = new List<SelectListItem>();
    }

    public class AbsenceInputModel
    {
        public int SubjectId { get; set; }
        public int Absences { get; set; }
    }
}
