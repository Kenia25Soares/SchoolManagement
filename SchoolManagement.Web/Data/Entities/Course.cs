namespace SchoolManagement.Web.Data.Entities
{
    public class Course
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Discipline> Disciplines { get; set; }
    }
}
