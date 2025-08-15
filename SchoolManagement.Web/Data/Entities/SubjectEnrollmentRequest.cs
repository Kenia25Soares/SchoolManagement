using System.ComponentModel.DataAnnotations;
using SchoolManagement.Web.Data.Enums;

namespace SchoolManagement.Web.Data.Entities
{
    public class SubjectEnrollmentRequest : IEntity
    {
        public int Id { get; set; }

        [Required]
        public string StudentId { get; set; } = null!;
        public ApplicationUser Student { get; set; } = null!;

        [Required]
        public int SubjectId { get; set; }
        public Subject Subject { get; set; } = null!;

        [Required]
        [StringLength(500, MinimumLength = 10)]
        public string Description { get; set; } = null!;

        public EnrollmentRequestStatus Status { get; set; } = EnrollmentRequestStatus.Pending;

        public DateTime RequestDate { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string? ResponseMessage { get; set; }

        public string? ProcessedById { get; set; }
        public ApplicationUser? ProcessedBy { get; set; }

        public DateTime? ProcessedDate { get; set; }
    }
}
