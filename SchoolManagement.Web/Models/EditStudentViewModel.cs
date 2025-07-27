using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Models
{
    public class EditStudentViewModel
    {
        public string Id { get; set; } = null!;

        [Required(ErrorMessage = "Full name is required.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Date of Birth is required.")]
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        public string? Address { get; set; }

        [Required(ErrorMessage = "Class is required.")]
        [Display(Name = "Class")]
        public int? StudentClassId { get; set; }

        [Display(Name = "Current Profile Picture")]
        [DataType(DataType.Upload)]
        public IFormFile? OfficialPhoto { get; set; }

        [Display(Name = "New Profile Picture")]
        public IFormFile? ProfilePicture { get; set; }  // Uso para pdf, imagens, e outros ficheiros que o utilizador possa querer enviar
    }
}