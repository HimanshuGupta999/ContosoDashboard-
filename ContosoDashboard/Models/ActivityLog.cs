using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoDashboard.Models;

public class ActivityLog
{
    [Key]
    public int ActivityLogId { get; set; }

    public int? DocumentId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    [MaxLength(100)]
    public string ActionType { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Details { get; set; }

    [Required]
    public DateTime OccurredUtc { get; set; } = DateTime.UtcNow;

    [ForeignKey("DocumentId")]
    public virtual Document? Document { get; set; }

    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
}
