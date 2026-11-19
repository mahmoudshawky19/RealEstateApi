
using Application.Interfaces;
using Domain.Enums;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Tests.Services
{
    public class InquiryServiceTests
    {
        private ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        private Domain.Entities.Property CreateProperty(
            int id,
            string title,
            string ownerId)
        {
            return new Domain.Entities.Property
            {
                Id = id,
                Title = title,
                Price = 1000000,
                AreaSize = 100,
                UserId = ownerId,
                CategoryId = 1,
                AreaId = 1
            };
        }

        private Domain.Entities.Inquiry CreateInquiry(
            int id,
            int propertyId,
            string userId,
            string message = "I am interested in this property.")
        {
            return new Domain.Entities.Inquiry
            {
                Id = id,
                PropertyId = propertyId,
                UserId = userId,
                Message = message,
                Status = InquiryStatus.New,
                CreatedAt = DateTime.UtcNow
            };
        }

        // 1. Get Sent Inquiries - Filter By User
        [Fact]
        public async Task GetSentInquiriesAsync_ShouldReturnOnlyUserInquiries()
        {
            await using var context = CreateContext();

            var property = CreateProperty(1, "Apartment", "owner1");
            context.Properties.Add(property);

            context.Inquiries.AddRange(
                CreateInquiry(1, 1, "user1"),
                CreateInquiry(2, 1, "user2")
            );

            await context.SaveChangesAsync();

            var notificationMock = new Mock<INotificationService>();
            var service = new InquiryService(context, notificationMock.Object);

            // Act
            var result = await service.GetSentInquiriesAsync("user1");

            // Assert
            var inquiries = result.ToList();

            Assert.Single(inquiries);
            Assert.Equal("user1", inquiries[0].UserId);
            Assert.Equal("Apartment", inquiries[0].PropertyTitle);
        }

        // 2. Get Received Inquiries - Filter By Property Owner
        [Fact]
        public async Task GetReceivedInquiriesAsync_ShouldReturnOnlyOwnerInquiries()
        {
            await using var context = CreateContext();

            context.Properties.AddRange(
                CreateProperty(1, "Apartment", "owner1"),
                CreateProperty(2, "Villa", "owner2")
            );

            context.Inquiries.AddRange(
                CreateInquiry(1, 1, "user1"),
                CreateInquiry(2, 2, "user2")
            );

            await context.SaveChangesAsync();

            var notificationMock = new Mock<INotificationService>();
            var service = new InquiryService(context, notificationMock.Object);

            // Act
            var result = await service.GetReceivedInquiriesAsync("owner1");

            // Assert
            var inquiries = result.ToList();

            Assert.Single(inquiries);
            Assert.Equal("Apartment", inquiries[0].PropertyTitle);
        }

        // 3. Get Inquiry By Id - Sender
        [Fact]
        public async Task GetInquiryByIdAsync_ShouldReturnInquiry_WhenUserIsSender()
        {
            await using var context = CreateContext();

            context.Properties.Add(
                CreateProperty(1, "Apartment", "owner1"));

            context.Inquiries.Add(
                CreateInquiry(5, 1, "user1"));

            await context.SaveChangesAsync();

            var notificationMock = new Mock<INotificationService>();
            var service = new InquiryService(context, notificationMock.Object);

            // Act
            var result = await service.GetInquiryByIdAsync(5, "user1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Id);
            Assert.Equal("user1", result.UserId);
            Assert.Equal("Apartment", result.PropertyTitle);
        }

        // 4. Get Inquiry By Id - Property Owner
        [Fact]
        public async Task GetInquiryByIdAsync_ShouldReturnInquiry_WhenUserIsPropertyOwner()
        {
            await using var context = CreateContext();

            context.Properties.Add(
                CreateProperty(1, "Apartment", "owner1"));

            context.Inquiries.Add(
                CreateInquiry(5, 1, "user1"));

            await context.SaveChangesAsync();

            var notificationMock = new Mock<INotificationService>();
            var service = new InquiryService(context, notificationMock.Object);

            // Act
            var result = await service.GetInquiryByIdAsync(5, "owner1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Id);
            Assert.Equal("Apartment", result.PropertyTitle);
        }

        // 5. Get Inquiry By Id - Unauthorized User
        [Fact]
        public async Task GetInquiryByIdAsync_ShouldReturnNull_WhenUserIsNotSenderOrOwner()
        {
            await using var context = CreateContext();

            context.Properties.Add(
                CreateProperty(1, "Apartment", "owner1"));

            context.Inquiries.Add(
                CreateInquiry(5, 1, "user1"));

            await context.SaveChangesAsync();

            var notificationMock = new Mock<INotificationService>();
            var service = new InquiryService(context, notificationMock.Object);

            // Act
            var result = await service.GetInquiryByIdAsync(5, "user3");

            // Assert
            Assert.Null(result);
        }

        // 6. Create Inquiry - Success
        [Fact]
        public async Task CreateInquiryAsync_ShouldCreateInquiryAndNotification()
        {
            await using var context = CreateContext();

            context.Properties.Add(
                CreateProperty(1, "Apartment", "owner1"));

            await context.SaveChangesAsync();

            var notificationMock = new Mock<INotificationService>();
            var service = new InquiryService(context, notificationMock.Object);

            var request = new Application.DTOs.Inquiry.CreateInquiryDto
            {
                PropertyId = 1,
                Message = "I want to know more about this property."
            };

            // Act
            var result = await service.CreateInquiryAsync(request, "user1");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("user1", result.UserId);
            Assert.Equal(1, result.PropertyId);
            Assert.Equal("Apartment", result.PropertyTitle);
            Assert.Equal(
                InquiryStatus.New,
                result.Status);

            var inquiryInDatabase = await context.Inquiries
                .FirstOrDefaultAsync(i => i.Id == result.Id);

            Assert.NotNull(inquiryInDatabase);

            notificationMock.Verify(
                n => n.CreateNotificationAsync(
                    "owner1",
                    "New Inquiry",
                    It.IsAny<string>()),
                Times.Once);
        }

        // 7. Create Inquiry - Property Does Not Exist
        [Fact]
        public async Task CreateInquiryAsync_ShouldThrowKeyNotFoundException_WhenPropertyDoesNotExist()
        {
            await using var context = CreateContext();

            var notificationMock = new Mock<INotificationService>();
            var service = new InquiryService(context, notificationMock.Object);

            var request = new Application.DTOs.Inquiry.CreateInquiryDto
            {
                PropertyId = 999,
                Message = "I am interested."
            };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => service.CreateInquiryAsync(request, "user1"));

            Assert.Empty(context.Inquiries);
        }

        // 8. Create Inquiry - Own Property
        [Fact]
        public async Task CreateInquiryAsync_ShouldThrowInvalidOperationException_WhenUserOwnsProperty()
        {
            await using var context = CreateContext();

            context.Properties.Add(
                CreateProperty(1, "Apartment", "user1"));

            await context.SaveChangesAsync();

            var notificationMock = new Mock<INotificationService>();
            var service = new InquiryService(context, notificationMock.Object);

            var request = new Application.DTOs.Inquiry.CreateInquiryDto
            {
                PropertyId = 1,
                Message = "I am interested."
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateInquiryAsync(request, "user1"));

            Assert.Empty(context.Inquiries);
        }

        // 9. Update Inquiry Status - Success
        [Fact]
        public async Task UpdateInquiryStatusAsync_ShouldUpdateStatus_WhenUserIsPropertyOwner()
        {
            await using var context = CreateContext();

            context.Properties.Add(
                CreateProperty(1, "Apartment", "owner1"));

            context.Inquiries.Add(
                CreateInquiry(5, 1, "user1"));

            await context.SaveChangesAsync();

            var notificationMock = new Mock<INotificationService>();
            var service = new InquiryService(context, notificationMock.Object);

            var request = new Application.DTOs.Inquiry.UpdateInquiryStatusDto
            {
                Status = InquiryStatus.InProgress
            };

            // Act
            var result = await service.UpdateInquiryStatusAsync(
                5,
                request,
                "owner1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(InquiryStatus.InProgress, result.Status);
            Assert.NotNull(result.UpdatedAt);

            var updatedInquiry = await context.Inquiries.FindAsync(5);

            Assert.NotNull(updatedInquiry);
            Assert.Equal(InquiryStatus.InProgress, updatedInquiry.Status);
        }

        // 10. Update Inquiry Status - Does Not Exist
        [Fact]
        public async Task UpdateInquiryStatusAsync_ShouldReturnNull_WhenInquiryDoesNotExist()
        {
            await using var context = CreateContext();

            var notificationMock = new Mock<INotificationService>();
            var service = new InquiryService(context, notificationMock.Object);

            var request = new Application.DTOs.Inquiry.UpdateInquiryStatusDto
            {
                Status = InquiryStatus.InProgress
            };

            // Act
            var result = await service.UpdateInquiryStatusAsync(
                999,
                request,
                "owner1");

            // Assert
            Assert.Null(result);
        }

        // 11. Update Inquiry Status - Not Property Owner
        [Fact]
        public async Task UpdateInquiryStatusAsync_ShouldThrowUnauthorizedAccessException_WhenUserIsNotPropertyOwner()
        {
            await using var context = CreateContext();

            context.Properties.Add(
                CreateProperty(1, "Apartment", "owner1"));

            context.Inquiries.Add(
                CreateInquiry(5, 1, "user1"));

            await context.SaveChangesAsync();

            var notificationMock = new Mock<INotificationService>();
            var service = new InquiryService(context, notificationMock.Object);

            var request = new Application.DTOs.Inquiry.UpdateInquiryStatusDto
            {
                Status = InquiryStatus.InProgress
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => service.UpdateInquiryStatusAsync(
                    5,
                    request,
                    "user1"));
        }

        // 12. Update Inquiry Status - Invalid Status
        [Fact]
        public async Task UpdateInquiryStatusAsync_ShouldThrowArgumentException_WhenStatusIsInvalid()
        {
            await using var context = CreateContext();

            context.Properties.Add(
                CreateProperty(1, "Apartment", "owner1"));

            context.Inquiries.Add(
                CreateInquiry(5, 1, "user1"));

            await context.SaveChangesAsync();

            var notificationMock = new Mock<INotificationService>();
            var service = new InquiryService(context, notificationMock.Object);

            var request = new Application.DTOs.Inquiry.UpdateInquiryStatusDto
            {
                Status = (InquiryStatus)999
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => service.UpdateInquiryStatusAsync(
                    5,
                    request,
                    "owner1"));
        }

        // 13. Update Inquiry Status - Sender Cannot Update
        [Fact]
        public async Task UpdateInquiryStatusAsync_ShouldNotChangeStatus_WhenUnauthorizedUserTriesToUpdate()
        {
            await using var context = CreateContext();

            context.Properties.Add(
                CreateProperty(1, "Apartment", "owner1"));

            context.Inquiries.Add(
                CreateInquiry(5, 1, "user1"));

            await context.SaveChangesAsync();

            var notificationMock = new Mock<INotificationService>();
            var service = new InquiryService(context, notificationMock.Object);

            var request = new Application.DTOs.Inquiry.UpdateInquiryStatusDto
            {
                Status = InquiryStatus.Closed
            };

            // Act
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => service.UpdateInquiryStatusAsync(
                    5,
                    request,
                    "user1"));

            // Assert
            var inquiry = await context.Inquiries.FindAsync(5);

            Assert.NotNull(inquiry);
            Assert.Equal(InquiryStatus.New, inquiry.Status);
        }
    }
}
