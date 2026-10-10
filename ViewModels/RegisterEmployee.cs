using DocumentSearch.Models;
using System.ComponentModel.DataAnnotations;

namespace DocumentSearch.ViewModels
{
    public class RegisterEmployee
    {
        [Required, MaxLength(50)] public string EmployeeCode { get; set; } = string.Empty;
        [Required, MaxLength(100)] public string FirstName { get; set; } = string.Empty;
        [Required, MaxLength(100)] public string LastName { get; set; } = string.Empty;
        [Required, MinLength(8)] public string Password { get; set; } = string.Empty;

    }

    public class RegisterEmployeeResponse
    {
        public int Id { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
    }
}
