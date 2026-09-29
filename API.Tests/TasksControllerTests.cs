using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Application.DTOs.Auth;
using Application.DTOs.Tasks;
using Application.DTOs.TeamMembers;
using Application.DTOs.Teams;
using Application.DTOs.Users;

using Domain.Enums;

using Infrastructure.Persistence;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace API.Tests
{
    public class TasksControllerTests
        : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public TasksControllerTests(
            WebApplicationFactory<Program> factory)
        {
            _factory = factory;

            _client = factory.CreateClient();
        }

        private class TestUser
        {
            public AuthResponse Auth { get; set; } = null!;

            public string Email { get; set; } = string.Empty;
        }

        // ============================================================
        // TEST USER HELPERS
        // ============================================================

        private async Task<TestUser> RegisterUserAsync()
        {
            var email =
                $"taskuser_{Guid.NewGuid():N}@example.com";

            var request = new RegisterRequest
            {
                Name =
                    $"Task Test User {Guid.NewGuid():N}",

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

            var result =
                await response.Content
                    .ReadFromJsonAsync<AuthResponse>();

            Assert.NotNull(result);

            return new TestUser
            {
                Auth = result,
                Email = email
            };
        }

        // ============================================================
        // CREATE TEST ADMIN DIRECTLY IN TEST DATABASE
        // ============================================================
        //
        // Public registration intentionally creates only User.
        // Therefore we bootstrap an Admin directly in the test DB.
        //
        // This does NOT change production behavior.
        //
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
        // CREATE USER / MANAGER / ADMIN THROUGH REAL ADMIN API
        // ============================================================

        private async Task<TestUser> CreateUserAsAdminAsync(
            string adminToken,
            UserRole role)
        {
            var email =
                $"task_{role.ToString().ToLower()}_{Guid.NewGuid():N}@example.com";

            SetBearerToken(adminToken);

            var request = new CreateUserRequest
            {
                Name =
                    $"Task {role} {Guid.NewGuid():N}",

                Email =
                    email,

                Password =
                    "Password123!",

                Role =
                    role
            };

            var response =
                await _client.PostAsJsonAsync(
                    "/api/Users",
                    request);

            Assert.True(
                response.IsSuccessStatusCode,
                $"Creating {role} failed with {(int)response.StatusCode} {response.StatusCode}");

            // Login so we get an AuthResponse/JWT for this user.
            var loginRequest = new LoginRequest
            {
                Email = email,
                Password = "Password123!"
            };

            var loginResponse =
                await _client.PostAsJsonAsync(
                    "/api/Auth/login",
                    loginRequest);

            loginResponse.EnsureSuccessStatusCode();

            var auth =
                await loginResponse.Content
                    .ReadFromJsonAsync<AuthResponse>();

            Assert.NotNull(auth);

            return new TestUser
            {
                Auth = auth,
                Email = email
            };
        }

        // ============================================================
        // AUTH
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
        // TEAM HELPERS
        // ============================================================

        private async Task<int> CreateTeamAsync(
            string adminToken)
        {
            SetBearerToken(adminToken);

            var request = new CreateTeamRequest
            {
                Name =
                    $"Task Test Team {Guid.NewGuid():N}",

                Description =
                    "Team for task integration test"
            };

            var response =
                await _client.PostAsJsonAsync(
                    "/api/Teams",
                    request);

            Assert.Equal(
                HttpStatusCode.Created,
                response.StatusCode);

            var team =
                await response.Content
                    .ReadFromJsonAsync<TeamResponse>();

            Assert.NotNull(team);

            return team.Id;
        }

        private async Task<int> GetUserIdByEmailAsync(
            string token,
            string email)
        {
            SetBearerToken(token);

            var response =
                await _client.GetAsync(
                    "/api/Users");

            response.EnsureSuccessStatusCode();

            var users =
                await response.Content
                    .ReadFromJsonAsync<
                        List<UserResponse>>();

            Assert.NotNull(users);

            var user =
                users.FirstOrDefault(
                    x => x.Email == email);

            Assert.NotNull(user);

            return user.Id;
        }

        private async Task AddUserToTeamAsync(
            string adminToken,
            int teamId,
            int userId)
        {
            SetBearerToken(adminToken);

            var request =
                new AddTeamMemberRequest
                {
                    UserId = userId
                };

            var response =
                await _client.PostAsJsonAsync(
                    $"/api/teams/{teamId}/members",
                    request);

            Assert.True(
                response.IsSuccessStatusCode,
                $"Adding team member failed with {(int)response.StatusCode} {response.StatusCode}");
        }

        // ============================================================
        // TASK HELPERS
        // ============================================================

        private async Task<int> CreateValidTaskAsync(
            string adminToken,
            int teamId,
            int userId)
        {
            SetBearerToken(adminToken);

            var request =
                new CreateTaskRequest
                {
                    Title =
                        $"Test Task {Guid.NewGuid():N}",

                    Description =
                        "Task created by integration test",

                    Priority =
                        TaskPriority.Medium,

                    DueDate =
                        DateTime.UtcNow.AddDays(7),

                    AssignedToId =
                        userId,

                    TeamId =
                        teamId
                };

            var response =
                await _client.PostAsJsonAsync(
                    "/api/Tasks",
                    request);

            Assert.Equal(
                HttpStatusCode.Created,
                response.StatusCode);

            var task =
                await response.Content
                    .ReadFromJsonAsync<TaskResponse>();

            Assert.NotNull(task);

            return task.Id;
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
                    "/api/Tasks");

            Assert.True(
                response.IsSuccessStatusCode,
                $"Expected success but received {(int)response.StatusCode} {response.StatusCode}");
        }

        [Fact]
        public async Task GetAll_WithoutAuthentication_ReturnsUnauthorized()
        {
            ClearBearerToken();

            var response =
                await _client.GetAsync(
                    "/api/Tasks");

            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }

        [Fact]
        public async Task GetById_NonExistingTask_ReturnsNotFound()
        {
            var user =
                await RegisterUserAsync();

            SetBearerToken(
                user.Auth.AccessToken);

            var response =
                await _client.GetAsync(
                    "/api/Tasks/999999");

            Assert.Equal(
                HttpStatusCode.NotFound,
                response.StatusCode);
        }

        [Fact]
        public async Task Create_AdminUser_WithValidTeamMember_ReturnsCreated()
        {
            var admin =
                await CreateTestAdminAsync();

            var user =
                await CreateUserAsAdminAsync(
                    admin.Auth.AccessToken,
                    UserRole.User);

            var teamId =
                await CreateTeamAsync(
                    admin.Auth.AccessToken);

            var userId =
                await GetUserIdByEmailAsync(
                    admin.Auth.AccessToken,
                    user.Email);

            await AddUserToTeamAsync(
                admin.Auth.AccessToken,
                teamId,
                userId);

            var taskId =
                await CreateValidTaskAsync(
                    admin.Auth.AccessToken,
                    teamId,
                    userId);

            Assert.True(
                taskId > 0);
        }

        [Fact]
        public async Task Create_User_ReturnsForbidden()
        {
            var user =
                await RegisterUserAsync();

            SetBearerToken(
                user.Auth.AccessToken);

            var request =
                new CreateTaskRequest
                {
                    Title =
                        "Unauthorized Task",

                    Description =
                        "User should not create tasks",

                    Priority =
                        TaskPriority.Medium,

                    DueDate =
                        DateTime.UtcNow.AddDays(5),

                    AssignedToId =
                        1,

                    TeamId =
                        1
                };

            var response =
                await _client.PostAsJsonAsync(
                    "/api/Tasks",
                    request);

            Assert.Equal(
                HttpStatusCode.Forbidden,
                response.StatusCode);
        }

        [Fact]
        public async Task Create_Manager_WithInvalidTeamMember_ReturnsBadRequest()
        {
            var admin =
                await CreateTestAdminAsync();

            var manager =
                await CreateUserAsAdminAsync(
                    admin.Auth.AccessToken,
                    UserRole.Manager);

            var user =
                await CreateUserAsAdminAsync(
                    admin.Auth.AccessToken,
                    UserRole.User);

            var teamId =
                await CreateTeamAsync(
                    admin.Auth.AccessToken);

            var userId =
                await GetUserIdByEmailAsync(
                    admin.Auth.AccessToken,
                    user.Email);

            SetBearerToken(
                manager.Auth.AccessToken);

            var request =
                new CreateTaskRequest
                {
                    Title =
                        "Invalid Assignment Task",

                    Description =
                        "User is not a member of this team",

                    Priority =
                        TaskPriority.High,

                    DueDate =
                        DateTime.UtcNow.AddDays(5),

                    AssignedToId =
                        userId,

                    TeamId =
                        teamId
                };

            var response =
                await _client.PostAsJsonAsync(
                    "/api/Tasks",
                    request);

            Assert.Equal(
                HttpStatusCode.BadRequest,
                response.StatusCode);
        }

        [Fact]
        public async Task GetById_UserAssignedToTask_ReturnsSuccess()
        {
            var admin =
                await CreateTestAdminAsync();

            var user =
                await CreateUserAsAdminAsync(
                    admin.Auth.AccessToken,
                    UserRole.User);

            var teamId =
                await CreateTeamAsync(
                    admin.Auth.AccessToken);

            var userId =
                await GetUserIdByEmailAsync(
                    admin.Auth.AccessToken,
                    user.Email);

            await AddUserToTeamAsync(
                admin.Auth.AccessToken,
                teamId,
                userId);

            var taskId =
                await CreateValidTaskAsync(
                    admin.Auth.AccessToken,
                    teamId,
                    userId);

            SetBearerToken(
                user.Auth.AccessToken);

            var response =
                await _client.GetAsync(
                    $"/api/Tasks/{taskId}");

            Assert.Equal(
                HttpStatusCode.OK,
                response.StatusCode);
        }

        [Fact]
        public async Task GetById_UserAccessingAnotherUsersTask_ReturnsForbidden()
        {
            var admin =
                await CreateTestAdminAsync();

            var assignedUser =
                await CreateUserAsAdminAsync(
                    admin.Auth.AccessToken,
                    UserRole.User);

            var otherUser =
                await CreateUserAsAdminAsync(
                    admin.Auth.AccessToken,
                    UserRole.User);

            var teamId =
                await CreateTeamAsync(
                    admin.Auth.AccessToken);

            var assignedUserId =
                await GetUserIdByEmailAsync(
                    admin.Auth.AccessToken,
                    assignedUser.Email);

            var otherUserId =
                await GetUserIdByEmailAsync(
                    admin.Auth.AccessToken,
                    otherUser.Email);

            Assert.NotEqual(
                assignedUserId,
                otherUserId);

            await AddUserToTeamAsync(
                admin.Auth.AccessToken,
                teamId,
                assignedUserId);

            await AddUserToTeamAsync(
                admin.Auth.AccessToken,
                teamId,
                otherUserId);

            var taskId =
                await CreateValidTaskAsync(
                    admin.Auth.AccessToken,
                    teamId,
                    assignedUserId);

            SetBearerToken(
                otherUser.Auth.AccessToken);

            var response =
                await _client.GetAsync(
                    $"/api/Tasks/{taskId}");

            Assert.Equal(
                HttpStatusCode.Forbidden,
                response.StatusCode);
        }

        [Fact]
        public async Task Update_NonExistingTask_AdminUser_ReturnsNotFound()
        {
            var admin =
                await CreateTestAdminAsync();

            SetBearerToken(
                admin.Auth.AccessToken);

            var request =
                new UpdateTaskRequest
                {
                    Title =
                        "Updated Task",

                    Description =
                        "Updated description",

                    Status =
                        TaskItemStatus.InProgress,

                    Priority =
                        TaskPriority.High,

                    DueDate =
                        DateTime.UtcNow.AddDays(5),

                    AssignedToId =
                        1,

                    TeamId =
                        1
                };

            var response =
                await _client.PutAsJsonAsync(
                    "/api/Tasks/999999",
                    request);

            Assert.Equal(
                HttpStatusCode.NotFound,
                response.StatusCode);
        }

        [Fact]
        public async Task Delete_NonExistingTask_AdminUser_ReturnsNotFound()
        {
            var admin =
                await CreateTestAdminAsync();

            SetBearerToken(
                admin.Auth.AccessToken);

            var response =
                await _client.DeleteAsync(
                    "/api/Tasks/999999");

            Assert.Equal(
                HttpStatusCode.NotFound,
                response.StatusCode);
        }

        [Fact]
        public async Task Delete_User_ReturnsForbidden()
        {
            var user =
                await RegisterUserAsync();

            SetBearerToken(
                user.Auth.AccessToken);

            var response =
                await _client.DeleteAsync(
                    "/api/Tasks/999999");

            Assert.Equal(
                HttpStatusCode.Forbidden,
                response.StatusCode);
        }
    }
}