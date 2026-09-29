using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Comments
{
    public class CreateCommentRequest
    {
        public string Content { get; set; } = string.Empty;

        public int TaskItemId { get; set; }
    }
}