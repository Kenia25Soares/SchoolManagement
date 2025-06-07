namespace SchoolManagement.Web.Data.Entities
{
    public class Grid
    {
        public int Id { get; set; }
        public double Value { get; set; }
        public int SubjectId { get; set; }
        public Subject Subject { get; set; }
        public int StudentId { get; set; }
        public Student Student { get; set; }
    }
}
