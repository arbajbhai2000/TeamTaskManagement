using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Notifications;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;

namespace Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(
            INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<IEnumerable<NotificationResponse>> GetByUserIdAsync(
            int userId)
        {
            var notifications =
                await _notificationRepository.GetByUserIdAsync(userId);

            return notifications.Select(MapToResponse);
        }

        public async Task<NotificationResponse> CreateAsync(
            string message,
            int userId)
        {
            var notification = new Notification
            {
                Message = message,
                UserId = userId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            var createdNotification =
                await _notificationRepository.AddAsync(notification);

            return MapToResponse(createdNotification);
        }

        public async Task MarkAsReadAsync(int id)
        {
            var notification =
                await _notificationRepository.GetByIdAsync(id);

            if (notification == null)
                throw new KeyNotFoundException("Notification not found.");

            notification.IsRead = true;
            notification.UpdatedAt = DateTime.UtcNow;

            await _notificationRepository.UpdateAsync(notification);
        }

        private static NotificationResponse MapToResponse(
            Notification notification)
        {
            return new NotificationResponse
            {
                Id = notification.Id,
                Message = notification.Message,
                IsRead = notification.IsRead,
                UserId = notification.UserId,
                CreatedAt = notification.CreatedAt
            };
        }
    }
}
