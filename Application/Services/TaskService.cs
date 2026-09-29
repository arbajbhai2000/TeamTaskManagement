using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.Tasks;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;

namespace Application.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly INotificationService _notificationService;
        private readonly ITeamMemberRepository _teamMemberRepository;

        public TaskService(
            ITaskRepository taskRepository,
            INotificationService notificationService,
            ITeamMemberRepository teamMemberRepository)
        {
            _taskRepository = taskRepository;
            _notificationService = notificationService;
            _teamMemberRepository = teamMemberRepository;
        }

        public async Task<TaskResponse?> GetByIdAsync(int id)
        {
            var task = await _taskRepository.GetByIdAsync(id);

            if (task == null)
                return null;

            return MapToResponse(task);
        }

        public async Task<IEnumerable<TaskResponse>> GetAllAsync()
        {
            var tasks = await _taskRepository.GetAllAsync();

            return tasks.Select(MapToResponse);
        }


        public async Task<IEnumerable<TaskResponse>> GetByAssignedUserIdAsync(
            int userId)
        {
            var tasks =
                await _taskRepository.GetByAssignedUserIdAsync(userId);

            return tasks.Select(MapToResponse);
        }


        public async Task<TaskResponse> CreateAsync(
            CreateTaskRequest request)
        {
            var isTeamMember =
          await _teamMemberRepository.ExistsAsync(
          request.TeamId,
          request.AssignedToId);

            if (!isTeamMember)
            {
                throw new InvalidOperationException(
                    "Assigned user is not a member of the selected team.");
            }

            var task = new TaskItem
            {
                Title = request.Title,
                Description = request.Description,
                Priority = request.Priority,
                DueDate = request.DueDate,
                AssignedToId = request.AssignedToId,
                TeamId = request.TeamId,
                CreatedAt = DateTime.UtcNow
            };

            var createdTask = await _taskRepository.AddAsync(task);

            await _notificationService.CreateAsync(
                $"You have been assigned a new task: {createdTask.Title}",
                createdTask.AssignedToId);

            return MapToResponse(createdTask);
        }

        public async Task UpdateAsync(
            int id,
            UpdateTaskRequest request)
        {
            var task = await _taskRepository.GetByIdAsync(id);

            if (task == null)
                throw new KeyNotFoundException("Task not found.");

            var oldStatus = task.Status;

            var isTeamMember =
    await _teamMemberRepository.ExistsAsync(
        request.TeamId,
        request.AssignedToId);

            if (!isTeamMember)
            {
                throw new InvalidOperationException(
                    "Assigned user is not a member of the selected team.");
            }

            task.Title = request.Title;
            task.Description = request.Description;
            task.Status = request.Status;
            task.Priority = request.Priority;
            task.DueDate = request.DueDate;
            task.AssignedToId = request.AssignedToId;
            task.TeamId = request.TeamId;
            task.UpdatedAt = DateTime.UtcNow;

            await _taskRepository.UpdateAsync(task);

            if (oldStatus != task.Status)
            {
                await _notificationService.CreateAsync(
                    $"Task '{task.Title}' status changed to {task.Status}.",
                    task.AssignedToId);
            }
        }

        public async Task DeleteAsync(int id)
        {
            var task = await _taskRepository.GetByIdAsync(id);

            if (task == null)
                throw new KeyNotFoundException("Task not found.");

            await _taskRepository.DeleteAsync(task);
        }

        private static TaskResponse MapToResponse(TaskItem task)
        {
            return new TaskResponse
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                DueDate = task.DueDate,
                AssignedToId = task.AssignedToId,
                AssignedToName = task.AssignedTo?.Name ?? string.Empty,
                TeamId = task.TeamId,
                TeamName = task.Team?.Name ?? string.Empty,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt
            };
        }
    }
}