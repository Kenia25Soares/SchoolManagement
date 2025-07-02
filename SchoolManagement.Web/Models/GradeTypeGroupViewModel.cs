using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolManagement.Web.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Models
{
    public class GradeTypeGroupViewModel
    {
        public string GradeTypeName { get; set; } = null!;
        public List<double> Grades { get; set; } = new();
        public double Weight { get; set; } = 0;
    }
}
