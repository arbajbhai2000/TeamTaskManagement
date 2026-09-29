
using System.Net;
using System.Net.Http.Json;
using Application.DTOs.Auth;
using Microsoft.AspNetCore.Mvc.Testing;

namespace API.Tests
{
    public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public AuthControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Register_ValidUser_ReturnsSuccess()
        {
            var request = new RegisterRequest
            {
                Name = "Test User",
                Email = $"test{Guid.NewGuid()}@example.com",
                Password = "Password123!"
                
            };

            var response = await _client.PostAsJsonAsync(
                "/api/Auth/register",
                request);

            Assert.True(
                response.IsSuccessStatusCode,
                $"Expected success but received {(int)response.StatusCode} {response.StatusCode}");

            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result.AccessToken));
            Assert.False(string.IsNullOrEmpty(result.RefreshToken));
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsSuccess()
        {
            var email = $"login{Guid.NewGuid()}@example.com";
            var password = "Password123!";

            var registerRequest = new RegisterRequest
            {
                Name = "Login Test User",
                Email = email,
                Password = password
            };

            var registerResponse = await _client.PostAsJsonAsync(
                "/api/Auth/register",
                registerRequest);

            Assert.True(
                registerResponse.IsSuccessStatusCode,
                $"Registration failed with {(int)registerResponse.StatusCode} {registerResponse.StatusCode}");

            var loginRequest = new LoginRequest
            {
                Email = email,
                Password = password
            };

            var response = await _client.PostAsJsonAsync(
                "/api/Auth/login",
                loginRequest);

            Assert.True(
                response.IsSuccessStatusCode,
                $"Expected success but received {(int)response.StatusCode} {response.StatusCode}");

            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result.AccessToken));
            Assert.False(string.IsNullOrEmpty(result.RefreshToken));
        }

        [Fact]
        public async Task Login_InvalidPassword_ReturnsUnauthorized()
        {
            var email = $"invalid{Guid.NewGuid()}@example.com";
            var correctPassword = "Password123!";

            var registerRequest = new RegisterRequest
            {
                Name = "Invalid Login User",
                Email = email,
                Password = correctPassword
            };

            var registerResponse = await _client.PostAsJsonAsync(
                "/api/Auth/register",
                registerRequest);

            Assert.True(
                registerResponse.IsSuccessStatusCode,
                $"Registration failed with {(int)registerResponse.StatusCode} {registerResponse.StatusCode}");

            var loginRequest = new LoginRequest
            {
                Email = email,
                Password = "WrongPassword123!"
            };

            var response = await _client.PostAsJsonAsync(
                "/api/Auth/login",
                loginRequest);

            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }
    }
}

