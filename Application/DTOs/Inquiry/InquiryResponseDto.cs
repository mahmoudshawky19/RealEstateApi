using Domain.Enums;

namespace Application.DTOs.Inquiry
{
    public class InquiryResponseDto
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public InquiryStatus Status { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int PropertyId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string PropertyTitle { get; set; } = string.Empty;
    }
}
