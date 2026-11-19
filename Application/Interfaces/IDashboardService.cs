using Application.DTOs.Dashboard;

namespace Application.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardOverviewDto> GetOverviewAsync();
         Task<IEnumerable<RecentPropertyDto>> GetRecentPropertiesAsync(int count = 5);

        Task<IEnumerable<RecentUserDto>> GetRecentUsersAsync(int count = 5);
        Task<IEnumerable<RecentInquiryDto>> GetRecentInquiriesAsync(int count = 5);
    }
}