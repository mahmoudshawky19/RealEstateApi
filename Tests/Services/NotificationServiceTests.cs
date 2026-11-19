
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Tests.Services
{
    public class NotificationServiceTests
    {
        private ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        private Notification CreateNotification(
            int id,
            string userId = "user-1",
            string title = "Test Title",
            string message = "Test Message",
            bool isRead = false,
            DateTime? createdAt = null)
        {
            return new Notification
            {
                Id = id,
                UserId = userId,
                Title = title,
                Message = message,
                IsRead = isRead,
                ReadAt = isRead ? DateTime.UtcNow : null,
                CreatedAt = createdAt ?? DateTime.UtcNow
            };
        }

        // =========================================
        // GetMyNotificationsAsync
        // =========================================

        [Fact]
        public async Task GetMyNotificationsAsync_ReturnsOnlyUserNotifications()
        {
            // Arrange
            using var context = CreateContext();

            context.Notifications.AddRange(
                CreateNotification(1, "user-1"),
                CreateNotification(2, "user-1"),
                CreateNotification(3, "user-2"));

            await context.SaveChangesAsync();

            var service = new NotificationService(context);

            // Act
            var result = (await service.GetMyNotificationsAsync("user-1"))
                .ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, n => Assert.Equal("Test Title", n.Title));
            Assert.DoesNotContain(result, n => n.Id == 3);
        }

        [Fact]
        public async Task GetMyNotificationsAsync_ReturnsNotificationsOrderedByNewest()
        {
            // Arrange
            using var context = CreateContext();

            context.Notifications.AddRange(
                CreateNotification(
                    1,
                    createdAt: new DateTime(2026, 1, 1)),
                CreateNotification(
                    2,
                    createdAt: new DateTime(2026, 1, 3)),
                CreateNotification(
                    3,
                    createdAt: new DateTime(2026, 1, 2)));

            await context.SaveChangesAsync();

            var service = new NotificationService(context);

            // Act
            var result = (await service.GetMyNotificationsAsync("user-1"))
                .ToList();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal(2, result[0].Id);
            Assert.Equal(3, result[1].Id);
            Assert.Equal(1, result[2].Id);
        }

        [Fact]
        public async Task GetMyNotificationsAsync_NoNotifications_ReturnsEmptyCollection()
        {
            // Arrange
            using var context = CreateContext();
            var service = new NotificationService(context);

            // Act
            var result = await service.GetMyNotificationsAsync("user-1");

            // Assert
            Assert.Empty(result);
        }

        // =========================================
        // GetUnreadNotificationsAsync
        // =========================================

        [Fact]
        public async Task GetUnreadNotificationsAsync_ReturnsOnlyUnreadUserNotifications()
        {
            // Arrange
            using var context = CreateContext();

            context.Notifications.AddRange(
                CreateNotification(1, "user-1", isRead: false),
                CreateNotification(2, "user-1", isRead: true),
                CreateNotification(3, "user-2", isRead: false));

            await context.SaveChangesAsync();

            var service = new NotificationService(context);

            // Act
            var result = (await service.GetUnreadNotificationsAsync("user-1"))
                .ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result[0].Id);
            Assert.False(result[0].IsRead);
        }

        [Fact]
        public async Task GetUnreadNotificationsAsync_ReturnsNotificationsOrderedByNewest()
        {
            // Arrange
            using var context = CreateContext();

            context.Notifications.AddRange(
                CreateNotification(
                    1,
                    createdAt: new DateTime(2026, 1, 1)),
                CreateNotification(
                    2,
                    createdAt: new DateTime(2026, 1, 3)),
                CreateNotification(
                    3,
                    createdAt: new DateTime(2026, 1, 2)));

            await context.SaveChangesAsync();

            var service = new NotificationService(context);

            // Act
            var result = (await service.GetUnreadNotificationsAsync("user-1"))
                .ToList();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal(2, result[0].Id);
            Assert.Equal(3, result[1].Id);
            Assert.Equal(1, result[2].Id);
        }

        [Fact]
        public async Task GetUnreadNotificationsAsync_NoUnreadNotifications_ReturnsEmptyCollection()
        {
            // Arrange
            using var context = CreateContext();

            context.Notifications.Add(
                CreateNotification(1, isRead: true));

            await context.SaveChangesAsync();

            var service = new NotificationService(context);

            // Act
            var result = await service.GetUnreadNotificationsAsync("user-1");

            // Assert
            Assert.Empty(result);
        }

        // =========================================
        // GetUnreadCountAsync
        // =========================================

        [Fact]
        public async Task GetUnreadCountAsync_ReturnsCorrectCount()
        {
            // Arrange
            using var context = CreateContext();

            context.Notifications.AddRange(
                CreateNotification(1, "user-1", isRead: false),
                CreateNotification(2, "user-1", isRead: false),
                CreateNotification(3, "user-1", isRead: true),
                CreateNotification(4, "user-2", isRead: false));

            await context.SaveChangesAsync();

            var service = new NotificationService(context);

            // Act
            var result = await service.GetUnreadCountAsync("user-1");

            // Assert
            Assert.Equal(2, result);
        }

        [Fact]
        public async Task GetUnreadCountAsync_NoUnreadNotifications_ReturnsZero()
        {
            // Arrange
            using var context = CreateContext();
            var service = new NotificationService(context);

            // Act
            var result = await service.GetUnreadCountAsync("user-1");

            // Assert
            Assert.Equal(0, result);
        }

        // =========================================
        // MarkAsReadAsync
        // =========================================

        [Fact]
        public async Task MarkAsReadAsync_NotificationExists_MarksAsRead()
        {
            // Arrange
            using var context = CreateContext();

            var notification = CreateNotification(
                1,
                isRead: false);

            context.Notifications.Add(notification);
            await context.SaveChangesAsync();

            var service = new NotificationService(context);

            // Act
            var result = await service.MarkAsReadAsync(1, "user-1");

            // Assert
            Assert.True(result);
            Assert.True(notification.IsRead);
            Assert.NotNull(notification.ReadAt);

            var savedNotification = await context.Notifications
                .AsNoTracking()
                .FirstAsync(n => n.Id == 1);

            Assert.True(savedNotification.IsRead);
            Assert.NotNull(savedNotification.ReadAt);
        }

        [Fact]
        public async Task MarkAsReadAsync_NotificationDoesNotExist_ReturnsFalse()
        {
            // Arrange
            using var context = CreateContext();
            var service = new NotificationService(context);

            // Act
            var result = await service.MarkAsReadAsync(999, "user-1");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task MarkAsReadAsync_NotificationBelongsToAnotherUser_ReturnsFalse()
        {
            // Arrange
            using var context = CreateContext();

            context.Notifications.Add(
                CreateNotification(1, "user-2"));

            await context.SaveChangesAsync();

            var service = new NotificationService(context);

            // Act
            var result = await service.MarkAsReadAsync(1, "user-1");

            // Assert
            Assert.False(result);

            var notification = await context.Notifications
                .AsNoTracking()
                .FirstAsync(n => n.Id == 1);

            Assert.False(notification.IsRead);
        }

        // =========================================
        // MarkAllAsReadAsync
        // =========================================

        [Fact]
        public async Task MarkAllAsReadAsync_MarksAllUnreadUserNotifications()
        {
            // Arrange
            using var context = CreateContext();

            context.Notifications.AddRange(
                CreateNotification(1, "user-1", isRead: false),
                CreateNotification(2, "user-1", isRead: false),
                CreateNotification(3, "user-1", isRead: true),
                CreateNotification(4, "user-2", isRead: false));

            await context.SaveChangesAsync();

            var service = new NotificationService(context);

            // Act
            await service.MarkAllAsReadAsync("user-1");

            // Assert
            var notifications = await context.Notifications
                .AsNoTracking()
                .ToListAsync();

            var userNotifications = notifications
                .Where(n => n.UserId == "user-1")
                .ToList();

            Assert.All(
                userNotifications,
                n => Assert.True(n.IsRead));

            Assert.All(
                userNotifications,
                n => Assert.NotNull(n.ReadAt));

            var otherUserNotification = notifications
                .First(n => n.Id == 4);

            Assert.False(otherUserNotification.IsRead);
        }

        [Fact]
        public async Task MarkAllAsReadAsync_NoUnreadNotifications_CompletesSuccessfully()
        {
            // Arrange
            using var context = CreateContext();

            context.Notifications.Add(
                CreateNotification(1, isRead: true));

            await context.SaveChangesAsync();

            var service = new NotificationService(context);

            // Act
            var exception = await Record.ExceptionAsync(
                () => service.MarkAllAsReadAsync("user-1"));

            // Assert
            Assert.Null(exception);
        }

        // =========================================
        // DeleteNotificationAsync
        // =========================================

        [Fact]
        public async Task DeleteNotificationAsync_NotificationExists_DeletesNotification()
        {
            // Arrange
            using var context = CreateContext();

            context.Notifications.Add(
                CreateNotification(1));

            await context.SaveChangesAsync();

            var service = new NotificationService(context);

            // Act
            var result = await service.DeleteNotificationAsync(1, "user-1");

            // Assert
            Assert.True(result);

            var notification = await context.Notifications
                .FindAsync(1);

            Assert.Null(notification);
        }

        [Fact]
        public async Task DeleteNotificationAsync_NotificationDoesNotExist_ReturnsFalse()
        {
            // Arrange
            using var context = CreateContext();
            var service = new NotificationService(context);

            // Act
            var result = await service.DeleteNotificationAsync(999, "user-1");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteNotificationAsync_NotificationBelongsToAnotherUser_ReturnsFalse()
        {
            // Arrange
            using var context = CreateContext();

            context.Notifications.Add(
                CreateNotification(1, "user-2"));

            await context.SaveChangesAsync();

            var service = new NotificationService(context);

            // Act
            var result = await service.DeleteNotificationAsync(1, "user-1");

            // Assert
            Assert.False(result);

            Assert.True(
                await context.Notifications.AnyAsync(n => n.Id == 1));
        }

        // =========================================
        // CreateNotificationAsync
        // =========================================

        [Fact]
        public async Task CreateNotificationAsync_ValidData_CreatesNotification()
        {
            // Arrange
            using var context = CreateContext();
            var service = new NotificationService(context);

            // Act
            await service.CreateNotificationAsync(
                "user-1",
                "Property Approved",
                "Your property has been approved.");

            // Assert
            var notification = await context.Notifications
                .SingleAsync();

            Assert.Equal("user-1", notification.UserId);
            Assert.Equal("Property Approved", notification.Title);
            Assert.Equal(
                "Your property has been approved.",
                notification.Message);

            Assert.False(notification.IsRead);
            Assert.Null(notification.ReadAt);
            Assert.NotEqual(default, notification.CreatedAt);
        }
    }
}
