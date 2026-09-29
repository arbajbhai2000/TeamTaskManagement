using Application.Interfaces.Repositories;
using Application.Services;
using Domain.Entities;
using Moq;

namespace Application.Tests.Services
{
    public class NotificationServiceTests
    {
        private readonly Mock<INotificationRepository> _notificationRepositoryMock;
        private readonly NotificationService _notificationService;

        public NotificationServiceTests()
        {
            _notificationRepositoryMock = new Mock<INotificationRepository>();
            _notificationService =
                new NotificationService(_notificationRepositoryMock.Object);
        }

        [Fact]
        public async Task GetByUserIdAsync_ReturnsUserNotifications()
        {
            // Arrange
            var notifications = new List<Notification>
            {
                new Notification
                {
                    Id = 1,
                    Message = "New task assigned",
                    IsRead = false,
                    UserId = 5,
                    CreatedAt = DateTime.UtcNow
                },
                new Notification
                {
                    Id = 2,
                    Message = "Task status updated",
                    IsRead = true,
                    UserId = 5,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _notificationRepositoryMock
                .Setup(x => x.GetByUserIdAsync(5))
                .ReturnsAsync(notifications);

            // Act
            var result = (await _notificationService.GetByUserIdAsync(5))
                .ToList();

            // Assert
            Assert.Equal(2, result.Count);

            Assert.Equal(1, result[0].Id);
            Assert.Equal("New task assigned", result[0].Message);
            Assert.False(result[0].IsRead);
            Assert.Equal(5, result[0].UserId);

            Assert.Equal(2, result[1].Id);
            Assert.Equal("Task status updated", result[1].Message);
            Assert.True(result[1].IsRead);

            _notificationRepositoryMock.Verify(
                x => x.GetByUserIdAsync(5),
                Times.Once);
        }

        [Fact]
        public async Task GetByUserIdAsync_NoNotifications_ReturnsEmptyList()
        {
            // Arrange
            _notificationRepositoryMock
                .Setup(x => x.GetByUserIdAsync(99))
                .ReturnsAsync(new List<Notification>());

            // Act
            var result = (await _notificationService.GetByUserIdAsync(99))
                .ToList();

            // Assert
            Assert.Empty(result);

            _notificationRepositoryMock.Verify(
                x => x.GetByUserIdAsync(99),
                Times.Once);
        }

        [Fact]
        public async Task CreateAsync_CreatesNotificationAndReturnsResponse()
        {
            // Arrange
            var createdNotification = new Notification
            {
                Id = 10,
                Message = "You have been assigned a new task",
                UserId = 5,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _notificationRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Notification>()))
                .ReturnsAsync(createdNotification);

            // Act
            var result = await _notificationService.CreateAsync(
                "You have been assigned a new task",
                5);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.Id);
            Assert.Equal(
                "You have been assigned a new task",
                result.Message);
            Assert.Equal(5, result.UserId);
            Assert.False(result.IsRead);

            _notificationRepositoryMock.Verify(
                x => x.AddAsync(It.Is<Notification>(n =>
                    n.Message == "You have been assigned a new task" &&
                    n.UserId == 5 &&
                    n.IsRead == false)),
                Times.Once);
        }

        [Fact]
        public async Task MarkAsReadAsync_NotificationExists_MarksAsRead()
        {
            // Arrange
            var notification = new Notification
            {
                Id = 10,
                Message = "New task assigned",
                UserId = 5,
                IsRead = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            _notificationRepositoryMock
                .Setup(x => x.GetByIdAsync(10))
                .ReturnsAsync(notification);

            // Act
            await _notificationService.MarkAsReadAsync(10);

            // Assert
            Assert.True(notification.IsRead);
            Assert.NotNull(notification.UpdatedAt);

            _notificationRepositoryMock.Verify(
                x => x.GetByIdAsync(10),
                Times.Once);

            _notificationRepositoryMock.Verify(
                x => x.UpdateAsync(It.Is<Notification>(n =>
                    n.Id == 10 &&
                    n.IsRead)),
                Times.Once);
        }

        [Fact]
        public async Task MarkAsReadAsync_NotificationDoesNotExist_ThrowsKeyNotFoundException()
        {
            // Arrange
            _notificationRepositoryMock
                .Setup(x => x.GetByIdAsync(99))
                .ReturnsAsync((Notification?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _notificationService.MarkAsReadAsync(99));

            Assert.Equal(
                "Notification not found.",
                exception.Message);

            _notificationRepositoryMock.Verify(
                x => x.UpdateAsync(It.IsAny<Notification>()),
                Times.Never);
        }
    }
}