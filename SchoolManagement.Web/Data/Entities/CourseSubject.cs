using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagement.Web.Data.Entities
{
    public class CourseSubject
    {
        [Required]
        public int CourseId { get; set; }
        public Course Course { get; set; }

        [Required]
        public int SubjectId { get; set; }
        public Subject Subject { get; set; }
    }
}

