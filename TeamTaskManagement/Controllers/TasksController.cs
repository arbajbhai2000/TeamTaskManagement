using Application.DTOs.Tasks;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TeamTaskManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        // GET: api/Tasks
        //[HttpGet]
        //[Authorize(Roles = "Admin,Manager,User")]
        //public async Task<IActionResult> GetAll()
        //{
        //    var tasks = await _taskService.GetAllAsync();

        //    return Ok(tasks);
        //}


        [HttpGet]
        [Authorize(Roles = "Admin,Manager,User")]
        public async Task<IActionResult> GetAll()
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var role =
                User.FindFirst(ClaimTypes.Role)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            // User can see only their assigned tasks
            if (role == "User")
            {
                var userTasks =
                    await _taskService.GetByAssignedUserIdAsync(userId);

                return Ok(userTasks);
            }

            // Admin and Manager can see all tasks
            var tasks =
                await _taskService.GetAllAsync();

            return Ok(tasks);
        }


        // GET: api/Tasks/{id}

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager,User")]
        public async Task<IActionResult> GetById(int id)
        {
            var task = await _taskService.GetByIdAsync(id);

            if (task == null)
                return NotFound(new { message = "Task not found." });

            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var role =
                User.FindFirst(ClaimTypes.Role)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            // User can view only their own assigned task
            if (role == "User" && task.AssignedToId != userId)
            {
                return Forbid();
            }

            return Ok(task);
        }



        // POST: api/Tasks
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create(
     [FromBody] CreateTaskRequest request)
        {
            try
            {
                var task = await _taskService.CreateAsync(request);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = task.Id },
                    task);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/Tasks/{id}

        //[HttpPut("{id}")]
        //[Authorize(Roles = "Admin,Manager,User")]
        //public async Task<IActionResult> Update(
        //    int id,
        //    [FromBody] UpdateTaskRequest request)
        //{
        //    try
        //    {
        //        await _taskService.UpdateAsync(id, request);

        //        var updatedTask =
        //            await _taskService.GetByIdAsync(id);

        //        return Ok(updatedTask);
        //    }
        //    catch (KeyNotFoundException ex)
        //    {
        //        return NotFound(new { message = ex.Message });
        //    }
        //}


        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager,User")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateTaskRequest request)
        {
            try
            {
                var task = await _taskService.GetByIdAsync(id);

                if (task == null)
                    return NotFound(new { message = "Task not found." });

                var userIdClaim =
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var role =
                    User.FindFirst(ClaimTypes.Role)?.Value;

                if (!int.TryParse(userIdClaim, out var userId))
                    return Unauthorized();

                // User can update only their own assigned task
                if (role == "User" && task.AssignedToId != userId)
                {
                    return Forbid();
                }

                await _taskService.UpdateAsync(id, request);

                var updatedTask =
                    await _taskService.GetByIdAsync(id);

                return Ok(updatedTask);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        // DELETE: api/Tasks/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _taskService.DeleteAsync(id);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}