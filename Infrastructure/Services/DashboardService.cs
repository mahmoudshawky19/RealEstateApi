using Application.DTOs.Dashboard;
using Application.Interfaces;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;
        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<DashboardOverviewDto> GetOverviewAsync()
        {
            // 1. Users Statistics
            var totalUsers = await _context.Users.CountAsync();

            // 2. Properties Statistics
            var propertyStats = await _context.Properties
                .GroupBy(p => p.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToDictionaryAsync(x => x.Status, x => x.Count);

            // Get property counts safely
            var totalProperties = propertyStats.Values.Sum();

            var pendingProperties = propertyStats.GetValueOrDefault(PropertyStatus.Pending);
            var approvedProperties = propertyStats.GetValueOrDefault(PropertyStatus.Approved);
            var rejectedProperties = propertyStats.GetValueOrDefault(PropertyStatus.Rejected);
            var soldProperties = propertyStats.GetValueOrDefault(PropertyStatus.Sold);
            var rentedProperties = propertyStats.GetValueOrDefault(PropertyStatus.Rented);

            // 3. Inquiries Statistics
            var totalInquiries = await _context.Inquiries.CountAsync();

            var newInquiries = await _context.Inquiries
                .CountAsync(i => i.Status == InquiryStatus.New);

            // 4. Reviews Statistics
            var totalReviews = await _context.Reviews.CountAsync();

            // 5. Return Dashboard Data
            return new DashboardOverviewDto
            {
                TotalUsers = totalUsers,

                TotalProperties = totalProperties,
                PendingProperties = pendingProperties,
                ApprovedProperties = approvedProperties,
                RejectedProperties = rejectedProperties,
                SoldProperties = soldProperties,
                RentedProperties = rentedProperties,

                TotalInquiries = totalInquiries,
                NewInquiries = newInquiries,

                TotalReviews = totalReviews
            };
        }

 

        public async Task<IEnumerable<RecentPropertyDto>> GetRecentPropertiesAsync(int count = 5)
        {
            return await (
                from property in _context.Properties.AsNoTracking()
                join user in _context.Users.AsNoTracking()
                    on property.UserId equals user.Id
                orderby property.CreatedAt descending
                select new RecentPropertyDto
                {
                    Id = property.Id,
                    Title = property.Title,
                    Price = property.Price,
                    OwnerName = user.FullName,
                    CreatedAt = property.CreatedAt,
                    Status = property.Status.ToString()
                }
            )
            .Take(count)
            .ToListAsync();
        }

        public async Task<IEnumerable<RecentUserDto>> GetRecentUsersAsync(int count = 5)
        {
            return await _context.Users
                .AsNoTracking()
                .OrderByDescending(u => u.CreatedAt)
                .Take(count)
                .Select(u => new RecentUserDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<RecentInquiryDto>> GetRecentInquiriesAsync(int count = 5)
        {
            return await (
                from inquiry in _context.Inquiries.AsNoTracking()
                join user in _context.Users.AsNoTracking()
                    on inquiry.UserId equals user.Id
                join property in _context.Properties.AsNoTracking()
                    on inquiry.PropertyId equals property.Id
                orderby inquiry.CreatedAt descending
                select new RecentInquiryDto
                {
                    Id = inquiry.Id,
                    Message = inquiry.Message,
                    SenderName = user.FullName,
                    PropertyTitle = property.Title,
                    CreatedAt = inquiry.CreatedAt,
                    Status = inquiry.Status.ToString()
                }
            )
            .Take(count)
            .ToListAsync();
        }

    }
}
