
namespace Application.DTOs.Dashboard
{
    public class PropertyStatisticsDto
    {
        public int TotalSaleProperties { get; set; }
        public int TotalRentProperties { get; set; }
        public int TotalFeaturedProperties { get; set; }

        public int PendingProperties { get; set; }
        public int ApprovedProperties { get; set; }
        public int RejectedProperties { get; set; }
        public int SoldProperties { get; set; }
        public int RentedProperties { get; set; }
    }
}
