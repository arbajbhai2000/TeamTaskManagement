using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Comments;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;

namespace Application.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;

        public CommentService(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }

        public async Task<IEnumerable<CommentResponse>> GetByTaskIdAsync(int taskId)
        {
            var comments = await _commentRepository.GetByTaskIdAsync(taskId);

            return comments.Select(MapToResponse);
        }


        public async Task<CommentResponse?> GetByIdAsync(int id)
        {
            var comment = await _commentRepository.GetByIdAsync(id);

            if (comment == null)
                return null;

            return MapToResponse(comment);
        }


        public async Task<CommentResponse> CreateAsync(
            CreateCommentRequest request,
            int userId)
        {
            var comment = new Comment
            {
                Content = request.Content,
                TaskItemId = request.TaskItemId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            var createdComment = await _commentRepository.AddAsync(comment);

            return MapToResponse(createdComment);
        }

        public async Task DeleteAsync(int id)
        {
            var comment = await _commentRepository.GetByIdAsync(id);

            if (comment == null)
                throw new KeyNotFoundException("Comment not found.");

            await _commentRepository.DeleteAsync(comment);
        }

        private static CommentResponse MapToResponse(Comment comment)
        {
            return new CommentResponse
            {
                Id = comment.Id,
                Content = comment.Content,
                TaskItemId = comment.TaskItemId,
                UserId = comment.UserId,
                UserName = comment.User?.Name ?? string.Empty,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt
            };
        }
    }
}
