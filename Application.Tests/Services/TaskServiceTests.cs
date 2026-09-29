using Application.DTOs.Tasks;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Moq;

namespace Application.Tests.Services
{
    public class TaskServiceTests
    {
        private readonly Mock<ITaskRepository> _taskRepositoryMock;
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly Mock<ITeamMemberRepository> _teamMemberRepositoryMock;
        private readonly TaskService _taskService;

        public TaskServiceTests()
        {
            _taskRepositoryMock = new Mock<ITaskRepository>();
            _notificationServiceMock = new Mock<INotificationService>();
            _teamMemberRepositoryMock = new Mock<ITeamMemberRepository>();

            _taskService = new TaskService(
                _taskRepositoryMock.Object,
                _notificationServiceMock.Object,
                _teamMemberRepositoryMock.Object);
        }

        [Fact]
        public async Task GetByIdAsync_TaskExists_ReturnsTaskResponse()
        {
            // Arrange
            var task = new TaskItem
            {
                Id = 1,
                Title = "Complete API",
                Description = "Complete task API",
                Status = TaskItemStatus.ToDo,
                Priority = TaskPriority.High,
                AssignedToId = 2,
                AssignedTo = new User
                {
                    Id = 2,
                    Name = "John"
                },
                TeamId = 5,
                Team = new Team
                {
                    Id = 5,
                    Name = "Development"
                },
                CreatedAt = DateTime.UtcNow
            };

            _taskRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(task);

            // Act
            var result = await _taskService.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Complete API", result.Title);
            Assert.Equal(TaskItemStatus.ToDo, result.Status);
            Assert.Equal(TaskPriority.High, result.Priority);
            Assert.Equal(2, result.AssignedToId);
            Assert.Equal("John", result.AssignedToName);
            Assert.Equal(5, result.TeamId);
            Assert.Equal("Development", result.TeamName);

            _taskRepositoryMock.Verify(
                x => x.GetByIdAsync(1),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_TaskDoesNotExist_ReturnsNull()
        {
            // Arrange
            _taskRepositoryMock
                .Setup(x => x.GetByIdAsync(99))
                .ReturnsAsync((TaskItem?)null);

            // Act
            var result = await _taskService.GetByIdAsync(99);

            // Assert
            Assert.Null(result);

            _taskRepositoryMock.Verify(
                x => x.GetByIdAsync(99),
                Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllTasks()
        {
            // Arrange
            var tasks = new List<TaskItem>
            {
                new TaskItem
                {
                    Id = 1,
                    Title = "Task One",
                    Status = TaskItemStatus.ToDo,
                    Priority = TaskPriority.High,
                    AssignedToId = 2,
                    AssignedTo = new User
                    {
                        Id = 2,
                        Name = "John"
                    },
                    TeamId = 5,
                    Team = new Team
                    {
                        Id = 5,
                        Name = "Development"
                    }
                },
                new TaskItem
                {
                    Id = 2,
                    Title = "Task Two",
                    Status = TaskItemStatus.Done,
                    Priority = TaskPriority.Medium,
                    AssignedToId = 3,
                    AssignedTo = new User
                    {
                        Id = 3,
                        Name = "Sarah"
                    },
                    TeamId = 5,
                    Team = new Team
                    {
                        Id = 5,
                        Name = "Development"
                    }
                }
            };

            _taskRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(tasks);

            // Act
            var result = (await _taskService.GetAllAsync()).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("Task One", result[0].Title);
            Assert.Equal("Task Two", result[1].Title);
            Assert.Equal(TaskItemStatus.Done, result[1].Status);

            _taskRepositoryMock.Verify(
                x => x.GetAllAsync(),
                Times.Once);
        }

        [Fact]
        public async Task GetByAssignedUserIdAsync_ReturnsAssignedTasks()
        {
            // Arrange
            var tasks = new List<TaskItem>
            {
                new TaskItem
                {
                    Id = 1,
                    Title = "Assigned Task",
                    AssignedToId = 2,
                    TeamId = 5,
                    Status = TaskItemStatus.InProgress,
                    Priority = TaskPriority.Medium
                }
            };

            _taskRepositoryMock
                .Setup(x => x.GetByAssignedUserIdAsync(2))
                .ReturnsAsync(tasks);

            // Act
            var result = (await _taskService.GetByAssignedUserIdAsync(2))
                .ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result[0].Id);
            Assert.Equal("Assigned Task", result[0].Title);
            Assert.Equal(2, result[0].AssignedToId);
            Assert.Equal(TaskItemStatus.InProgress, result[0].Status);

            _taskRepositoryMock.Verify(
                x => x.GetByAssignedUserIdAsync(2),
                Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ValidRequest_CreatesTaskAndNotification()
        {
            // Arrange
            var request = new CreateTaskRequest
            {
                Title = "Create Dashboard",
                Description = "Create task dashboard",
                Priority = TaskPriority.High,
                DueDate = DateTime.UtcNow.AddDays(5),
                AssignedToId = 2,
                TeamId = 5
            };

            var createdTask = new TaskItem
            {
                Id = 10,
                Title = "Create Dashboard",
                Description = "Create task dashboard",
                Priority = TaskPriority.High,
                DueDate = request.DueDate,
                AssignedToId = 2,
                TeamId = 5,
                Status = TaskItemStatus.ToDo,
                CreatedAt = DateTime.UtcNow
            };

            _teamMemberRepositoryMock
                .Setup(x => x.ExistsAsync(5, 2))
                .ReturnsAsync(true);

            _taskRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<TaskItem>()))
                .ReturnsAsync(createdTask);

            _notificationServiceMock
                .Setup(x => x.CreateAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>()))
                .ReturnsAsync(new Application.DTOs.Notifications.NotificationResponse());

            // Act
            var result = await _taskService.CreateAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.Id);
            Assert.Equal("Create Dashboard", result.Title);
            Assert.Equal(TaskPriority.High, result.Priority);
            Assert.Equal(2, result.AssignedToId);
            Assert.Equal(5, result.TeamId);

            _teamMemberRepositoryMock.Verify(
                x => x.ExistsAsync(5, 2),
                Times.Once);

            _taskRepositoryMock.Verify(
                x => x.AddAsync(It.Is<TaskItem>(t =>
                    t.Title == "Create Dashboard" &&
                    t.AssignedToId == 2 &&
                    t.TeamId == 5 &&
                    t.Priority == TaskPriority.High)),
                Times.Once);

            _notificationServiceMock.Verify(
                x => x.CreateAsync(
                    "You have been assigned a new task: Create Dashboard",
                    2),
                Times.Once);
        }

        [Fact]
        public async Task CreateAsync_AssignedUserNotTeamMember_ThrowsInvalidOperationException()
        {
            // Arrange
            var request = new CreateTaskRequest
            {
                Title = "Invalid Task",
                Description = "Invalid assignment",
                Priority = TaskPriority.Medium,
                AssignedToId = 2,
                TeamId = 5
            };

            _teamMemberRepositoryMock
                .Setup(x => x.ExistsAsync(5, 2))
                .ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _taskService.CreateAsync(request));

            Assert.Equal(
                "Assigned user is not a member of the selected team.",
                exception.Message);

            _taskRepositoryMock.Verify(
                x => x.AddAsync(It.IsAny<TaskItem>()),
                Times.Never);

            _notificationServiceMock.Verify(
                x => x.CreateAsync(It.IsAny<string>(), It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_TaskExists_UpdatesTask()
        {
            // Arrange
            var task = new TaskItem
            {
                Id = 10,
                Title = "Old Title",
                Description = "Old Description",
                Status = TaskItemStatus.ToDo,
                Priority = TaskPriority.Low,
                AssignedToId = 2,
                TeamId = 5,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var request = new UpdateTaskRequest
            {
                Title = "Updated Title",
                Description = "Updated Description",
                Status = TaskItemStatus.ToDo,
                Priority = TaskPriority.High,
                DueDate = DateTime.UtcNow.AddDays(7),
                AssignedToId = 2,
                TeamId = 5
            };

            _taskRepositoryMock
                .Setup(x => x.GetByIdAsync(10))
                .ReturnsAsync(task);

            _teamMemberRepositoryMock
                .Setup(x => x.ExistsAsync(5, 2))
                .ReturnsAsync(true);

            // Act
            await _taskService.UpdateAsync(10, request);

            // Assert
            Assert.Equal("Updated Title", task.Title);
            Assert.Equal("Updated Description", task.Description);
            Assert.Equal(TaskPriority.High, task.Priority);
            Assert.Equal(TaskItemStatus.ToDo, task.Status);
            Assert.Equal(2, task.AssignedToId);
            Assert.Equal(5, task.TeamId);
            Assert.NotNull(task.UpdatedAt);

            _taskRepositoryMock.Verify(
                x => x.UpdateAsync(task),
                Times.Once);

            _notificationServiceMock.Verify(
                x => x.CreateAsync(It.IsAny<string>(), It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_StatusChanged_CreatesNotification()
        {
            // Arrange
            var task = new TaskItem
            {
                Id = 10,
                Title = "Complete API",
                Description = "API work",
                Status = TaskItemStatus.ToDo,
                Priority = TaskPriority.High,
                AssignedToId = 2,
                TeamId = 5,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var request = new UpdateTaskRequest
            {
                Title = "Complete API",
                Description = "API work",
                Status = TaskItemStatus.Done,
                Priority = TaskPriority.High,
                AssignedToId = 2,
                TeamId = 5
            };

            _taskRepositoryMock
                .Setup(x => x.GetByIdAsync(10))
                .ReturnsAsync(task);

            _teamMemberRepositoryMock
                .Setup(x => x.ExistsAsync(5, 2))
                .ReturnsAsync(true);

            // Act
            await _taskService.UpdateAsync(10, request);

            // Assert
            Assert.Equal(TaskItemStatus.Done, task.Status);

            _taskRepositoryMock.Verify(
                x => x.UpdateAsync(task),
                Times.Once);

            _notificationServiceMock.Verify(
                x => x.CreateAsync(
                    "Task 'Complete API' status changed to Done.",
                    2),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_TaskDoesNotExist_ThrowsKeyNotFoundException()
        {
            // Arrange
            var request = new UpdateTaskRequest
            {
                Title = "Updated",
                Description = "Updated",
                Status = TaskItemStatus.Done,
                Priority = TaskPriority.High,
                AssignedToId = 2,
                TeamId = 5
            };

            _taskRepositoryMock
                .Setup(x => x.GetByIdAsync(99))
                .ReturnsAsync((TaskItem?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _taskService.UpdateAsync(99, request));

            Assert.Equal("Task not found.", exception.Message);

            _taskRepositoryMock.Verify(
                x => x.UpdateAsync(It.IsAny<TaskItem>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_AssignedUserNotTeamMember_ThrowsInvalidOperationException()
        {
            // Arrange
            var task = new TaskItem
            {
                Id = 10,
                Title = "Task",
                AssignedToId = 2,
                TeamId = 5,
                Status = TaskItemStatus.ToDo
            };

            var request = new UpdateTaskRequest
            {
                Title = "Updated Task",
                Description = "Updated",
                Status = TaskItemStatus.InProgress,
                Priority = TaskPriority.Medium,
                AssignedToId = 3,
                TeamId = 6
            };

            _taskRepositoryMock
                .Setup(x => x.GetByIdAsync(10))
                .ReturnsAsync(task);

            _teamMemberRepositoryMock
                .Setup(x => x.ExistsAsync(6, 3))
                .ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _taskService.UpdateAsync(10, request));

            Assert.Equal(
                "Assigned user is not a member of the selected team.",
                exception.Message);

            _taskRepositoryMock.Verify(
                x => x.UpdateAsync(It.IsAny<TaskItem>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_TaskExists_DeletesTask()
        {
            // Arrange
            var task = new TaskItem
            {
                Id = 10,
                Title = "Delete Task",
                AssignedToId = 2,
                TeamId = 5
            };

            _taskRepositoryMock
                .Setup(x => x.GetByIdAsync(10))
                .ReturnsAsync(task);

            // Act
            await _taskService.DeleteAsync(10);

            // Assert
            _taskRepositoryMock.Verify(
                x => x.GetByIdAsync(10),
                Times.Once);

            _taskRepositoryMock.Verify(
                x => x.DeleteAsync(task),
                Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_TaskDoesNotExist_ThrowsKeyNotFoundException()
        {
            // Arrange
            _taskRepositoryMock
                .Setup(x => x.GetByIdAsync(99))
                .ReturnsAsync((TaskItem?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _taskService.DeleteAsync(99));

            Assert.Equal(
                "Task not found.",
                exception.Message);

            _taskRepositoryMock.Verify(
                x => x.DeleteAsync(It.IsAny<TaskItem>()),
                Times.Never);
        }
    }
}