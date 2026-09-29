using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Comments;


namespace Application.Interfaces.Services
{
    public interface ICommentService
    {
        Task<IEnumerable<CommentResponse>> GetByTaskIdAsync(int taskId);

        Task<CommentResponse?> GetByIdAsync(int id);

        Task<CommentResponse> CreateAsync(
            CreateCommentRequest request,
            int userId);

        Task DeleteAsync(int id);
    }
}

