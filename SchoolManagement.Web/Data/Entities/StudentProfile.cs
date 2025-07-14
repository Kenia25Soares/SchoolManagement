using SchoolManagement.Web.Data.Entities;
using System.ComponentModel.DataAnnotations;

public class StudentProfile : IEntity
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;

    [Required]
    public DateTime DateOfBirth { get; set; }

    public string? Address { get; set; }

    public bool IsExcludedDueToAbsences { get; set; }

    public string? OfficialPhotoUrl { get; set; }

    public int? StudentClassId { get; set; }
    public StudentClass? StudentClass { get; set; }
}
