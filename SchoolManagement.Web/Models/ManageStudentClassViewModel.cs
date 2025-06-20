using SchoolManagement.Web.Data.Enums;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace SchoolManagement.Web.Models
{
    public class ManageStudentClassViewModel
    {
        public int StudentClassId { get; set; }
        public string StudentClassName { get; set; }

        public List<StudentAssignmentViewModel> AvailableStudents { get; set; } = new List<StudentAssignmentViewModel>();
        public List<StudentAssignmentViewModel> AssignedStudents { get; set; } = new List<StudentAssignmentViewModel>();
    }

}
