using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs.Comments;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TeamTaskManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly ITaskService _taskService;

        public CommentsController(
            ICommentService commentService,
            ITaskService taskService)
        {
            _commentService = commentService;
            _taskService = taskService;
        }

        [HttpGet("task/{taskId}")]
        [Authorize(Roles = "Admin,Manager,User")]
        public async Task<IActionResult> GetByTaskId(int taskId)
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var role =
                User.FindFirst(ClaimTypes.Role)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var task =
                await _taskService.GetByIdAsync(taskId);

            if (task == null)
                return NotFound(new { message = "Task not found." });

            // User can view comments only for their assigned task
            if (role == "User" && task.AssignedToId != userId)
            {
                return Forbid();
            }

            var comments =
                await _commentService.GetByTaskIdAsync(taskId);

            return Ok(comments);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager,User")]
        public async Task<IActionResult> Create(
    [FromBody] CreateCommentRequest request)
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var role =
                User.FindFirst(ClaimTypes.Role)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var task =
                await _taskService.GetByIdAsync(request.TaskItemId);

            if (task == null)
                return NotFound(new { message = "Task not found." });

            // User can comment only on their assigned task
            if (role == "User" && task.AssignedToId != userId)
            {
                return Forbid();
            }

            var comment =
                await _commentService.CreateAsync(request, userId);

            return Ok(comment);
        }



        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager,User")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userIdClaim =
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var role =
                    User.FindFirst(ClaimTypes.Role)?.Value;

                if (!int.TryParse(userIdClaim, out var userId))
                    return Unauthorized();

                var comment =
                    await _commentService.GetByIdAsync(id);

                if (comment == null)
                    return NotFound(new { message = "Comment not found." });

                // Admin and Manager can delete any comment
                if (role == "Admin" || role == "Manager")
                {
                    await _commentService.DeleteAsync(id);
                    return NoContent();
                }

                // User can delete only their own comment
                if (comment.UserId != userId)
                {
                    return Forbid();
                }

                await _commentService.DeleteAsync(id);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }




    }
}
