using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Comments
{
    public class CommentResponse
    {
        public int Id { get; set; }

        public string Content { get; set; } = string.Empty;

        public int TaskItemId { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}