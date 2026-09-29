using Domain.Entities;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Repositories
{
    public class CommentRepositoryTests
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

        private static async Task<(User User, Team Team, TaskItem Task)> SeedDataAsync(
            ApplicationDbContext context)
        {
            var user = new User
            {
                Name = "Comment User",
                Email = $"comment{Guid.NewGuid()}@example.com",
                PasswordHash = "hash",
                Role = Domain.Enums.UserRole.User,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var team = new Team
            {
                Name = "Comment Team",
                Description = "Comment Team Description",
                CreatedById = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Teams.Add(team);
            await context.SaveChangesAsync();

            var task = new TaskItem
            {
                Title = "Comment Task",
                Description = "Task for comments",
                Status = Domain.Enums.TaskItemStatus.ToDo,
                Priority = Domain.Enums.TaskPriority.Medium,
                AssignedToId = user.Id,
                TeamId = team.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Tasks.Add(task);
            await context.SaveChangesAsync();

            return (user, team, task);
        }

        [Fact]
        public async Task AddAsync_ShouldAddComment()
        {
            await using var context = CreateContext();

            var (user, _, task) =
                await SeedDataAsync(context);

            var repository =
                new CommentRepository(context);

            var comment = new Comment
            {
                Content = "This is a test comment.",
                UserId = user.Id,
                TaskItemId = task.Id,
                CreatedAt = DateTime.UtcNow
            };

            var result =
                await repository.AddAsync(comment);

            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal(
                "This is a test comment.",
                result.Content);
            Assert.Equal(user.Id, result.UserId);
            Assert.Equal(task.Id, result.TaskItemId);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnComment()
        {
            await using var context = CreateContext();

            var (user, _, task) =
                await SeedDataAsync(context);

            var comment = new Comment
            {
                Content = "Test comment",
                UserId = user.Id,
                TaskItemId = task.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Comments.Add(comment);
            await context.SaveChangesAsync();

            var repository =
                new CommentRepository(context);

            var result =
                await repository.GetByIdAsync(comment.Id);

            Assert.NotNull(result);
            Assert.Equal(comment.Id, result.Id);
            Assert.Equal(
                "Test comment",
                result.Content);

            Assert.NotNull(result.User);
            Assert.Equal(
                user.Id,
                result.User.Id);

            Assert.NotNull(result.TaskItem);
            Assert.Equal(
                task.Id,
                result.TaskItem.Id);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenCommentDoesNotExist()
        {
            await using var context = CreateContext();

            var repository =
                new CommentRepository(context);

            var result =
                await repository.GetByIdAsync(999999);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByTaskIdAsync_ShouldReturnCommentsForTask()
        {
            await using var context = CreateContext();

            var (user, _, task) =
                await SeedDataAsync(context);

            var comment1 = new Comment
            {
                Content = "First comment",
                UserId = user.Id,
                TaskItemId = task.Id,
                CreatedAt = DateTime.UtcNow.AddMinutes(-10)
            };

            var comment2 = new Comment
            {
                Content = "Second comment",
                UserId = user.Id,
                TaskItemId = task.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Comments.AddRange(
                comment1,
                comment2);

            await context.SaveChangesAsync();

            var repository =
                new CommentRepository(context);

            var result =
                (await repository.GetByTaskIdAsync(task.Id))
                .ToList();

            Assert.Equal(2, result.Count);

            Assert.Equal(
                "First comment",
                result[0].Content);

            Assert.Equal(
                "Second comment",
                result[1].Content);
        }

        [Fact]
        public async Task GetByTaskIdAsync_ShouldReturnEmpty_WhenTaskHasNoComments()
        {
            await using var context = CreateContext();

            var (_, _, task) =
                await SeedDataAsync(context);

            var repository =
                new CommentRepository(context);

            var result =
                (await repository.GetByTaskIdAsync(task.Id))
                .ToList();

            Assert.Empty(result);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateComment()
        {
            await using var context = CreateContext();

            var (user, _, task) =
                await SeedDataAsync(context);

            var comment = new Comment
            {
                Content = "Old comment",
                UserId = user.Id,
                TaskItemId = task.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Comments.Add(comment);
            await context.SaveChangesAsync();

            var repository =
                new CommentRepository(context);

            comment.Content = "Updated comment";

            await repository.UpdateAsync(comment);

            var updatedComment =
                await context.Comments
                    .FirstOrDefaultAsync(
                        x => x.Id == comment.Id);

            Assert.NotNull(updatedComment);
            Assert.Equal(
                "Updated comment",
                updatedComment.Content);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteComment()
        {
            await using var context = CreateContext();

            var (user, _, task) =
                await SeedDataAsync(context);

            var comment = new Comment
            {
                Content = "Delete comment",
                UserId = user.Id,
                TaskItemId = task.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Comments.Add(comment);
            await context.SaveChangesAsync();

            var repository =
                new CommentRepository(context);

            await repository.DeleteAsync(comment);

            var deletedComment =
                await context.Comments
                    .FirstOrDefaultAsync(
                        x => x.Id == comment.Id);

            Assert.Null(deletedComment);
        }
    }
}