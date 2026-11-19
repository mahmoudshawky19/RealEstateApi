
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Infrastructure.Identity;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Tests.Services
{
    public class DashboardServiceTests
    {
        private ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        private async Task SeedBaseDataAsync(ApplicationDbContext context)
        {
            context.Users.AddRange(
                new ApplicationUser
                {
                    Id = "user-1",
                    UserName = "user1@test.com",
                    Email = "user1@test.com",
                    FullName = "Ahmed",
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new ApplicationUser
                {
                    Id = "user-2",
                    UserName = "user2@test.com",
                    Email = "user2@test.com",
                    FullName = "Mohamed",
                    CreatedAt = new DateTime(2026, 2, 1)
                });

            context.Properties.AddRange(
                new Property
                {
                    Id = 1,
                    Title = "Apartment 1",
                    Price = 1000000,
                    UserId = "user-1",
                    Status = PropertyStatus.Approved,
                    CreatedAt = new DateTime(2026, 3, 1)
                },
                new Property
                {
                    Id = 2,
                    Title = "Apartment 2",
                    Price = 1500000,
                    UserId = "user-2",
                    Status = PropertyStatus.Pending,
                    CreatedAt = new DateTime(2026, 3, 2)
                });

            await context.SaveChangesAsync();
        }

        // =========================================
        // GetOverviewAsync
        // =========================================

        [Fact]
        public async Task GetOverviewAsync_NoData_ReturnsZeroStatistics()
        {
            // Arrange
            using var context = CreateContext();
            var service = new DashboardService(context);

            // Act
            var result = await service.GetOverviewAsync();

            // Assert
            Assert.Equal(0, result.TotalUsers);
            Assert.Equal(0, result.TotalProperties);
            Assert.Equal(0, result.PendingProperties);
            Assert.Equal(0, result.ApprovedProperties);
            Assert.Equal(0, result.RejectedProperties);
            Assert.Equal(0, result.SoldProperties);
            Assert.Equal(0, result.RentedProperties);
            Assert.Equal(0, result.TotalInquiries);
            Assert.Equal(0, result.NewInquiries);
            Assert.Equal(0, result.TotalReviews);
        }

        [Fact]
        public async Task GetOverviewAsync_WithData_ReturnsCorrectStatistics()
        {
            // Arrange
            using var context = CreateContext();
            await SeedBaseDataAsync(context);

            context.Properties.AddRange(
                new Property
                {
                    Id = 3,
                    Title = "Apartment 3",
                    UserId = "user-1",
                    Status = PropertyStatus.Rejected
                },
                new Property
                {
                    Id = 4,
                    Title = "Apartment 4",
                    UserId = "user-1",
                    Status = PropertyStatus.Sold
                },
                new Property
                {
                    Id = 5,
                    Title = "Apartment 5",
                    UserId = "user-2",
                    Status = PropertyStatus.Rented
                });

            context.Inquiries.AddRange(
                new Inquiry
                {
                    Id = 1,
                    Message = "Is this available?",
                    UserId = "user-2",
                    PropertyId = 1,
                    Status = InquiryStatus.New
                },
                new Inquiry
                {
                    Id = 2,
                    Message = "I am interested",
                    UserId = "user-1",
                    PropertyId = 2,
                    Status = InquiryStatus.Replied
                });

            context.Reviews.Add(
                new Review
                {
                    Id = 1,
                    UserId = "user-2",
                    PropertyId = 1,
                    Rating = 5,
                    Comment = "Excellent"
                });

            await context.SaveChangesAsync();

            var service = new DashboardService(context);

            // Act
            var result = await service.GetOverviewAsync();

            // Assert
            Assert.Equal(2, result.TotalUsers);

            Assert.Equal(5, result.TotalProperties);
            Assert.Equal(1, result.PendingProperties);
            Assert.Equal(1, result.ApprovedProperties);
            Assert.Equal(1, result.RejectedProperties);
            Assert.Equal(1, result.SoldProperties);
            Assert.Equal(1, result.RentedProperties);

            Assert.Equal(2, result.TotalInquiries);
            Assert.Equal(1, result.NewInquiries);

            Assert.Equal(1, result.TotalReviews);
        }

        // =========================================
        // GetRecentPropertiesAsync
        // =========================================

        [Fact]
        public async Task GetRecentPropertiesAsync_ReturnsPropertiesOrderedByNewest()
        {
            // Arrange
            using var context = CreateContext();
            await SeedBaseDataAsync(context);

            var service = new DashboardService(context);

            // Act
            var result = (await service.GetRecentPropertiesAsync()).ToList();

            // Assert
            Assert.Equal(2, result.Count);

            Assert.Equal(2, result[0].Id);
            Assert.Equal("Apartment 2", result[0].Title);
            Assert.Equal("Mohamed", result[0].OwnerName);
            Assert.Equal(PropertyStatus.Pending.ToString(), result[0].Status);

            Assert.Equal(1, result[1].Id);
            Assert.Equal("Ahmed", result[1].OwnerName);
        }

        [Fact]
        public async Task GetRecentPropertiesAsync_WithCount_ReturnsLimitedResults()
        {
            // Arrange
            using var context = CreateContext();
            await SeedBaseDataAsync(context);

            var service = new DashboardService(context);

            // Act
            var result = await service.GetRecentPropertiesAsync(1);

            // Assert
            var properties = result.ToList();

            Assert.Single(properties);
            Assert.Equal(2, properties[0].Id);
        }

        [Fact]
        public async Task GetRecentPropertiesAsync_NoProperties_ReturnsEmptyCollection()
        {
            // Arrange
            using var context = CreateContext();
            var service = new DashboardService(context);

            // Act
            var result = await service.GetRecentPropertiesAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetRecentPropertiesAsync_PropertyWithoutMatchingUser_IsNotReturned()
        {
            // Arrange
            using var context = CreateContext();

            context.Properties.Add(new Property
            {
                Id = 1,
                Title = "Apartment",
                UserId = "unknown-user",
                CreatedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();

            var service = new DashboardService(context);

            // Act
            var result = await service.GetRecentPropertiesAsync();

            // Assert
            Assert.Empty(result);
        }

        // =========================================
        // GetRecentUsersAsync
        // =========================================

        [Fact]
        public async Task GetRecentUsersAsync_ReturnsUsersOrderedByNewest()
        {
            // Arrange
            using var context = CreateContext();
            await SeedBaseDataAsync(context);

            var service = new DashboardService(context);

            // Act
            var result = (await service.GetRecentUsersAsync()).ToList();

            // Assert
            Assert.Equal(2, result.Count);

            Assert.Equal("user-2", result[0].Id);
            Assert.Equal("Mohamed", result[0].FullName);
            Assert.Equal("user2@test.com", result[0].Email);

            Assert.Equal("user-1", result[1].Id);
            Assert.Equal("Ahmed", result[1].FullName);
        }

        [Fact]
        public async Task GetRecentUsersAsync_WithCount_ReturnsLimitedResults()
        {
            // Arrange
            using var context = CreateContext();
            await SeedBaseDataAsync(context);

            var service = new DashboardService(context);

            // Act
            var result = await service.GetRecentUsersAsync(1);

            // Assert
            var users = result.ToList();

            Assert.Single(users);
            Assert.Equal("user-2", users[0].Id);
        }

        [Fact]
        public async Task GetRecentUsersAsync_NoUsers_ReturnsEmptyCollection()
        {
            // Arrange
            using var context = CreateContext();
            var service = new DashboardService(context);

            // Act
            var result = await service.GetRecentUsersAsync();

            // Assert
            Assert.Empty(result);
        }

        // =========================================
        // GetRecentInquiriesAsync
        // =========================================

        [Fact]
        public async Task GetRecentInquiriesAsync_ReturnsInquiriesOrderedByNewest()
        {
            // Arrange
            using var context = CreateContext();
            await SeedBaseDataAsync(context);

            context.Inquiries.AddRange(
                new Inquiry
                {
                    Id = 1,
                    Message = "First inquiry",
                    UserId = "user-1",
                    PropertyId = 1,
                    Status = InquiryStatus.New,
                    CreatedAt = new DateTime(2026, 4, 1)
                },
                new Inquiry
                {
                    Id = 2,
                    Message = "Second inquiry",
                    UserId = "user-2",
                    PropertyId = 2,
                    Status = InquiryStatus.Replied,
                    CreatedAt = new DateTime(2026, 4, 2)
                });

            await context.SaveChangesAsync();

            var service = new DashboardService(context);

            // Act
            var result = (await service.GetRecentInquiriesAsync()).ToList();

            // Assert
            Assert.Equal(2, result.Count);

            Assert.Equal(2, result[0].Id);
            Assert.Equal("Second inquiry", result[0].Message);
            Assert.Equal("Mohamed", result[0].SenderName);
            Assert.Equal("Apartment 2", result[0].PropertyTitle);
            Assert.Equal(InquiryStatus.Replied.ToString(), result[0].Status);

            Assert.Equal(1, result[1].Id);
            Assert.Equal("Ahmed", result[1].SenderName);
        }

        [Fact]
        public async Task GetRecentInquiriesAsync_WithCount_ReturnsLimitedResults()
        {
            // Arrange
            using var context = CreateContext();
            await SeedBaseDataAsync(context);

            context.Inquiries.AddRange(
                new Inquiry
                {
                    Id = 1,
                    Message = "First",
                    UserId = "user-1",
                    PropertyId = 1,
                    CreatedAt = new DateTime(2026, 4, 1)
                },
                new Inquiry
                {
                    Id = 2,
                    Message = "Second",
                    UserId = "user-2",
                    PropertyId = 2,
                    CreatedAt = new DateTime(2026, 4, 2)
                });

            await context.SaveChangesAsync();

            var service = new DashboardService(context);

            // Act
            var result = await service.GetRecentInquiriesAsync(1);

            // Assert
            var inquiries = result.ToList();

            Assert.Single(inquiries);
            Assert.Equal(2, inquiries[0].Id);
        }

        [Fact]
        public async Task GetRecentInquiriesAsync_NoInquiries_ReturnsEmptyCollection()
        {
            // Arrange
            using var context = CreateContext();
            var service = new DashboardService(context);

            // Act
            var result = await service.GetRecentInquiriesAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetRecentInquiriesAsync_InquiryWithoutMatchingUser_IsNotReturned()
        {
            // Arrange
            using var context = CreateContext();
            await SeedBaseDataAsync(context);

            context.Inquiries.Add(new Inquiry
            {
                Id = 1,
                Message = "Inquiry",
                UserId = "unknown-user",
                PropertyId = 1
            });

            await context.SaveChangesAsync();

            var service = new DashboardService(context);

            // Act
            var result = await service.GetRecentInquiriesAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetRecentInquiriesAsync_InquiryWithoutMatchingProperty_IsNotReturned()
        {
            // Arrange
            using var context = CreateContext();

            context.Users.Add(new ApplicationUser
            {
                Id = "user-1",
                UserName = "user1@test.com",
                Email = "user1@test.com",
                FullName = "Ahmed"
            });

            context.Inquiries.Add(new Inquiry
            {
                Id = 1,
                Message = "Inquiry",
                UserId = "user-1",
                PropertyId = 999
            });

            await context.SaveChangesAsync();

            var service = new DashboardService(context);

            // Act
            var result = await service.GetRecentInquiriesAsync();

            // Assert
            Assert.Empty(result);
        }
    }
}
