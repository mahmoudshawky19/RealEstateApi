
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace RealEstate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // GET: api/Notifications
        [HttpGet]
        public async Task<IActionResult> GetMyNotifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var notifications =
                await _notificationService.GetMyNotificationsAsync(userId);

            return Ok(notifications);
        }

        // GET: api/Notifications/unread
        [HttpGet("unread")]
        public async Task<IActionResult> GetUnreadNotifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var notifications =
                await _notificationService.GetUnreadNotificationsAsync(userId);

            return Ok(notifications);
        }

        // GET: api/Notifications/unread-count
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var count =
                await _notificationService.GetUnreadCountAsync(userId);

            return Ok(new { unreadCount = count });
        }

        // PUT: api/Notifications/5/read
        [HttpPut("{notificationId:int}/read")]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            if (notificationId <= 0)
                return BadRequest("Invalid notification ID.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _notificationService.MarkAsReadAsync(
                notificationId, userId);

            if (!result)
                return NotFound("Notification not found.");

            return NoContent();
        }

        // PUT: api/Notifications/read-all
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            await _notificationService.MarkAllAsReadAsync(userId);

            return NoContent();
        }

        // DELETE: api/Notifications/5
        [HttpDelete("{notificationId:int}")]
        public async Task<IActionResult> DeleteNotification(int notificationId)
        {
            if (notificationId <= 0)
                return BadRequest("Invalid notification ID.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _notificationService.DeleteNotificationAsync(
                notificationId, userId);

            if (!result)
                return NotFound("Notification not found.");

            return NoContent();
        }
    }
}
