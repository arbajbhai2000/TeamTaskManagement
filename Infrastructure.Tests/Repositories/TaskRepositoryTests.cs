using Domain.Entities;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Repositories
{
    public class TaskRepositoryTests
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

        private static async Task<(User User, Team Team)> SeedUserAndTeamAsync(
            ApplicationDbContext context)
        {
            var user = new User
            {
                Name = "Task User",
                Email = $"taskuser{Guid.NewGuid()}@example.com",
                PasswordHash = "hash",
                Role = Domain.Enums.UserRole.User,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var team = new Team
            {
                Name = "Task Team",
                Description = "Task Team Description",
                CreatedById = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Teams.Add(team);
            await context.SaveChangesAsync();

            return (user, team);
        }

        private static TaskItem CreateTask(
            int userId,
            int teamId,
            string title = "Test Task")
        {
            return new TaskItem
            {
                Title = title,
                Description = "Test task description",
                Status = Domain.Enums.TaskItemStatus.ToDo,
                Priority = Domain.Enums.TaskPriority.Medium,
                DueDate = DateTime.UtcNow.AddDays(7),
                AssignedToId = userId,
                TeamId = teamId,
                CreatedAt = DateTime.UtcNow
            };
        }

        [Fact]
        public async Task AddAsync_ShouldAddTask()
        {
            await using var context = CreateContext();

            var (user, team) =
                await SeedUserAndTeamAsync(context);

            var repository =
                new TaskRepository(context);

            var task =
                CreateTask(user.Id, team.Id);

            var result =
                await repository.AddAsync(task);

            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("Test Task", result.Title);
            Assert.Equal(user.Id, result.AssignedToId);
            Assert.Equal(team.Id, result.TeamId);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnTask()
        {
            await using var context = CreateContext();

            var (user, team) =
                await SeedUserAndTeamAsync(context);

            var task =
                CreateTask(
                    user.Id,
                    team.Id,
                    "Get Task");

            context.Tasks.Add(task);
            await context.SaveChangesAsync();

            var repository =
                new TaskRepository(context);

            var result =
                await repository.GetByIdAsync(task.Id);

            Assert.NotNull(result);
            Assert.Equal(task.Id, result.Id);
            Assert.Equal("Get Task", result.Title);

            Assert.NotNull(result.AssignedTo);
            Assert.Equal(
                user.Id,
                result.AssignedTo.Id);

            Assert.NotNull(result.Team);
            Assert.Equal(
                team.Id,
                result.Team.Id);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenTaskDoesNotExist()
        {
            await using var context = CreateContext();

            var repository =
                new TaskRepository(context);

            var result =
                await repository.GetByIdAsync(999999);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnTasksOrderedByCreatedAtDescending()
        {
            await using var context = CreateContext();

            var (user, team) =
                await SeedUserAndTeamAsync(context);

            var olderTask =
                CreateTask(
                    user.Id,
                    team.Id,
                    "Older Task");

            olderTask.CreatedAt =
                DateTime.UtcNow.AddDays(-2);

            var newerTask =
                CreateTask(
                    user.Id,
                    team.Id,
                    "Newer Task");

            newerTask.CreatedAt =
                DateTime.UtcNow;

            context.Tasks.AddRange(
                olderTask,
                newerTask);

            await context.SaveChangesAsync();

            var repository =
                new TaskRepository(context);

            var result =
                (await repository.GetAllAsync()).ToList();

            Assert.Equal(2, result.Count);

            Assert.Equal(
                "Newer Task",
                result[0].Title);

            Assert.Equal(
                "Older Task",
                result[1].Title);
        }

        [Fact]
        public async Task GetByTeamIdAsync_ShouldReturnOnlyTasksForSpecifiedTeam()
        {
            await using var context = CreateContext();

            var user = new User
            {
                Name = "Task User",
                Email = $"user{Guid.NewGuid()}@example.com",
                PasswordHash = "hash",
                Role = Domain.Enums.UserRole.User,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var team1 = new Team
            {
                Name = "Team One",
                Description = "Team One",
                CreatedById = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            var team2 = new Team
            {
                Name = "Team Two",
                Description = "Team Two",
                CreatedById = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Teams.AddRange(
                team1,
                team2);

            await context.SaveChangesAsync();

            var task1 =
                CreateTask(
                    user.Id,
                    team1.Id,
                    "Team One Task");

            var task2 =
                CreateTask(
                    user.Id,
                    team2.Id,
                    "Team Two Task");

            context.Tasks.AddRange(
                task1,
                task2);

            await context.SaveChangesAsync();

            var repository =
                new TaskRepository(context);

            var result =
                (await repository.GetByTeamIdAsync(team1.Id))
                .ToList();

            Assert.Single(result);

            Assert.Equal(
                "Team One Task",
                result[0].Title);

            Assert.Equal(
                team1.Id,
                result[0].TeamId);
        }

        [Fact]
        public async Task GetByAssignedUserIdAsync_ShouldReturnOnlyAssignedUserTasks()
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

            context.Users.AddRange(
                user1,
                user2);

            await context.SaveChangesAsync();

            var team = new Team
            {
                Name = "Development Team",
                Description = "Development",
                CreatedById = user1.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Teams.Add(team);
            await context.SaveChangesAsync();

            var user1Task =
                CreateTask(
                    user1.Id,
                    team.Id,
                    "User One Task");

            var user2Task =
                CreateTask(
                    user2.Id,
                    team.Id,
                    "User Two Task");

            context.Tasks.AddRange(
                user1Task,
                user2Task);

            await context.SaveChangesAsync();

            var repository =
                new TaskRepository(context);

            var result =
                (await repository.GetByAssignedUserIdAsync(user1.Id))
                .ToList();

            Assert.Single(result);

            Assert.Equal(
                "User One Task",
                result[0].Title);

            Assert.Equal(
                user1.Id,
                result[0].AssignedToId);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateTask()
        {
            await using var context = CreateContext();

            var (user, team) =
                await SeedUserAndTeamAsync(context);

            var task =
                CreateTask(
                    user.Id,
                    team.Id,
                    "Old Task");

            context.Tasks.Add(task);
            await context.SaveChangesAsync();

            var repository =
                new TaskRepository(context);

            task.Title = "Updated Task";
            task.Status =
                Domain.Enums.TaskItemStatus.InProgress;
            task.Priority =
                Domain.Enums.TaskPriority.High;

            await repository.UpdateAsync(task);

            var updatedTask =
                await context.Tasks
                    .FirstOrDefaultAsync(
                        x => x.Id == task.Id);

            Assert.NotNull(updatedTask);

            Assert.Equal(
                "Updated Task",
                updatedTask.Title);

            Assert.Equal(
                Domain.Enums.TaskItemStatus.InProgress,
                updatedTask.Status);

            Assert.Equal(
                Domain.Enums.TaskPriority.High,
                updatedTask.Priority);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteTask()
        {
            await using var context = CreateContext();

            var (user, team) =
                await SeedUserAndTeamAsync(context);

            var task =
                CreateTask(
                    user.Id,
                    team.Id,
                    "Delete Task");

            context.Tasks.Add(task);
            await context.SaveChangesAsync();

            var repository =
                new TaskRepository(context);

            await repository.DeleteAsync(task);

            var deletedTask =
                await context.Tasks
                    .FirstOrDefaultAsync(
                        x => x.Id == task.Id);

            Assert.Null(deletedTask);
        }
    }
}