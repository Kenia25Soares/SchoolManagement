using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolManagement.Web.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Web.Models
{
    public class AdminDashboardCombinedViewModel
    {
        public AdminDashboardViewModel Stats { get; set; }
        public List<AlertViewModel> Alerts { get; set; }
    }
}
