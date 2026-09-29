using Domain.Entities;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Repositories
{
    public class NotificationRepositoryTests
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
                Name = "Notification User",
                Email = $"notification{Guid.NewGuid()}@example.com",
                PasswordHash = "hash",
                Role = Domain.Enums.UserRole.User,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return user;
        }

        [Fact]
        public async Task AddAsync_ShouldAddNotification()
        {
            await using var context = CreateContext();

            var user = await SeedUserAsync(context);

            var repository =
                new NotificationRepository(context);

            var notification = new Notification
            {
                UserId = user.Id,
                Message = "You have been assigned a task.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            var result =
                await repository.AddAsync(notification);

            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal(user.Id, result.UserId);
            Assert.Equal(
                "You have been assigned a task.",
                result.Message);
            Assert.False(result.IsRead);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNotification()
        {
            await using var context = CreateContext();

            var user = await SeedUserAsync(context);

            var notification = new Notification
            {
                UserId = user.Id,
                Message = "New task assigned.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            context.Notifications.Add(notification);
            await context.SaveChangesAsync();

            var repository =
                new NotificationRepository(context);

            var result =
                await repository.GetByIdAsync(notification.Id);

            Assert.NotNull(result);
            Assert.Equal(
                notification.Id,
                result.Id);

            Assert.Equal(
                user.Id,
                result.UserId);

            Assert.Equal(
                "New task assigned.",
                result.Message);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotificationDoesNotExist()
        {
            await using var context = CreateContext();

            var repository =
                new NotificationRepository(context);

            var result =
                await repository.GetByIdAsync(999999);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByUserIdAsync_ShouldReturnNotificationsForUser()
        {
            await using var context = CreateContext();

            var user1 = new User
            {
                Name = "User One",
                Email = $"user1{Guid.NewGuid()}@example.com",
                PasswordHash = "hash",
                Role = Domain.Enums.UserRole.User,
                CreatedAt = DateTime.UtcNow
            };

            var user2 = new User
            {
                Name = "User Two",
                Email = $"user2{Guid.NewGuid()}@example.com",
                PasswordHash = "hash",
                Role = Domain.Enums.UserRole.User,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.AddRange(user1, user2);
            await context.SaveChangesAsync();

            var notification1 = new Notification
            {
                UserId = user1.Id,
                Message = "First notification",
                IsRead = false,
                CreatedAt = DateTime.UtcNow.AddMinutes(-10)
            };

            var notification2 = new Notification
            {
                UserId = user1.Id,
                Message = "Second notification",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            var otherNotification = new Notification
            {
                UserId = user2.Id,
                Message = "Other user's notification",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            context.Notifications.AddRange(
                notification1,
                notification2,
                otherNotification);

            await context.SaveChangesAsync();

            var repository =
                new NotificationRepository(context);

            var result =
                (await repository.GetByUserIdAsync(user1.Id))
                .ToList();

            Assert.Equal(2, result.Count);

            Assert.Equal(
                "Second notification",
                result[0].Message);

            Assert.Equal(
                "First notification",
                result[1].Message);

            Assert.All(
                result,
                x => Assert.Equal(user1.Id, x.UserId));
        }

        [Fact]
        public async Task GetByUserIdAsync_ShouldReturnEmpty_WhenUserHasNoNotifications()
        {
            await using var context = CreateContext();

            var user = await SeedUserAsync(context);

            var repository =
                new NotificationRepository(context);

            var result =
                (await repository.GetByUserIdAsync(user.Id))
                .ToList();

            Assert.Empty(result);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateNotification()
        {
            await using var context = CreateContext();

            var user = await SeedUserAsync(context);

            var notification = new Notification
            {
                UserId = user.Id,
                Message = "Unread notification",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            context.Notifications.Add(notification);
            await context.SaveChangesAsync();

            var repository =
                new NotificationRepository(context);

            notification.IsRead = true;
            notification.Message = "Read notification";

            await repository.UpdateAsync(notification);

            var updatedNotification =
                await context.Notifications
                    .FirstOrDefaultAsync(
                        x => x.Id == notification.Id);

            Assert.NotNull(updatedNotification);

            Assert.True(
                updatedNotification.IsRead);

            Assert.Equal(
                "Read notification",
                updatedNotification.Message);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteNotification()
        {
            await using var context = CreateContext();

            var user = await SeedUserAsync(context);

            var notification = new Notification
            {
                UserId = user.Id,
                Message = "Delete notification",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            context.Notifications.Add(notification);
            await context.SaveChangesAsync();

            var repository =
                new NotificationRepository(context);

            await repository.DeleteAsync(notification);

            var deletedNotification =
                await context.Notifications
                    .FirstOrDefaultAsync(
                        x => x.Id == notification.Id);

            Assert.Null(deletedNotification);
        }
    }
}