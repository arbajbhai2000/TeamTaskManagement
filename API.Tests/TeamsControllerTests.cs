using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Application.DTOs.Auth;
using Application.DTOs.Teams;
using Application.DTOs.Users;

using Domain.Enums;

using Infrastructure.Persistence;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace API.Tests
{
    public class TeamsControllerTests
        : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public TeamsControllerTests(
            WebApplicationFactory<Program> factory)
        {
            _factory = factory;

            _client = factory.CreateClient();
        }

        // ============================================================
        // TEST USER
        // ============================================================

        private class TestUser
        {
            public AuthResponse Auth { get; set; } = null!;

            public string Email { get; set; } = string.Empty;
        }

        // ============================================================
        // REGISTER NORMAL USER
        // ============================================================

        private async Task<TestUser> RegisterUserAsync()
        {
            var email =
                $"teamuser_{Guid.NewGuid():N}@example.com";

            var request = new RegisterRequest
            {
                Name =
                    $"Team Test User {Guid.NewGuid():N}",

                Email =
                    email,

                Password =
                    "Password123!"
            };

            var response =
                await _client.PostAsJsonAsync(
                    "/api/Auth/register",
                    request);

            response.EnsureSuccessStatusCode();

            var auth =
                await response.Content
                    .ReadFromJsonAsync<AuthResponse>();

            Assert.NotNull(auth);

            return new TestUser
            {
                Auth = auth,
                Email = email
            };
        }

        // ============================================================
        // CREATE TEST ADMIN DIRECTLY IN TEST DATABASE
        // ============================================================

        private async Task<TestUser> CreateTestAdminAsync()
        {
            var email =
                $"testadmin_{Guid.NewGuid():N}@example.com";

            var password =
                "Admin@12345";

            using var scope =
                _factory.Services.CreateScope();

            var db =
                scope.ServiceProvider
                    .GetRequiredService<ApplicationDbContext>();

            var admin = new Domain.Entities.User
            {
                Name =
                    $"Test Admin {Guid.NewGuid():N}",

                Email =
                    email,

                PasswordHash =
                    BCrypt.Net.BCrypt.HashPassword(
                        password),

                Role =
                    UserRole.Admin,

                CreatedAt =
                    DateTime.UtcNow
            };

            db.Users.Add(admin);

            await db.SaveChangesAsync();

            // Login through the real API to obtain a JWT.
            var loginRequest = new LoginRequest
            {
                Email = email,
                Password = password
            };

            var response =
                await _client.PostAsJsonAsync(
                    "/api/Auth/login",
                    loginRequest);

            response.EnsureSuccessStatusCode();

            var auth =
                await response.Content
                    .ReadFromJsonAsync<AuthResponse>();

            Assert.NotNull(auth);

            return new TestUser
            {
                Auth = auth,
                Email = email
            };
        }

        // ============================================================
        // AUTH HELPERS
        // ============================================================

        private void SetBearerToken(string token)
        {
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);
        }

        private void ClearBearerToken()
        {
            _client.DefaultRequestHeaders.Authorization = null;
        }

        // ============================================================
        // TESTS
        // ============================================================

        [Fact]
        public async Task GetAll_AuthenticatedUser_ReturnsSuccess()
        {
            var user =
                await RegisterUserAsync();

            SetBearerToken(
                user.Auth.AccessToken);

            var response =
                await _client.GetAsync(
                    "/api/Teams");

            Assert.True(
                response.IsSuccessStatusCode,
                $"Expected success but received {(int)response.StatusCode} {response.StatusCode}");
        }

        [Fact]
        public async Task GetById_NonExistingTeam_ReturnsNotFound()
        {
            var user =
                await RegisterUserAsync();

            SetBearerToken(
                user.Auth.AccessToken);

            var response =
                await _client.GetAsync(
                    "/api/Teams/999999");

            Assert.Equal(
                HttpStatusCode.NotFound,
                response.StatusCode);
        }

        [Fact]
        public async Task Create_AdminUser_ReturnsCreated()
        {
            var admin =
                await CreateTestAdminAsync();

            SetBearerToken(
                admin.Auth.AccessToken);

            var request =
                new CreateTeamRequest
                {
                    Name =
                        $"Test Team {Guid.NewGuid():N}",

                    Description =
                        "Team created by API integration test"
                };

            var response =
                await _client.PostAsJsonAsync(
                    "/api/Teams",
                    request);

            Assert.Equal(
                HttpStatusCode.Created,
                response.StatusCode);

            var result =
                await response.Content
                    .ReadFromJsonAsync<TeamResponse>();

            Assert.NotNull(result);

            Assert.Equal(
                request.Name,
                result.Name);

            Assert.Equal(
                request.Description,
                result.Description);
        }

        [Fact]
        public async Task Create_RegularUser_ReturnsForbidden()
        {
            var user =
                await RegisterUserAsync();

            SetBearerToken(
                user.Auth.AccessToken);

            var request =
                new CreateTeamRequest
                {
                    Name =
                        $"Unauthorized Team {Guid.NewGuid():N}",

                    Description =
                        "This should not be created"
                };

            var response =
                await _client.PostAsJsonAsync(
                    "/api/Teams",
                    request);

            Assert.Equal(
                HttpStatusCode.Forbidden,
                response.StatusCode);
        }

        [Fact]
        public async Task Create_ManagerUser_ReturnsForbidden()
        {
            var user =
                await RegisterUserAsync();

            SetBearerToken(
                user.Auth.AccessToken);

            var request =
                new CreateTeamRequest
                {
                    Name =
                        $"Manager Team {Guid.NewGuid():N}",

                    Description =
                        "Manager should not create teams"
                };

            var response =
                await _client.PostAsJsonAsync(
                    "/api/Teams",
                    request);

            Assert.Equal(
                HttpStatusCode.Forbidden,
                response.StatusCode);
        }

        [Fact]
        public async Task Create_WithoutAuthentication_ReturnsUnauthorized()
        {
            ClearBearerToken();

            var request =
                new CreateTeamRequest
                {
                    Name =
                        $"Unauthorized Team {Guid.NewGuid():N}",

                    Description =
                        "Unauthenticated request"
                };

            var response =
                await _client.PostAsJsonAsync(
                    "/api/Teams",
                    request);

            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }

        [Fact]
        public async Task Update_NonExistingTeam_AdminUser_ReturnsNotFound()
        {
            var admin =
                await CreateTestAdminAsync();

            SetBearerToken(
                admin.Auth.AccessToken);

            var request =
                new UpdateTeamRequest
                {
                    Name =
                        "Updated Team",

                    Description =
                        "Updated description"
                };

            var response =
                await _client.PutAsJsonAsync(
                    "/api/Teams/999999",
                    request);

            Assert.Equal(
                HttpStatusCode.NotFound,
                response.StatusCode);
        }

        [Fact]
        public async Task Delete_NonExistingTeam_AdminUser_ReturnsNotFound()
        {
            var admin =
                await CreateTestAdminAsync();

            SetBearerToken(
                admin.Auth.AccessToken);

            var response =
                await _client.DeleteAsync(
                    "/api/Teams/999999");

            Assert.Equal(
                HttpStatusCode.NotFound,
                response.StatusCode);
        }
    }
}