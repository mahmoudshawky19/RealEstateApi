
using Application.Interfaces;
using Domain.Enums;
using Infrastructure.Data;
using Infrastructure.Identity;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Tests.Services
{
    public class ReviewServiceTests
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
            string ownerId,
            PropertyStatus status = PropertyStatus.Approved)
        {
            return new Domain.Entities.Property
            {
                Id = id,
                Title = title,
                Price = 1000000,
                AreaSize = 100,
                UserId = ownerId,
                CategoryId = 1,
                AreaId = 1,
                Status = status
            };
        }

        private Domain.Entities.Review CreateReview(
            int id,
            int propertyId,
            string userId,
            int rating = 5,
            string comment = "Great property")
        {
            return new Domain.Entities.Review
            {
                Id = id,
                PropertyId = propertyId,
                UserId = userId,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.UtcNow
            };
        }

        // 1. Get Reviews By Property Id
        [Fact]
        public async Task GetReviewsByPropertyIdAsync_ShouldReturnPropertyReviews()
        {
            await using var context = CreateContext();

            context.Properties.Add(
                CreateProperty(1, "Apartment", "owner1"));

            context.Users.Add(new ApplicationUser
            {
                Id = "user1",
                FullName = "Ahmed Ali",
                UserName = "ahmed",
                Email = "ahmed@test.com"
            });

            context.Reviews.AddRange(
                CreateReview(1, 1, "user1"),
                CreateReview(2, 1, "user2")
            );

            await context.SaveChangesAsync();

            var notificationMock = new Mock<INotificationService>();
            var service = new ReviewService(context, notificationMock.Object);

            // Act
            var result = await service.GetReviewsByPropertyIdAsync(1);

            // Assert
            var reviews = result.ToList();

            Assert.Equal(2, reviews.Count);
            Assert.All(reviews, r => Assert.Equal(1, r.PropertyId));

            Assert.Equal(
                "Ahmed Ali",
                reviews.First(r => r.Id == 1).ReviewerName);
        }

        // 2. Get Reviews - No Reviews
        [Fact]
        public async Task GetReviewsByPropertyIdAsync_ShouldReturnEmptyList_WhenNoReviewsExist()
        {
            await using var context = CreateContext();

            var notificationMock = new Mock<INotificationService>();
            var service = new ReviewService(context, notificationMock.Object);

            // Act
            var result = await service.GetReviewsByPropertyIdAsync(1);

            // Assert
            Assert.Empty(result);
        }

        // 3. Create Review - Success
        [Fact]
        public async Task CreateReviewAsync_ShouldCreateReviewAndNotification()
        {
            await using var context = CreateContext();

            context.Properties.Add(
                CreateProperty(1, "Apartment", "owner1"));

            await context.Users.AddAsync(
                new ApplicationUser
                {
                    Id = "user1",
                    FullName = "Ahmed Ali",
                    UserName = "ahmed",
                    Email = "ahmed@test.com"
                });

            await context.SaveChangesAsync();

            var notificationMock = new Mock<INotificationService>();
            var service = new ReviewService(context, notificationMock.Object);

            var request = new Application.DTOs.Review.CreateReviewDto
            {
                PropertyId = 1,
                Rating = 5,
                Comment = "Excellent property"
            };

            // Act
            var result = await service.CreateReviewAsync(request, "user1");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal(1, result.PropertyId);
            Assert.Equal(5, result.Rating);
            Assert.Equal("Excellent property", result.Comment);

            var reviewInDatabase =
                await context.Reviews.FindAsync(result.Id);

            Assert.NotNull(reviewInDatabase);

            notificationMock.Verify(
                n => n.CreateNotificationAsync(
                    "owner1",
                    "New Review",
                    It.IsAny<string>()),
                Times.Once);
        }

        // 4. Create Review - Property Does Not Exist
        [Fact]
        public async Task CreateReviewAsync_ShouldThrowArgumentException_WhenPropertyDoesNotExist()
        {
            await using var context = CreateContext();

            var notificationMock = new Mock<INotificationService>();
            var service = new ReviewService(context, notificationMock.Object);

            var request = new Application.DTOs.Review.CreateReviewDto
            {
                PropertyId = 999,
                Rating = 5,
                Comment = "Great property"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => service.CreateReviewAsync(request, "user1"));

            Assert.Empty(context.Reviews);
        }

        // 5. Create Review - Property Not Approved
        [Fact]
        public async Task CreateReviewAsync_ShouldThrowInvalidOperationException_WhenPropertyIsNotApproved()
        {
            await using var context = CreateContext();

            context.Properties.Add(
                CreateProperty(
                    1,
                    "Apartment",
                    "owner1",
                    PropertyStatus.Pending));

            await context.SaveChangesAsync();

            var notificationMock = new Mock<INotificationService>();
            var service = new ReviewService(context, notificationMock.Object);

            var request = new Application.DTOs.Review.CreateReviewDto
            {
                PropertyId = 1,
                Rating = 5,
                Comment = "Great property"
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateReviewAsync(request, "user1"));

            Assert.Empty(context.Reviews);
        }

        // 6. Create Review - Own Property
        [Fact]
        public async Task CreateReviewAsync_ShouldThrowInvalidOperationException_WhenUserOwnsProperty()
        {
            await using var context = CreateContext();

            context.Properties.Add(
                CreateProperty(1, "Apartment", "user1"));

            await context.SaveChangesAsync();

            var notificationMock = new Mock<INotificationService>();
            var service = new ReviewService(context, notificationMock.Object);

            var request = new Application.DTOs.Review.CreateReviewDto
            {
                PropertyId = 1,
                Rating = 5,
                Comment = "Great property"
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateReviewAsync(request, "user1"));

            Assert.Empty(context.Reviews);
        }

        // 7. Create Review - Already Reviewed
        [Fact]
        public async Task CreateReviewAsync_ShouldThrowInvalidOperationException_WhenUserAlreadyReviewedProperty()
        {
            await using var context = CreateContext();

            context.Properties.Add(
                CreateProperty(1, "Apartment", "owner1"));

            context.Reviews.Add(
                CreateReview(1, 1, "user1"));

            await context.SaveChangesAsync();

            var notificationMock = new Mock<INotificationService>();
            var service = new ReviewService(context, notificationMock.Object);

            var request = new Application.DTOs.Review.CreateReviewDto
            {
                PropertyId = 1,
                Rating = 4,
                Comment = "Another review"
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateReviewAsync(request, "user1"));

            Assert.Single(context.Reviews);
        }

        // 8. Update Review - Success
        [Fact]
        public async Task UpdateReviewAsync_ShouldUpdateReview_WhenUserOwnsReview()
        {
            await using var context = CreateContext();

            context.Reviews.Add(
                CreateReview(5, 1, "user1"));

            await context.SaveChangesAsync();

            var notificationMock = new Mock<INotificationService>();
            var service = new ReviewService(context, notificationMock.Object);

            var request = new Application.DTOs.Review.UpdateReviewDto
            {
                Rating = 3,
                Comment = "Updated comment"
            };

            // Act
            var result = await service.UpdateReviewAsync(5, request, "user1");

            // Assert
            Assert.Equal(5, result.Id);
            Assert.Equal(3, result.Rating);
            Assert.Equal("Updated comment", result.Comment);
            Assert.NotNull(result.UpdatedAt);

            var updatedReview = await context.Reviews.FindAsync(5);

            Assert.NotNull(updatedReview);
            Assert.Equal(3, updatedReview.Rating);
            Assert.Equal("Updated comment", updatedReview.Comment);
        }

        // 9. Update Review - Not Owner
        [Fact]
        public async Task UpdateReviewAsync_ShouldThrowException_WhenUserDoesNotOwnReview()
        {
            await using var context = CreateContext();

            context.Reviews.Add(
                CreateReview(5, 1, "user1"));

            await context.SaveChangesAsync();

            var notificationMock = new Mock<INotificationService>();
            var service = new ReviewService(context, notificationMock.Object);

            var request = new Application.DTOs.Review.UpdateReviewDto
            {
                Rating = 3,
                Comment = "Updated comment"
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                () => service.UpdateReviewAsync(5, request, "user2"));
        }

        // 10. Delete Review - Success
        [Fact]
        public async Task DeleteReviewAsync_ShouldDeleteReview_WhenUserOwnsReview()
        {
            await using var context = CreateContext();

            context.Reviews.Add(
                CreateReview(5, 1, "user1"));

            await context.SaveChangesAsync();

            var notificationMock = new Mock<INotificationService>();
            var service = new ReviewService(context, notificationMock.Object);

            // Act
            var result = await service.DeleteReviewAsync(5, "user1");

            // Assert
            Assert.True(result);

            var deletedReview = await context.Reviews.FindAsync(5);

            Assert.Null(deletedReview);
        }

        // 11. Delete Review - Not Owner
        [Fact]
        public async Task DeleteReviewAsync_ShouldThrowException_WhenUserDoesNotOwnReview()
        {
            await using var context = CreateContext();

            context.Reviews.Add(
                CreateReview(5, 1, "user1"));

            await context.SaveChangesAsync();

            var notificationMock = new Mock<INotificationService>();
            var service = new ReviewService(context, notificationMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                () => service.DeleteReviewAsync(5, "user2"));
        }

        // 12. Get Average Rating
        [Fact]
        public async Task GetAverageRatingByPropertyIdAsync_ShouldReturnAverageRating()
        {
            await using var context = CreateContext();

            context.Reviews.AddRange(
                CreateReview(1, 1, "user1", 5),
                CreateReview(2, 1, "user2", 3),
                CreateReview(3, 1, "user3", 4)
            );

            await context.SaveChangesAsync();

            var notificationMock = new Mock<INotificationService>();
            var service = new ReviewService(context, notificationMock.Object);

            // Act
            var result = await service.GetAverageRatingByPropertyIdAsync(1);

            // Assert
            Assert.Equal(4m, result);
        }
    }
}
