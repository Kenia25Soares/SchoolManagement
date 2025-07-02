using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Models
{
    public class StudentGradesIndexViewModel
    {
        public List<SelectListItem> Classes { get; set; } = new();
        public List<UserListViewModel> Students { get; set; } = new();
    }
}
