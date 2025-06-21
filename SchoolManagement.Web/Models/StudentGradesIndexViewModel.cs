using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Models
{
    public class StudentGradesIndexViewModel
    {
        public List<SelectListItem> Classes { get; set; } = new();
        public List<UserListViewModel> Students { get; set; } = new();
    }

    public class AddGradesViewModel
    {
        public string StudentId { get; set; } = null!;

        public List<GradeInputModel> Grades { get; set; } = new();

        public IEnumerable<SelectListItem> Subjects { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> GradeTypes { get; set; } = new List<SelectListItem>();
    }

    public class GradeInputModel
    {
        public int SubjectId { get; set; }
        public int GradeTypeId { get; set; }
        public double? Grade { get; set; }
        public int Absences { get; set; }
    }
}
