using Application.DTOs.Notification;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class NotificationService :INotificationService
    {
        private readonly ApplicationDbContext context;
        public NotificationService(ApplicationDbContext context)
        {
            this.context = context;
        }
        public async Task<IEnumerable<NotificationResponseDto>> GetMyNotificationsAsync(string userId)
        {
            var notifications = await context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
            return notifications.Select(n => new NotificationResponseDto
            {
                Id = n.Id,
                Message = n.Message,
                Title = n.Title,
                ReadAt = n.ReadAt,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            });
        }
        public async Task<IEnumerable<NotificationResponseDto>> GetUnreadNotificationsAsync(string userId)
        {
            var notifications = await context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
            return notifications.Select(n => new NotificationResponseDto
            {
                Id = n.Id,
                Message = n.Message,
                Title = n.Title,
                ReadAt = n.ReadAt,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            });
        }
        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
        }
        public async Task<bool> MarkAsReadAsync(int notificationId, string userId)
        {
            var notification = await context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
            if (notification == null)
            {
                return false; // Notification not found or does not belong to the user
            }
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            return true;
        }
        public async Task MarkAllAsReadAsync(string userId)
        {
            var notifications = await context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();
            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }
            await context.SaveChangesAsync();
        }
        public async Task<bool> DeleteNotificationAsync(int notificationId, string userId)
        {
            var notification = await context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
            if (notification == null)
            {
                return false; // Notification not found or does not belong to the user
            }
            context.Notifications.Remove(notification);
            await context.SaveChangesAsync();
            return true;
        }
        public async Task CreateNotificationAsync(
    string userId,
    string title,
    string message)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            context.Notifications.Add(notification);

            await context.SaveChangesAsync();
        }
    }
}
