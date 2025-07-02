using Microsoft.AspNetCore.Mvc.Rendering;

namespace SchoolManagement.Web.Models
{
    public class AddGradesViewModel
    {
        public string StudentId { get; set; } = null!;

        public List<GradeInputModel> Grades { get; set; } = new();

        public IEnumerable<SelectListItem> Subjects { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> GradeTypes { get; set; } = new List<SelectListItem>();
    }
}
