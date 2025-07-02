namespace SchoolManagement.Web.Models
{
    public class GradeInputModel
    {
        public int SubjectId { get; set; }
        public int GradeTypeId { get; set; }
        public double? Grade { get; set; }
        public int Absences { get; set; }
    }
}
