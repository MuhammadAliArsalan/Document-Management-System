using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentSearch.Models;

[Table("T_Documents")]
public class Document
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UploadedByEmployeeId { get; set; }

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string ContentType { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public string? ExtractedText { get; set; }

    [ForeignKey(nameof(UploadedByEmployeeId))]
    public Employee UploadedByEmployee { get; set; } = null!;
}