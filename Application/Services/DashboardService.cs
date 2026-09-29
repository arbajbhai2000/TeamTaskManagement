using Application.DTOs.Dashboard;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;

namespace Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ITaskRepository _taskRepository;

        public DashboardService(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public async Task<DashboardResponse> GetDashboardAsync(
            int userId,
            string role,
            string? status = null,
            string? priority = null,
            DateTime? deadline = null)
        {
            List<Domain.Entities.TaskItem> tasks;

            // Admin sees workspace-wide dashboard statistics.
            if (role == "Admin")
            {
                tasks = (await _taskRepository.GetAllAsync()).ToList();
            }
            else
            {
                // Manager and User see their assigned tasks.
                tasks = (
                    await _taskRepository.GetByAssignedUserIdAsync(userId)
                ).ToList();
            }

            // Status filter
            if (!string.IsNullOrWhiteSpace(status) &&
                Enum.TryParse<TaskItemStatus>(
                    status,
                    true,
                    out var statusValue))
            {
                tasks = tasks
                    .Where(x => x.Status == statusValue)
                    .ToList();
            }

            // Priority filter
            if (!string.IsNullOrWhiteSpace(priority) &&
                Enum.TryParse<TaskPriority>(
                    priority,
                    true,
                    out var priorityValue))
            {
                tasks = tasks
                    .Where(x => x.Priority == priorityValue)
                    .ToList();
            }

            // Deadline filter
            if (deadline.HasValue)
            {
                tasks = tasks
                    .Where(x =>
                        x.DueDate.HasValue &&
                        x.DueDate.Value.Date <= deadline.Value.Date)
                    .ToList();
            }

            var today = DateTime.UtcNow.Date;

            return new DashboardResponse
            {
                TotalTasks = tasks.Count,

                ToDoTasks = tasks.Count(
                    x => x.Status == TaskItemStatus.ToDo),

                InProgressTasks = tasks.Count(
                    x => x.Status == TaskItemStatus.InProgress),

                CompletedTasks = tasks.Count(
                    x => x.Status == TaskItemStatus.Done),

                HighPriorityTasks = tasks.Count(
                    x => x.Priority == TaskPriority.High),

                MediumPriorityTasks = tasks.Count(
                    x => x.Priority == TaskPriority.Medium),

                LowPriorityTasks = tasks.Count(
                    x => x.Priority == TaskPriority.Low),

                OverdueTasks = tasks.Count(
                    x =>
                        x.DueDate.HasValue &&
                        x.DueDate.Value.Date < today &&
                        x.Status != TaskItemStatus.Done),

                Tasks = tasks.Select(x => new DashboardTaskResponse
                {
                    Id = x.Id,
                    Title = x.Title,
                    Status = x.Status,
                    Priority = x.Priority,
                    DueDate = x.DueDate,
                    AssignedToId = x.AssignedToId,
                    AssignedToName =
                        x.AssignedTo?.Name ?? string.Empty,
                    TeamId = x.TeamId,
                    TeamName =
                        x.Team?.Name ?? string.Empty
                }).ToList()
            };
        }
    }
}