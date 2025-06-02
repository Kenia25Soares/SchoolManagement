namespace SchoolManagement.Web.Data.Entities
{
    public class Note
    {
        public int Id { get; set; }
        public double Value { get; set; }
        public int DisciplineId { get; set; }
        public Discipline Discipline { get; set; }
        public int StudentId { get; set; }
        public Student Student { get; set; }
    }
}
