using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Users;
using Application.Interfaces.Repositories;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Moq;

namespace Application.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _userService = new UserService(_userRepositoryMock.Object);
        }

        [Fact]
        public async Task GetByIdAsync_UserExists_ReturnsUserResponse()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Name = "Arbaj",
                Email = "arbaj@test.com",
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow
            };

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Arbaj", result.Name);
            Assert.Equal("arbaj@test.com", result.Email);
            Assert.Equal(UserRole.User, result.Role);

            _userRepositoryMock.Verify(
                x => x.GetByIdAsync(1),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_UserDoesNotExist_ReturnsNull()
        {
            // Arrange
            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(99))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.GetByIdAsync(99);

            // Assert
            Assert.Null(result);

            _userRepositoryMock.Verify(
                x => x.GetByIdAsync(99),
                Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllUsers()
        {
            // Arrange
            var users = new List<User>
            {
                new User
                {
                    Id = 1,
                    Name = "User One",
                    Email = "user1@test.com",
                    Role = UserRole.User,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = 2,
                    Name = "Manager One",
                    Email = "manager@test.com",
                    Role = UserRole.Manager,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _userRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(users);

            // Act
            var result = (await _userService.GetAllAsync()).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("User One", result[0].Name);
            Assert.Equal("Manager One", result[1].Name);
            Assert.Equal(UserRole.Manager, result[1].Role);

            _userRepositoryMock.Verify(
                x => x.GetAllAsync(),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_UserExists_UpdatesUserAndReturnsResponse()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Name = "Old Name",
                Email = "user@test.com",
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var request = new UpdateUserRequest
            {
                Name = "Updated Name",
                Role = UserRole.Manager
            };

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);

            _userRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _userService.UpdateAsync(1, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Name", result.Name);
            Assert.Equal(UserRole.Manager, result.Role);

            _userRepositoryMock.Verify(
                x => x.GetByIdAsync(1),
                Times.Once);

            _userRepositoryMock.Verify(
                x => x.UpdateAsync(It.Is<User>(u =>
                    u.Id == 1 &&
                    u.Name == "Updated Name" &&
                    u.Role == UserRole.Manager)),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_UserDoesNotExist_ReturnsNull()
        {
            // Arrange
            var request = new UpdateUserRequest
            {
                Name = "Updated Name",
                Role = UserRole.Manager
            };

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(99))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.UpdateAsync(99, request);

            // Assert
            Assert.Null(result);

            _userRepositoryMock.Verify(
                x => x.GetByIdAsync(99),
                Times.Once);

            _userRepositoryMock.Verify(
                x => x.UpdateAsync(It.IsAny<User>()),
                Times.Never);
        }
    }
}