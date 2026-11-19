using Domain.Enums;

namespace Domain.Entities;

public class Inquiry
{
    public int Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public InquiryStatus Status { get; set; } = InquiryStatus.New;

    public string UserId { get; set; } = string.Empty;
    public int PropertyId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Property Property { get; set; } = null!;
}
