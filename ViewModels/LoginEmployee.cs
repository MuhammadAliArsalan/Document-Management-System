using System.ComponentModel.DataAnnotations;

namespace DocumentSearch.ViewModels
{
    public class LoginEmployee
    {
        [Required]
        public string EmployeeCode { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginEmployeeResponse
    {
        public string Token { get; set; } = string.Empty;
    }
}
