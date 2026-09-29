using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TeamTaskManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(
            INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,User")]
        public async Task<IActionResult> GetMyNotifications()
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var notifications =
                await _notificationService.GetByUserIdAsync(userId);

            return Ok(notifications);
        }


        [HttpPut("{id}/read")]
        [Authorize(Roles = "Admin,Manager,User")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                var userIdClaim =
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var role =
                    User.FindFirst(ClaimTypes.Role)?.Value;

                if (!int.TryParse(userIdClaim, out var userId))
                    return Unauthorized();

                // Admin can mark any notification as read
                if (role == "Admin")
                {
                    await _notificationService.MarkAsReadAsync(id);
                    return NoContent();
                }

                // Get current user's notifications
                var notifications =
                    await _notificationService.GetByUserIdAsync(userId);

                var notification =
                    notifications.FirstOrDefault(x => x.Id == id);

                if (notification == null)
                    return Forbid();

                await _notificationService.MarkAsReadAsync(id);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }


    }
}