using System.ComponentModel.DataAnnotations;
using SchoolManagement.Web.Data.Enums;

namespace SchoolManagement.Web.Data.Entities
{
    public class GradeType
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public double Weight { get; set; }
    }
}
