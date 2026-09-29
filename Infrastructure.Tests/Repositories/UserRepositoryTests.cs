using Domain.Entities;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Repositories
{
    public class UserRepositoryTests
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

        [Fact]
        public async Task AddAsync_ShouldAddUser()
        {
            await using var context = CreateContext();

            var repository =
                new UserRepository(context);

            var user = new User
            {
                Name = "Test User",
                Email = $"test{Guid.NewGuid()}@example.com",
                PasswordHash = "hashed-password",
                Role = Domain.Enums.UserRole.User,
                CreatedAt = DateTime.UtcNow
            };

            var result =
                await repository.AddAsync(user);

            Assert.NotNull(result);
            Assert.True(result.Id > 0);

            var savedUser =
                await context.Users
                    .FirstOrDefaultAsync(x => x.Id == result.Id);

            Assert.NotNull(savedUser);
            Assert.Equal(user.Name, savedUser.Name);
            Assert.Equal(user.Email, savedUser.Email);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnUser()
        {
            await using var context = CreateContext();

            var user = new User
            {
                Name = "John Doe",
                Email = "john@example.com",
                PasswordHash = "hashed-password",
                Role = Domain.Enums.UserRole.User,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var repository =
                new UserRepository(context);

            var result =
                await repository.GetByIdAsync(user.Id);

            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
            Assert.Equal("John Doe", result.Name);
            Assert.Equal("john@example.com", result.Email);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            await using var context = CreateContext();

            var repository =
                new UserRepository(context);

            var result =
                await repository.GetByIdAsync(999999);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByEmailAsync_ShouldReturnUser()
        {
            await using var context = CreateContext();

            var user = new User
            {
                Name = "Email User",
                Email = "emailuser@example.com",
                PasswordHash = "hashed-password",
                Role = Domain.Enums.UserRole.User,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var repository =
                new UserRepository(context);

            var result =
                await repository.GetByEmailAsync(
                    "emailuser@example.com");

            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
            Assert.Equal(
                "emailuser@example.com",
                result.Email);
        }

        [Fact]
        public async Task GetByEmailAsync_ShouldReturnNull_WhenEmailDoesNotExist()
        {
            await using var context = CreateContext();

            var repository =
                new UserRepository(context);

            var result =
                await repository.GetByEmailAsync(
                    "missing@example.com");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnUsersOrderedByName()
        {
            await using var context = CreateContext();

            context.Users.AddRange(
                new User
                {
                    Name = "Zack",
                    Email = "zack@example.com",
                    PasswordHash = "hash",
                    Role = Domain.Enums.UserRole.User,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Name = "Arbaj",
                    Email = "arbaj@example.com",
                    PasswordHash = "hash",
                    Role = Domain.Enums.UserRole.User,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Name = "John",
                    Email = "john@example.com",
                    PasswordHash = "hash",
                    Role = Domain.Enums.UserRole.User,
                    CreatedAt = DateTime.UtcNow
                });

            await context.SaveChangesAsync();

            var repository =
                new UserRepository(context);

            var result =
                (await repository.GetAllAsync()).ToList();

            Assert.Equal(3, result.Count);

            Assert.Equal("Arbaj", result[0].Name);
            Assert.Equal("John", result[1].Name);
            Assert.Equal("Zack", result[2].Name);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateUser()
        {
            await using var context = CreateContext();

            var user = new User
            {
                Name = "Old Name",
                Email = "update@example.com",
                PasswordHash = "old-hash",
                Role = Domain.Enums.UserRole.User,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var repository =
                new UserRepository(context);

            user.Name = "Updated Name";
            user.Role = Domain.Enums.UserRole.Manager;

            await repository.UpdateAsync(user);

            var updatedUser =
                await context.Users
                    .FirstOrDefaultAsync(
                        x => x.Id == user.Id);

            Assert.NotNull(updatedUser);
            Assert.Equal(
                "Updated Name",
                updatedUser.Name);

            Assert.Equal(
                Domain.Enums.UserRole.Manager,
                updatedUser.Role);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteUser()
        {
            await using var context = CreateContext();

            var user = new User
            {
                Name = "Delete User",
                Email = "delete@example.com",
                PasswordHash = "hash",
                Role = Domain.Enums.UserRole.User,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var repository =
                new UserRepository(context);

            await repository.DeleteAsync(user);

            var deletedUser =
                await context.Users
                    .FirstOrDefaultAsync(
                        x => x.Id == user.Id);

            Assert.Null(deletedUser);
        }
    }
}