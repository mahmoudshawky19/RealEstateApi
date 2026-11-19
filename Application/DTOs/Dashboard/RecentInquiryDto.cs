namespace Application.DTOs.Dashboard
{
    public class RecentInquiryDto
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string PropertyTitle { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}