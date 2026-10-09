using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;

namespace DocumentSearch.Models;

[Table("T_Employees")]
public class Employee
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string EmployeeCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    // Navigation property
    public ICollection<Document> Documents { get; set; }
        = new List<Document>();
}