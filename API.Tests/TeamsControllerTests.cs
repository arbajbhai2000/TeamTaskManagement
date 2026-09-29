
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.DTOs.Auth;
using Application.DTOs.Teams;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;

namespace API.Tests
{
    public class TeamsControllerTests
        : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public TeamsControllerTests(
            WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        private async Task<AuthResponse> RegisterUserAsync(
            UserRole role)
        {
            var request = new RegisterRequest
            {
                Name = $"Team Test {Guid.NewGuid()}",
                Email = $"team{Guid.NewGuid()}@example.com",
                Password = "Password123!"
            };

            var response = await _client.PostAsJsonAsync(
                "/api/Auth/register",
                request);

            response.EnsureSuccessStatusCode();

            var result =
                await response.Content.ReadFromJsonAsync<AuthResponse>();

            Assert.NotNull(result);

            return result;
        }

        private void SetBearerToken(string token)
        {
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        [Fact]
        public async Task GetAll_AuthenticatedUser_ReturnsSuccess()
        {
            var auth = await RegisterUserAsync(UserRole.User);

            SetBearerToken(auth.AccessToken);

            var response = await _client.GetAsync("/api/Teams");

            Assert.True(
                response.IsSuccessStatusCode,
                $"Expected success but received {(int)response.StatusCode} {response.StatusCode}");
        }

        [Fact]
        public async Task GetById_NonExistingTeam_ReturnsNotFound()
        {
            var auth = await RegisterUserAsync(UserRole.User);

            SetBearerToken(auth.AccessToken);

            var response = await _client.GetAsync("/api/Teams/999999");

            Assert.Equal(
                HttpStatusCode.NotFound,
                response.StatusCode);
        }

        [Fact]
        public async Task Create_AdminUser_ReturnsCreated()
        {
            var auth = await RegisterUserAsync(UserRole.Admin);

            SetBearerToken(auth.AccessToken);

            var request = new CreateTeamRequest
            {
                Name = $"Test Team {Guid.NewGuid()}",
                Description = "Team created by API integration test"
            };

            var response = await _client.PostAsJsonAsync(
                "/api/Teams",
                request);

            Assert.Equal(
                HttpStatusCode.Created,
                response.StatusCode);

            var result =
                await response.Content.ReadFromJsonAsync<TeamResponse>();

            Assert.NotNull(result);
            Assert.Equal(request.Name, result.Name);
            Assert.Equal(request.Description, result.Description);
        }

        [Fact]
        public async Task Create_RegularUser_ReturnsForbidden()
        {
            var auth = await RegisterUserAsync(UserRole.User);

            SetBearerToken(auth.AccessToken);

            var request = new CreateTeamRequest
            {
                Name = $"Unauthorized Team {Guid.NewGuid()}",
                Description = "This should not be created"
            };

            var response = await _client.PostAsJsonAsync(
                "/api/Teams",
                request);

            Assert.Equal(
                HttpStatusCode.Forbidden,
                response.StatusCode);
        }

        [Fact]
        public async Task Create_ManagerUser_ReturnsForbidden()
        {
            var auth = await RegisterUserAsync(UserRole.Manager);

            SetBearerToken(auth.AccessToken);

            var request = new CreateTeamRequest
            {
                Name = $"Manager Team {Guid.NewGuid()}",
                Description = "Manager should not create teams"
            };

            var response = await _client.PostAsJsonAsync(
                "/api/Teams",
                request);

            Assert.Equal(
                HttpStatusCode.Forbidden,
                response.StatusCode);
        }

        [Fact]
        public async Task Create_WithoutAuthentication_ReturnsUnauthorized()
        {
            _client.DefaultRequestHeaders.Authorization = null;

            var request = new CreateTeamRequest
            {
                Name = $"Unauthorized Team {Guid.NewGuid()}",
                Description = "Unauthenticated request"
            };

            var response = await _client.PostAsJsonAsync(
                "/api/Teams",
                request);

            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }

        [Fact]
        public async Task Update_NonExistingTeam_AdminUser_ReturnsNotFound()
        {
            var auth = await RegisterUserAsync(UserRole.Admin);

            SetBearerToken(auth.AccessToken);

            var request = new UpdateTeamRequest
            {
                Name = "Updated Team",
                Description = "Updated description"
            };

            var response = await _client.PutAsJsonAsync(
                "/api/Teams/999999",
                request);

            Assert.Equal(
                HttpStatusCode.NotFound,
                response.StatusCode);
        }

        [Fact]
        public async Task Delete_NonExistingTeam_AdminUser_ReturnsNotFound()
        {
            var auth = await RegisterUserAsync(UserRole.Admin);

            SetBearerToken(auth.AccessToken);

            var response = await _client.DeleteAsync(
                "/api/Teams/999999");

            Assert.Equal(
                HttpStatusCode.NotFound,
                response.StatusCode);
        }
    }
}

