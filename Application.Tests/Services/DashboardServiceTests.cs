using Application.Interfaces.Repositories;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Moq;

namespace Application.Tests.Services
{
    public class DashboardServiceTests
    {
        private readonly Mock<ITaskRepository> _taskRepositoryMock;
        private readonly DashboardService _dashboardService;

        public DashboardServiceTests()
        {
            _taskRepositoryMock = new Mock<ITaskRepository>();

            _dashboardService = new DashboardService(
                _taskRepositoryMock.Object);
        }

        [Fact]
        public async Task GetDashboardAsync_ReturnsCorrectTaskCounts()
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
                    AssignedToId = 5,
                    TeamId = 1
                },
                new TaskItem
                {
                    Id = 2,
                    Title = "Task Two",
                    Status = TaskItemStatus.InProgress,
                    Priority = TaskPriority.Medium,
                    AssignedToId = 5,
                    TeamId = 1
                },
                new TaskItem
                {
                    Id = 3,
                    Title = "Task Three",
                    Status = TaskItemStatus.Done,
                    Priority = TaskPriority.Low,
                    AssignedToId = 5,
                    TeamId = 1
                },
                new TaskItem
                {
                    Id = 4,
                    Title = "Task Four",
                    Status = TaskItemStatus.ToDo,
                    Priority = TaskPriority.High,
                    AssignedToId = 5,
                    TeamId = 1
                }
            };

            _taskRepositoryMock
                .Setup(x => x.GetByAssignedUserIdAsync(5))
                .ReturnsAsync(tasks);

            // Act
            var result = await _dashboardService.GetDashboardAsync(
                5,
                "User");

            // Assert
            Assert.Equal(4, result.TotalTasks);
            Assert.Equal(2, result.ToDoTasks);
            Assert.Equal(1, result.InProgressTasks);
            Assert.Equal(1, result.CompletedTasks);

            Assert.Equal(2, result.HighPriorityTasks);
            Assert.Equal(1, result.MediumPriorityTasks);
            Assert.Equal(1, result.LowPriorityTasks);

            Assert.Equal(4, result.Tasks.Count);

            _taskRepositoryMock.Verify(
                x => x.GetByAssignedUserIdAsync(5),
                Times.Once);
        }

        [Fact]
        public async Task GetDashboardAsync_StatusFilter_ReturnsOnlyMatchingTasks()
        {
            // Arrange
            var tasks = new List<TaskItem>
            {
                new TaskItem
                {
                    Id = 1,
                    Title = "To Do Task",
                    Status = TaskItemStatus.ToDo,
                    Priority = TaskPriority.High,
                    AssignedToId = 5
                },
                new TaskItem
                {
                    Id = 2,
                    Title = "Done Task",
                    Status = TaskItemStatus.Done,
                    Priority = TaskPriority.Medium,
                    AssignedToId = 5
                },
                new TaskItem
                {
                    Id = 3,
                    Title = "Another To Do",
                    Status = TaskItemStatus.ToDo,
                    Priority = TaskPriority.Low,
                    AssignedToId = 5
                }
            };

            _taskRepositoryMock
                .Setup(x => x.GetByAssignedUserIdAsync(5))
                .ReturnsAsync(tasks);

            // Act
            var result = await _dashboardService.GetDashboardAsync(
                5,
                "User",
                status: "ToDo");

            // Assert
            Assert.Equal(2, result.TotalTasks);
            Assert.Equal(2, result.ToDoTasks);
            Assert.Equal(0, result.InProgressTasks);
            Assert.Equal(0, result.CompletedTasks);

            Assert.All(
                result.Tasks,
                task => Assert.Equal(
                    TaskItemStatus.ToDo,
                    task.Status));
        }

        [Fact]
        public async Task GetDashboardAsync_PriorityFilter_ReturnsOnlyMatchingTasks()
        {
            // Arrange
            var tasks = new List<TaskItem>
            {
                new TaskItem
                {
                    Id = 1,
                    Title = "High Task",
                    Status = TaskItemStatus.ToDo,
                    Priority = TaskPriority.High,
                    AssignedToId = 5
                },
                new TaskItem
                {
                    Id = 2,
                    Title = "Medium Task",
                    Status = TaskItemStatus.InProgress,
                    Priority = TaskPriority.Medium,
                    AssignedToId = 5
                },
                new TaskItem
                {
                    Id = 3,
                    Title = "Another High Task",
                    Status = TaskItemStatus.Done,
                    Priority = TaskPriority.High,
                    AssignedToId = 5
                }
            };

            _taskRepositoryMock
                .Setup(x => x.GetByAssignedUserIdAsync(5))
                .ReturnsAsync(tasks);

            // Act
            var result = await _dashboardService.GetDashboardAsync(
                5,
                "User",
                priority: "High");

            // Assert
            Assert.Equal(2, result.TotalTasks);
            Assert.Equal(2, result.HighPriorityTasks);
            Assert.Equal(0, result.MediumPriorityTasks);
            Assert.Equal(0, result.LowPriorityTasks);

            Assert.All(
                result.Tasks,
                task => Assert.Equal(
                    TaskPriority.High,
                    task.Priority));
        }

        [Fact]
        public async Task GetDashboardAsync_DeadlineFilter_ReturnsTasksBeforeOrOnDeadline()
        {
            // Arrange
            var deadline = DateTime.UtcNow.Date.AddDays(5);

            var tasks = new List<TaskItem>
            {
                new TaskItem
                {
                    Id = 1,
                    Title = "Before Deadline",
                    Status = TaskItemStatus.ToDo,
                    Priority = TaskPriority.High,
                    DueDate = DateTime.UtcNow.Date.AddDays(2),
                    AssignedToId = 5
                },
                new TaskItem
                {
                    Id = 2,
                    Title = "On Deadline",
                    Status = TaskItemStatus.InProgress,
                    Priority = TaskPriority.Medium,
                    DueDate = deadline,
                    AssignedToId = 5
                },
                new TaskItem
                {
                    Id = 3,
                    Title = "After Deadline",
                    Status = TaskItemStatus.ToDo,
                    Priority = TaskPriority.Low,
                    DueDate = DateTime.UtcNow.Date.AddDays(10),
                    AssignedToId = 5
                }
            };

            _taskRepositoryMock
                .Setup(x => x.GetByAssignedUserIdAsync(5))
                .ReturnsAsync(tasks);

            // Act
            var result = await _dashboardService.GetDashboardAsync(
                5,
                "User",
                deadline: deadline);

            // Assert
            Assert.Equal(2, result.TotalTasks);

            Assert.Contains(
                result.Tasks,
                x => x.Title == "Before Deadline");

            Assert.Contains(
                result.Tasks,
                x => x.Title == "On Deadline");

            Assert.DoesNotContain(
                result.Tasks,
                x => x.Title == "After Deadline");
        }

        [Fact]
        public async Task GetDashboardAsync_OverdueTasks_CountsIncompleteOverdueTasks()
        {
            // Arrange
            var tasks = new List<TaskItem>
            {
                new TaskItem
                {
                    Id = 1,
                    Title = "Overdue ToDo",
                    Status = TaskItemStatus.ToDo,
                    Priority = TaskPriority.High,
                    DueDate = DateTime.UtcNow.Date.AddDays(-2),
                    AssignedToId = 5
                },
                new TaskItem
                {
                    Id = 2,
                    Title = "Overdue InProgress",
                    Status = TaskItemStatus.InProgress,
                    Priority = TaskPriority.Medium,
                    DueDate = DateTime.UtcNow.Date.AddDays(-1),
                    AssignedToId = 5
                },
                new TaskItem
                {
                    Id = 3,
                    Title = "Overdue Done",
                    Status = TaskItemStatus.Done,
                    Priority = TaskPriority.High,
                    DueDate = DateTime.UtcNow.Date.AddDays(-3),
                    AssignedToId = 5
                },
                new TaskItem
                {
                    Id = 4,
                    Title = "Future Task",
                    Status = TaskItemStatus.ToDo,
                    Priority = TaskPriority.Low,
                    DueDate = DateTime.UtcNow.Date.AddDays(5),
                    AssignedToId = 5
                }
            };

            _taskRepositoryMock
                .Setup(x => x.GetByAssignedUserIdAsync(5))
                .ReturnsAsync(tasks);

            // Act
            var result = await _dashboardService.GetDashboardAsync(
                5,
                "User");

            // Assert
            Assert.Equal(2, result.OverdueTasks);
        }

        [Fact]
        public async Task GetDashboardAsync_NoTasks_ReturnsZeroCounts()
        {
            // Arrange
            _taskRepositoryMock
                .Setup(x => x.GetByAssignedUserIdAsync(99))
                .ReturnsAsync(new List<TaskItem>());

            // Act
            var result = await _dashboardService.GetDashboardAsync(
                99,
                "User");

            // Assert
            Assert.Equal(0, result.TotalTasks);
            Assert.Equal(0, result.ToDoTasks);
            Assert.Equal(0, result.InProgressTasks);
            Assert.Equal(0, result.CompletedTasks);
            Assert.Equal(0, result.HighPriorityTasks);
            Assert.Equal(0, result.MediumPriorityTasks);
            Assert.Equal(0, result.LowPriorityTasks);
            Assert.Equal(0, result.OverdueTasks);
            Assert.Empty(result.Tasks);

            _taskRepositoryMock.Verify(
                x => x.GetByAssignedUserIdAsync(99),
                Times.Once);
        }

        [Fact]
        public async Task GetDashboardAsync_InvalidStatusFilter_DoesNotFilterTasks()
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
                    AssignedToId = 5
                },
                new TaskItem
                {
                    Id = 2,
                    Title = "Task Two",
                    Status = TaskItemStatus.Done,
                    Priority = TaskPriority.Medium,
                    AssignedToId = 5
                }
            };

            _taskRepositoryMock
                .Setup(x => x.GetByAssignedUserIdAsync(5))
                .ReturnsAsync(tasks);

            // Act
            var result = await _dashboardService.GetDashboardAsync(
                5,
                "User",
                status: "InvalidStatus");

            // Assert
            Assert.Equal(2, result.TotalTasks);
            Assert.Equal(1, result.ToDoTasks);
            Assert.Equal(1, result.CompletedTasks);
        }
    }
}
