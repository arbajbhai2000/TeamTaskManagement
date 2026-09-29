
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.DTOs.Auth;
using Application.DTOs.Users;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;

namespace API.Tests
{
    public class UsersControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public UsersControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        private async Task<string> RegisterAndGetTokenAsync()
        {
            var request = new RegisterRequest
            {
                Name = "Users Test",
                Email = $"users{Guid.NewGuid()}@example.com",
                Password = "Password123!"
            };

            var response = await _client.PostAsJsonAsync(
                "/api/Auth/register",
                request);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result.AccessToken));

            return result.AccessToken;
        }

        [Fact]
        public async Task GetAll_AuthenticatedUser_ReturnsSuccess()
        {
            var token = await RegisterAndGetTokenAsync();

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/Users");

            Assert.True(
                response.IsSuccessStatusCode,
                $"Expected success but received {(int)response.StatusCode} {response.StatusCode}");
        }

        [Fact]
        public async Task GetById_AuthenticatedUser_ReturnsUser()
        {
            var email = $"userid{Guid.NewGuid()}@example.com";

            var registerRequest = new RegisterRequest
            {
                Name = "Get By Id User",
                Email = email,
                Password = "Password123!"
            };

            var registerResponse = await _client.PostAsJsonAsync(
                "/api/Auth/register",
                registerRequest);

            Assert.True(
                registerResponse.IsSuccessStatusCode,
                $"Registration failed with {(int)registerResponse.StatusCode} {registerResponse.StatusCode}");

            var authResponse =
                await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();

            Assert.NotNull(authResponse);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    authResponse.AccessToken);

            var usersResponse = await _client.GetAsync("/api/Users");

            Assert.True(
                usersResponse.IsSuccessStatusCode,
                $"Get users failed with {(int)usersResponse.StatusCode} {usersResponse.StatusCode}");

            var users =
                await usersResponse.Content.ReadFromJsonAsync<List<UserResponse>>();

            Assert.NotNull(users);

            var user = users.FirstOrDefault(x => x.Email == email);

            Assert.NotNull(user);

            var response = await _client.GetAsync(
                $"/api/Users/{user.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result =
                await response.Content.ReadFromJsonAsync<UserResponse>();

            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
            Assert.Equal(email, result.Email);
        }

        [Fact]
        public async Task GetById_NonExistingUser_ReturnsNotFound()
        {
            var token = await RegisterAndGetTokenAsync();

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/Users/999999");

            Assert.Equal(
                HttpStatusCode.NotFound,
                response.StatusCode);
        }

        [Fact]
        public async Task GetAll_WithoutAuthentication_ReturnsUnauthorized()
        {
            _client.DefaultRequestHeaders.Authorization = null;

            var response = await _client.GetAsync("/api/Users");

            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }
    }
}

