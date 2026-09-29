using Domain.Entities;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Repositories
{
    public class TeamRepositoryTests
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
        public async Task AddAsync_ShouldAddTeam()
        {
            await using var context = CreateContext();

            var user = new User
            {
                Name = "Admin User",
                Email = "admin@example.com",
                PasswordHash = "hash",
                Role = Domain.Enums.UserRole.Admin,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var repository = new TeamRepository(context);

            var team = new Team
            {
                Name = "Development Team",
                Description = "Development team",
                CreatedById = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            var result = await repository.AddAsync(team);

            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("Development Team", result.Name);
            Assert.Equal(user.Id, result.CreatedById);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnTeam()
        {
            await using var context = CreateContext();

            var user = new User
            {
                Name = "Admin User",
                Email = "admin@example.com",
                PasswordHash = "hash",
                Role = Domain.Enums.UserRole.Admin,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var team = new Team
            {
                Name = "Development Team",
                Description = "Development team",
                CreatedById = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Teams.Add(team);
            await context.SaveChangesAsync();

            var repository = new TeamRepository(context);

            var result = await repository.GetByIdAsync(team.Id);

            Assert.NotNull(result);
            Assert.Equal(team.Id, result.Id);
            Assert.Equal("Development Team", result.Name);
            Assert.Equal("Development team", result.Description);

            Assert.NotNull(result.CreatedBy);
            Assert.Equal(user.Id, result.CreatedBy.Id);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenTeamDoesNotExist()
        {
            await using var context = CreateContext();

            var repository = new TeamRepository(context);

            var result = await repository.GetByIdAsync(999999);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnTeamsOrderedByCreatedAtDescending()
        {
            await using var context = CreateContext();

            var user = new User
            {
                Name = "Admin User",
                Email = "admin@example.com",
                PasswordHash = "hash",
                Role = Domain.Enums.UserRole.Admin,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var olderTeam = new Team
            {
                Name = "Older Team",
                Description = "Older",
                CreatedById = user.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            };

            var newerTeam = new Team
            {
                Name = "Newer Team",
                Description = "Newer",
                CreatedById = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Teams.AddRange(
                olderTeam,
                newerTeam);

            await context.SaveChangesAsync();

            var repository = new TeamRepository(context);

            var result =
                (await repository.GetAllAsync()).ToList();

            Assert.Equal(2, result.Count);

            Assert.Equal(
                "Newer Team",
                result[0].Name);

            Assert.Equal(
                "Older Team",
                result[1].Name);
        }

        [Fact]
        public async Task GetAllAsync_ShouldIncludeCreatedBy()
        {
            await using var context = CreateContext();

            var user = new User
            {
                Name = "Team Creator",
                Email = "creator@example.com",
                PasswordHash = "hash",
                Role = Domain.Enums.UserRole.Admin,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var team = new Team
            {
                Name = "Test Team",
                Description = "Test Description",
                CreatedById = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Teams.Add(team);
            await context.SaveChangesAsync();

            var repository = new TeamRepository(context);

            var result =
                (await repository.GetAllAsync()).ToList();

            Assert.Single(result);

            Assert.NotNull(result[0].CreatedBy);
            Assert.Equal(
                "Team Creator",
                result[0].CreatedBy.Name);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldIncludeMembers()
        {
            await using var context = CreateContext();

            var creator = new User
            {
                Name = "Creator",
                Email = "creator@example.com",
                PasswordHash = "hash",
                Role = Domain.Enums.UserRole.Admin,
                CreatedAt = DateTime.UtcNow
            };

            var memberUser = new User
            {
                Name = "Member",
                Email = "member@example.com",
                PasswordHash = "hash",
                Role = Domain.Enums.UserRole.User,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.AddRange(
                creator,
                memberUser);

            await context.SaveChangesAsync();

            var team = new Team
            {
                Name = "Development Team",
                Description = "Development",
                CreatedById = creator.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Teams.Add(team);
            await context.SaveChangesAsync();

            var member = new TeamMember
            {
                TeamId = team.Id,
                UserId = memberUser.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.TeamMembers.Add(member);
            await context.SaveChangesAsync();

            var repository = new TeamRepository(context);

            var result =
                await repository.GetByIdAsync(team.Id);

            Assert.NotNull(result);
            Assert.NotNull(result.Members);
            Assert.Single(result.Members);

            Assert.Equal(
                memberUser.Id,
                result.Members.First().UserId);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateTeam()
        {
            await using var context = CreateContext();

            var user = new User
            {
                Name = "Admin User",
                Email = "admin@example.com",
                PasswordHash = "hash",
                Role = Domain.Enums.UserRole.Admin,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var team = new Team
            {
                Name = "Old Team Name",
                Description = "Old Description",
                CreatedById = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Teams.Add(team);
            await context.SaveChangesAsync();

            var repository = new TeamRepository(context);

            team.Name = "Updated Team Name";
            team.Description = "Updated Description";

            await repository.UpdateAsync(team);

            var updatedTeam =
                await context.Teams
                    .FirstOrDefaultAsync(
                        x => x.Id == team.Id);

            Assert.NotNull(updatedTeam);

            Assert.Equal(
                "Updated Team Name",
                updatedTeam.Name);

            Assert.Equal(
                "Updated Description",
                updatedTeam.Description);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteTeam()
        {
            await using var context = CreateContext();

            var user = new User
            {
                Name = "Admin User",
                Email = "admin@example.com",
                PasswordHash = "hash",
                Role = Domain.Enums.UserRole.Admin,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var team = new Team
            {
                Name = "Delete Team",
                Description = "Delete Description",
                CreatedById = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Teams.Add(team);
            await context.SaveChangesAsync();

            var repository = new TeamRepository(context);

            await repository.DeleteAsync(team);

            var deletedTeam =
                await context.Teams
                    .FirstOrDefaultAsync(
                        x => x.Id == team.Id);

            Assert.Null(deletedTeam);
        }
    }
}