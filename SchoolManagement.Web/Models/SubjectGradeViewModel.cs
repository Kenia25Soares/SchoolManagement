namespace SchoolManagement.Web.Models
{
    public class SubjectGradeViewModel
    {
        public string SubjectName { get; set; } = string.Empty;

        public double WeightedAverage { get; set; }

        public int TotalAbsences { get; set; }

        public int AllowedAbsences { get; set; }
    }
}
