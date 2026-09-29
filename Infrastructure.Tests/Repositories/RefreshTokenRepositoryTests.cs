using Domain.Entities;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Repositories
{
    public class RefreshTokenRepositoryTests
    {
        private static ApplicationDbContext CreateContext()
        {
            var options =
                new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseInMemoryDatabase(
                        Guid.NewGuid().ToString())
                    .Options;

            return new ApplicationDbContext(options);
        }

        private static async Task<User> SeedUserAsync(
            ApplicationDbContext context)
        {
            var user = new User
            {
                Name = "Refresh Token User",
                Email = $"refresh{Guid.NewGuid()}@example.com",
                PasswordHash = "hash",
                Role = Domain.Enums.UserRole.User,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return user;
        }

        [Fact]
        public async Task SaveAsync_ShouldSaveRefreshToken()
        {
            await using var context = CreateContext();

            var user = await SeedUserAsync(context);

            var repository =
                new RefreshTokenRepository(context);

            var token = "test-refresh-token";
            var expiresAt = DateTime.UtcNow.AddDays(7);

            await repository.SaveAsync(
                user.Id,
                token,
                expiresAt);

            var savedToken =
                await context.RefreshTokens
                    .FirstOrDefaultAsync(
                        x => x.Token == token);

            Assert.NotNull(savedToken);
            Assert.Equal(user.Id, savedToken.UserId);
            Assert.Equal(token, savedToken.Token);
            Assert.Equal(expiresAt, savedToken.ExpiresAt);
        }

        [Fact]
        public async Task GetUserIdAsync_ShouldReturnUserId_WhenTokenIsValid()
        {
            await using var context = CreateContext();

            var user = await SeedUserAsync(context);

            var token = "valid-refresh-token";

            context.RefreshTokens.Add(
                new RefreshToken
                {
                    UserId = user.Id,
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow
                });

            await context.SaveChangesAsync();

            var repository =
                new RefreshTokenRepository(context);

            var result =
                await repository.GetUserIdAsync(token);

            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Value);
        }

        [Fact]
        public async Task GetUserIdAsync_ShouldReturnNull_WhenTokenDoesNotExist()
        {
            await using var context = CreateContext();

            var repository =
                new RefreshTokenRepository(context);

            var result =
                await repository.GetUserIdAsync(
                    "missing-token");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserIdAsync_ShouldReturnNull_WhenTokenIsExpired()
        {
            await using var context = CreateContext();

            var user = await SeedUserAsync(context);

            var token = "expired-refresh-token";

            context.RefreshTokens.Add(
                new RefreshToken
                {
                    UserId = user.Id,
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(-10),
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                });

            await context.SaveChangesAsync();

            var repository =
                new RefreshTokenRepository(context);

            var result =
                await repository.GetUserIdAsync(token);

            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteRefreshToken()
        {
            await using var context = CreateContext();

            var user = await SeedUserAsync(context);

            var token = "delete-refresh-token";

            context.RefreshTokens.Add(
                new RefreshToken
                {
                    UserId = user.Id,
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow
                });

            await context.SaveChangesAsync();

            var repository =
                new RefreshTokenRepository(context);

            await repository.DeleteAsync(token);

            var deletedToken =
                await context.RefreshTokens
                    .FirstOrDefaultAsync(
                        x => x.Token == token);

            Assert.Null(deletedToken);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDoNothing_WhenTokenDoesNotExist()
        {
            await using var context = CreateContext();

            var repository =
                new RefreshTokenRepository(context);

            await repository.DeleteAsync(
                "non-existing-token");

            var count =
                await context.RefreshTokens.CountAsync();

            Assert.Equal(0, count);
        }
    }
}