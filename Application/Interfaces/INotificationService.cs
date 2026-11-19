using Application.DTOs.Notification;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationResponseDto>> GetMyNotificationsAsync(string userId);
        Task<IEnumerable<NotificationResponseDto>> GetUnreadNotificationsAsync(string userId);
        Task<int> GetUnreadCountAsync(string userId);
        Task<bool> MarkAsReadAsync(int notificationId, string userId);
        Task MarkAllAsReadAsync(string userId);
        Task<bool> DeleteNotificationAsync(int notificationId, string userId);
        Task CreateNotificationAsync(string userId, string title, string message);
    }
}
