using System;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Data.Entities
{
    public class CourseSubject
    {
        public int CourseId { get; set; }
        public Course Course { get; set; }

        public int SubjectId { get; set; }
        public Subject Subject { get; set; }
    }
}
