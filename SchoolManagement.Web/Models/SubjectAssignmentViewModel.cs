using SchoolManagement.Web.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Models
{
    public class SubjectAssignmentViewModel
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
        public bool IsAssigned { get; set; }
    }
}
