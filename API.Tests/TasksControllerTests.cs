using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.DTOs.Auth;
using Application.DTOs.Tasks;
using Application.DTOs.TeamMembers;
using Application.DTOs.Teams;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;

namespace API.Tests
{
    public class TasksControllerTests
        : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public TasksControllerTests(
            WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        private class TestUser
        {
            public AuthResponse Auth { get; set; } = null!;
            public string Email { get; set; } = string.Empty;
        }

        private async Task<TestUser> RegisterUserAsync(
            UserRole role)
        {
            var email =
                $"task{Guid.NewGuid()}@example.com";

            var request = new RegisterRequest
            {
                Name = $"Task Test {Guid.NewGuid()}",
                Email = email,
                Password = "Password123!"
            };

            var response = await _client.PostAsJsonAsync(
                "/api/Auth/register",
                request);

            response.EnsureSuccessStatusCode();

            var result =
                await response.Content.ReadFromJsonAsync<AuthResponse>();

            Assert.NotNull(result);

            return new TestUser
            {
                Auth = result,
                Email = email
            };
        }

        private void SetBearerToken(string token)
        {
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);
        }

        private async Task<int> CreateTeamAsync(
            string accessToken)
        {
            SetBearerToken(accessToken);

            var request = new CreateTeamRequest
            {
                Name =
                    $"Task Test Team {Guid.NewGuid()}",
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
            string accessToken,
            string email)
        {
            SetBearerToken(accessToken);

            var response =
                await _client.GetAsync("/api/Users");

            response.EnsureSuccessStatusCode();

            var users =
                await response.Content
                    .ReadFromJsonAsync<
                        List<Application.DTOs.Users.UserResponse>>();

            Assert.NotNull(users);

            var user =
                users.FirstOrDefault(
                    x => x.Email == email);

            Assert.NotNull(
                user);

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
                        $"Test Task {Guid.NewGuid()}",
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

        [Fact]
        public async Task GetAll_AuthenticatedUser_ReturnsSuccess()
        {
            var user =
                await RegisterUserAsync(
                    UserRole.User);

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
            _client.DefaultRequestHeaders.Authorization =
                null;

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
                await RegisterUserAsync(
                    UserRole.User);

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
                await RegisterUserAsync(
                    UserRole.Admin);

            var user =
                await RegisterUserAsync(
                    UserRole.User);

            var teamId =
                await CreateTeamAsync(
                    admin.Auth.AccessToken);

            var userId =
                await GetUserIdByEmailAsync(
                    user.Auth.AccessToken,
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
                await RegisterUserAsync(
                    UserRole.User);

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
                    AssignedToId = 1,
                    TeamId = 1
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
            var manager =
                await RegisterUserAsync(
                    UserRole.Manager);

            var user =
                await RegisterUserAsync(
                    UserRole.User);

            var admin =
                await RegisterUserAsync(
                    UserRole.Admin);

            var teamId =
                await CreateTeamAsync(
                    admin.Auth.AccessToken);

            var userId =
                await GetUserIdByEmailAsync(
                    user.Auth.AccessToken,
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
                await RegisterUserAsync(
                    UserRole.Admin);

            var user =
                await RegisterUserAsync(
                    UserRole.User);

            var teamId =
                await CreateTeamAsync(
                    admin.Auth.AccessToken);

            var userId =
                await GetUserIdByEmailAsync(
                    user.Auth.AccessToken,
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
                await RegisterUserAsync(
                    UserRole.Admin);

            var assignedUser =
                await RegisterUserAsync(
                    UserRole.User);

            var otherUser =
                await RegisterUserAsync(
                    UserRole.User);

            var teamId =
                await CreateTeamAsync(
                    admin.Auth.AccessToken);

            var assignedUserId =
                await GetUserIdByEmailAsync(
                    assignedUser.Auth.AccessToken,
                    assignedUser.Email);

            var otherUserId =
                await GetUserIdByEmailAsync(
                    otherUser.Auth.AccessToken,
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
                await RegisterUserAsync(
                    UserRole.Admin);

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
                    AssignedToId = 1,
                    TeamId = 1
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
                await RegisterUserAsync(
                    UserRole.Admin);

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
                await RegisterUserAsync(
                    UserRole.User);

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