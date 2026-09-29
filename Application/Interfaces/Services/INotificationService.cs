using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Notifications;

namespace Application.Interfaces.Services
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationResponse>> GetByUserIdAsync(int userId);

        Task<NotificationResponse> CreateAsync(
            string message,
            int userId);

        Task MarkAsReadAsync(int id);
    }
}