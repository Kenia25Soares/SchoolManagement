using SchoolManagement.Web.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Models
{
    public class StudentAssignmentViewModel
    {
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public bool IsAssigned { get; set; }
    }
}
