using SchoolManagement.Web.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Models
{
    public class StudentDashboardViewModel
    {
        public string StudentName { get; set; }
        public string StudentId { get; set; }
    }

    public class AbsenceViewModel
    {
        public string SubjectName { get; set; }
        public int TotalAbsences { get; set; }
    }

}
