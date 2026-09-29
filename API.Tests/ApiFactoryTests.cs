using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace API.Tests
{
    public class ApiFactoryTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public ApiFactoryTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Api_StartsSuccessfully_ReturnsResponse()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/swagger/index.html");

            Assert.True(
                response.IsSuccessStatusCode,
                $"Expected successful response but received {(int)response.StatusCode} {response.StatusCode}");
        }
    }
}