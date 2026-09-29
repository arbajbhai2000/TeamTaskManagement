using Application.DTOs.Auth;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Application.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly IConfiguration _configuration;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
            _jwtServiceMock = new Mock<IJwtService>();

            var configurationData = new Dictionary<string, string?>
            {
                ["Jwt:RefreshTokenExpiryDays"] = "7"
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configurationData)
                .Build();

            _authService = new AuthService(
                _userRepositoryMock.Object,
                _refreshTokenRepositoryMock.Object,
                _jwtServiceMock.Object,
                _configuration);
        }

        [Fact]
        public async Task RegisterAsync_NewUser_ReturnsAuthResponse()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Name = "Arbaj",
                Email = "arbaj@test.com",
                Password = "Password123!"
                
            };

            var createdUser = new User
            {
                Id = 1,
                Name = "Arbaj",
                Email = "arbaj@test.com",
                Role = UserRole.User,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow
            };

            var expiry = DateTime.UtcNow.AddMinutes(60);

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync(request.Email))
                .ReturnsAsync((User?)null);

            _userRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<User>()))
                .ReturnsAsync(createdUser);

            _jwtServiceMock
                .Setup(x => x.GenerateAccessToken(It.IsAny<User>()))
                .Returns("access-token");

            _jwtServiceMock
                .Setup(x => x.GenerateRefreshToken())
                .Returns("refresh-token");

            _jwtServiceMock
                .Setup(x => x.GetAccessTokenExpiry())
                .Returns(expiry);

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("access-token", result.AccessToken);
            Assert.Equal("refresh-token", result.RefreshToken);
            Assert.Equal(expiry, result.ExpiresAt);

            _userRepositoryMock.Verify(
                x => x.AddAsync(It.Is<User>(u =>
                    u.Name == "Arbaj" &&
                    u.Email == "arbaj@test.com" &&
                    u.Role == UserRole.User &&
                    !string.IsNullOrEmpty(u.PasswordHash))),
                Times.Once);

            _refreshTokenRepositoryMock.Verify(
                x => x.SaveAsync(
                    1,
                    "refresh-token",
                    It.IsAny<DateTime>()),
                Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_EmailAlreadyExists_ThrowsInvalidOperationException()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Name = "Arbaj",
                Email = "existing@test.com",
                Password = "Password123!"
                
            };

            var existingUser = new User
            {
                Id = 1,
                Name = "Existing User",
                Email = "existing@test.com",
                Role = UserRole.User
            };

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync(request.Email))
                .ReturnsAsync(existingUser);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _authService.RegisterAsync(request));

            Assert.Equal("Email already registered.", exception.Message);

            _userRepositoryMock.Verify(
                x => x.AddAsync(It.IsAny<User>()),
                Times.Never);
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
        {
            // Arrange
            var password = "Password123!";

            var user = new User
            {
                Id = 1,
                Name = "Arbaj",
                Email = "arbaj@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow
            };

            var request = new LoginRequest
            {
                Email = "arbaj@test.com",
                Password = password
            };

            var expiry = DateTime.UtcNow.AddMinutes(60);

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync(request.Email))
                .ReturnsAsync(user);

            _jwtServiceMock
                .Setup(x => x.GenerateAccessToken(user))
                .Returns("access-token");

            _jwtServiceMock
                .Setup(x => x.GenerateRefreshToken())
                .Returns("refresh-token");

            _jwtServiceMock
                .Setup(x => x.GetAccessTokenExpiry())
                .Returns(expiry);

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("access-token", result.AccessToken);
            Assert.Equal("refresh-token", result.RefreshToken);
            Assert.Equal(expiry, result.ExpiresAt);

            _userRepositoryMock.Verify(
                x => x.GetByEmailAsync(request.Email),
                Times.Once);

            _refreshTokenRepositoryMock.Verify(
                x => x.SaveAsync(
                    1,
                    "refresh-token",
                    It.IsAny<DateTime>()),
                Times.Once);
        }

        [Fact]
        public async Task LoginAsync_UserDoesNotExist_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "missing@test.com",
                Password = "Password123!"
            };

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync(request.Email))
                .ReturnsAsync((User?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authService.LoginAsync(request));

            Assert.Equal(
                "Invalid email or password.",
                exception.Message);

            _jwtServiceMock.Verify(
                x => x.GenerateAccessToken(It.IsAny<User>()),
                Times.Never);
        }

        [Fact]
        public async Task LoginAsync_InvalidPassword_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Name = "Arbaj",
                Email = "arbaj@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword123!"),
                Role = UserRole.User
            };

            var request = new LoginRequest
            {
                Email = "arbaj@test.com",
                Password = "WrongPassword123!"
            };

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync(request.Email))
                .ReturnsAsync(user);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authService.LoginAsync(request));

            Assert.Equal(
                "Invalid email or password.",
                exception.Message);

            _jwtServiceMock.Verify(
                x => x.GenerateAccessToken(It.IsAny<User>()),
                Times.Never);

            _refreshTokenRepositoryMock.Verify(
                x => x.SaveAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>()),
                Times.Never);
        }

        [Fact]
        public async Task RefreshTokenAsync_InvalidToken_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            _refreshTokenRepositoryMock
                .Setup(x => x.GetUserIdAsync("invalid-token"))
                .ReturnsAsync((int?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authService.RefreshTokenAsync("invalid-token"));

            Assert.Equal(
                "Invalid or expired refresh token.",
                exception.Message);

            _userRepositoryMock.Verify(
                x => x.GetByIdAsync(It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task RefreshTokenAsync_UserDoesNotExist_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            _refreshTokenRepositoryMock
                .Setup(x => x.GetUserIdAsync("refresh-token"))
                .ReturnsAsync(99);

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(99))
                .ReturnsAsync((User?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authService.RefreshTokenAsync("refresh-token"));

            Assert.Equal(
                "User not found.",
                exception.Message);

            _refreshTokenRepositoryMock.Verify(
                x => x.DeleteAsync(It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task RefreshTokenAsync_ValidToken_ReturnsNewAuthResponse()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Name = "Arbaj",
                Email = "arbaj@test.com",
                Role = UserRole.User
            };

            var expiry = DateTime.UtcNow.AddMinutes(60);

            _refreshTokenRepositoryMock
                .Setup(x => x.GetUserIdAsync("old-refresh-token"))
                .ReturnsAsync(1);

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);

            _jwtServiceMock
                .Setup(x => x.GenerateAccessToken(user))
                .Returns("new-access-token");

            _jwtServiceMock
                .Setup(x => x.GenerateRefreshToken())
                .Returns("new-refresh-token");

            _jwtServiceMock
                .Setup(x => x.GetAccessTokenExpiry())
                .Returns(expiry);

            // Act
            var result = await _authService.RefreshTokenAsync(
                "old-refresh-token");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("new-access-token", result.AccessToken);
            Assert.Equal("new-refresh-token", result.RefreshToken);
            Assert.Equal(expiry, result.ExpiresAt);

            _refreshTokenRepositoryMock.Verify(
                x => x.DeleteAsync("old-refresh-token"),
                Times.Once);

            _refreshTokenRepositoryMock.Verify(
                x => x.SaveAsync(
                    1,
                    "new-refresh-token",
                    It.IsAny<DateTime>()),
                Times.Once);
        }
    }
}