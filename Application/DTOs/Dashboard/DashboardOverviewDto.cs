namespace Application.DTOs.Dashboard
{
    public class DashboardOverviewDto
    {
        // Users
        public int TotalUsers { get; set; }

        // Properties
        public int TotalProperties { get; set; }
        public int PendingProperties { get; set; }
        public int ApprovedProperties { get; set; }
        public int RejectedProperties { get; set; }
        public int SoldProperties { get; set; }
        public int RentedProperties { get; set; }

        // Inquiries
        public int TotalInquiries { get; set; }
        public int NewInquiries { get; set; }

        // Reviews
        public int TotalReviews { get; set; }
    }
}