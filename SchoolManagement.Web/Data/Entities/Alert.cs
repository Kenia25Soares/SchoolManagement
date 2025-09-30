using System;
using System.ComponentModel.DataAnnotations;
using SchoolManagement.Web.Data.Enums;

namespace SchoolManagement.Web.Data.Entities
{
    public class Alert : IEntity
    {
        public int Id { get; set; }

        [Required]
        public string StudentId { get; set; } = null!;
        public ApplicationUser Student { get; set; } = null!;

        [Required]
        public AlertType Type { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = null!;

        [Required]
        [StringLength(500)]
        public string Message { get; set; } = null!;

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReadAt { get; set; }

        public int? SubjectId { get; set; }
        public Subject? Subject { get; set; }

        public int? StudentClassId { get; set; }
        public StudentClass? StudentClass { get; set; }

        public int? StudentGradeId { get; set; }
        public StudentGrade? StudentGrade { get; set; }

        [StringLength(1000)]
        public string? Metadata { get; set; }

        // Who created the alert Employee
        public string? CreatedById { get; set; }
        public ApplicationUser? CreatedBy { get; set; }
    }
}