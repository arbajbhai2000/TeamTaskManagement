using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Application.DTOs.Notifications
{
    public class NotificationResponse
    {
        public int Id { get; set; }

        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; }

        public int UserId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}