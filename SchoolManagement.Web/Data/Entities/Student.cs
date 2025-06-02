using System.Diagnostics;

namespace SchoolManagement.Web.Data.Entities
{
    public class Student
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        public string PhotoStudentPath { get; set; }

        public ICollection<Note> Notes { get; set; }


    }
}
