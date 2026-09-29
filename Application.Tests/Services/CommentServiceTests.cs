using Application.DTOs.Comments;
using Application.Interfaces.Repositories;
using Application.Services;
using Domain.Entities;
using Moq;

namespace Application.Tests.Services
{
    public class CommentServiceTests
    {
        private readonly Mock<ICommentRepository> _commentRepositoryMock;
        private readonly CommentService _commentService;

        public CommentServiceTests()
        {
            _commentRepositoryMock = new Mock<ICommentRepository>();
            _commentService = new CommentService(
                _commentRepositoryMock.Object);
        }

        [Fact]
        public async Task GetByTaskIdAsync_ReturnsComments()
        {
            // Arrange
            var comments = new List<Comment>
            {
                new Comment
                {
                    Id = 1,
                    Content = "First comment",
                    TaskItemId = 10,
                    UserId = 2,
                    User = new User
                    {
                        Id = 2,
                        Name = "John"
                    },
                    CreatedAt = DateTime.UtcNow
                },
                new Comment
                {
                    Id = 2,
                    Content = "Second comment",
                    TaskItemId = 10,
                    UserId = 3,
                    User = new User
                    {
                        Id = 3,
                        Name = "Sarah"
                    },
                    CreatedAt = DateTime.UtcNow
                }
            };

            _commentRepositoryMock
                .Setup(x => x.GetByTaskIdAsync(10))
                .ReturnsAsync(comments);

            // Act
            var result = (await _commentService.GetByTaskIdAsync(10))
                .ToList();

            // Assert
            Assert.Equal(2, result.Count);

            Assert.Equal(1, result[0].Id);
            Assert.Equal("First comment", result[0].Content);
            Assert.Equal(10, result[0].TaskItemId);
            Assert.Equal(2, result[0].UserId);
            Assert.Equal("John", result[0].UserName);

            Assert.Equal(2, result[1].Id);
            Assert.Equal("Second comment", result[1].Content);
            Assert.Equal("Sarah", result[1].UserName);

            _commentRepositoryMock.Verify(
                x => x.GetByTaskIdAsync(10),
                Times.Once);
        }

        [Fact]
        public async Task GetByTaskIdAsync_NoComments_ReturnsEmptyList()
        {
            // Arrange
            _commentRepositoryMock
                .Setup(x => x.GetByTaskIdAsync(99))
                .ReturnsAsync(new List<Comment>());

            // Act
            var result = (await _commentService.GetByTaskIdAsync(99))
                .ToList();

            // Assert
            Assert.Empty(result);

            _commentRepositoryMock.Verify(
                x => x.GetByTaskIdAsync(99),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_CommentExists_ReturnsCommentResponse()
        {
            // Arrange
            var comment = new Comment
            {
                Id = 1,
                Content = "Good work",
                TaskItemId = 10,
                UserId = 2,
                User = new User
                {
                    Id = 2,
                    Name = "John"
                },
                CreatedAt = DateTime.UtcNow
            };

            _commentRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(comment);

            // Act
            var result = await _commentService.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Good work", result.Content);
            Assert.Equal(10, result.TaskItemId);
            Assert.Equal(2, result.UserId);
            Assert.Equal("John", result.UserName);

            _commentRepositoryMock.Verify(
                x => x.GetByIdAsync(1),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_CommentDoesNotExist_ReturnsNull()
        {
            // Arrange
            _commentRepositoryMock
                .Setup(x => x.GetByIdAsync(99))
                .ReturnsAsync((Comment?)null);

            // Act
            var result = await _commentService.GetByIdAsync(99);

            // Assert
            Assert.Null(result);

            _commentRepositoryMock.Verify(
                x => x.GetByIdAsync(99),
                Times.Once);
        }

        [Fact]
        public async Task CreateAsync_CreatesCommentAndReturnsResponse()
        {
            // Arrange
            var request = new CreateCommentRequest
            {
                Content = "This task is completed.",
                TaskItemId = 10
            };

            var createdComment = new Comment
            {
                Id = 20,
                Content = "This task is completed.",
                TaskItemId = 10,
                UserId = 2,
                CreatedAt = DateTime.UtcNow
            };

            _commentRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Comment>()))
                .ReturnsAsync(createdComment);

            // Act
            var result = await _commentService.CreateAsync(request, 2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(20, result.Id);
            Assert.Equal("This task is completed.", result.Content);
            Assert.Equal(10, result.TaskItemId);
            Assert.Equal(2, result.UserId);

            _commentRepositoryMock.Verify(
                x => x.AddAsync(It.Is<Comment>(c =>
                    c.Content == "This task is completed." &&
                    c.TaskItemId == 10 &&
                    c.UserId == 2)),
                Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_CommentExists_DeletesComment()
        {
            // Arrange
            var comment = new Comment
            {
                Id = 10,
                Content = "Delete this comment",
                TaskItemId = 5,
                UserId = 2
            };

            _commentRepositoryMock
                .Setup(x => x.GetByIdAsync(10))
                .ReturnsAsync(comment);

            // Act
            await _commentService.DeleteAsync(10);

            // Assert
            _commentRepositoryMock.Verify(
                x => x.GetByIdAsync(10),
                Times.Once);

            _commentRepositoryMock.Verify(
                x => x.DeleteAsync(comment),
                Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_CommentDoesNotExist_ThrowsKeyNotFoundException()
        {
            // Arrange
            _commentRepositoryMock
                .Setup(x => x.GetByIdAsync(99))
                .ReturnsAsync((Comment?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _commentService.DeleteAsync(99));

            Assert.Equal(
                "Comment not found.",
                exception.Message);

            _commentRepositoryMock.Verify(
                x => x.DeleteAsync(It.IsAny<Comment>()),
                Times.Never);
        }
    }
}