using Domain.Entities;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Repositories
{
    public class TeamMemberRepositoryTests
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
                Name = "Test User",
                Email = $"user{Guid.NewGuid()}@example.com",
                PasswordHash = "hash",
                Role = Domain.Enums.UserRole.User,
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

            return (user, team);
        }

        [Fact]
        public async Task AddAsync_ShouldAddTeamMember()
        {
            await using var context = CreateContext();

            var (user, team) =
                await SeedUserAndTeamAsync(context);

            var repository =
                new TeamMemberRepository(context);

            var teamMember = new TeamMember
            {
                TeamId = team.Id,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            var result =
                await repository.AddAsync(teamMember);

            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal(team.Id, result.TeamId);
            Assert.Equal(user.Id, result.UserId);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnTeamMember()
        {
            await using var context = CreateContext();

            var (user, team) =
                await SeedUserAndTeamAsync(context);

            var teamMember = new TeamMember
            {
                TeamId = team.Id,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.TeamMembers.Add(teamMember);
            await context.SaveChangesAsync();

            var repository =
                new TeamMemberRepository(context);

            var result =
                await repository.GetByIdAsync(teamMember.Id);

            Assert.NotNull(result);
            Assert.Equal(teamMember.Id, result.Id);
            Assert.Equal(team.Id, result.TeamId);
            Assert.Equal(user.Id, result.UserId);

            Assert.NotNull(result.User);
            Assert.Equal(user.Name, result.User.Name);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenTeamMemberDoesNotExist()
        {
            await using var context = CreateContext();

            var repository =
                new TeamMemberRepository(context);

            var result =
                await repository.GetByIdAsync(999999);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByTeamIdAsync_ShouldReturnMembersForTeam()
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

            var user1 = new User
            {
                Name = "User One",
                Email = "user1@example.com",
                PasswordHash = "hash",
                Role = Domain.Enums.UserRole.User,
                CreatedAt = DateTime.UtcNow
            };

            var user2 = new User
            {
                Name = "User Two",
                Email = "user2@example.com",
                PasswordHash = "hash",
                Role = Domain.Enums.UserRole.User,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.AddRange(
                creator,
                user1,
                user2);

            await context.SaveChangesAsync();

            var team = new Team
            {
                Name = "Development Team",
                Description = "Development",
                CreatedById = creator.Id,
                CreatedAt = DateTime.UtcNow
            };

            var anotherTeam = new Team
            {
                Name = "Another Team",
                Description = "Another",
                CreatedById = creator.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Teams.AddRange(
                team,
                anotherTeam);

            await context.SaveChangesAsync();

            var member1 = new TeamMember
            {
                TeamId = team.Id,
                UserId = user1.Id,
                CreatedAt = DateTime.UtcNow
            };

            var member2 = new TeamMember
            {
                TeamId = team.Id,
                UserId = user2.Id,
                CreatedAt = DateTime.UtcNow
            };

            var otherMember = new TeamMember
            {
                TeamId = anotherTeam.Id,
                UserId = user2.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.TeamMembers.AddRange(
                member1,
                member2,
                otherMember);

            await context.SaveChangesAsync();

            var repository =
                new TeamMemberRepository(context);

            var result =
                (await repository.GetByTeamIdAsync(team.Id))
                .ToList();

            Assert.Equal(2, result.Count);

            Assert.Contains(
                result,
                x => x.UserId == user1.Id);

            Assert.Contains(
                result,
                x => x.UserId == user2.Id);

            Assert.All(
                result,
                x => Assert.Equal(team.Id, x.TeamId));
        }

        [Fact]
        public async Task GetByTeamIdAsync_ShouldReturnEmpty_WhenTeamHasNoMembers()
        {
            await using var context = CreateContext();

            var (user, team) =
                await SeedUserAndTeamAsync(context);

            var repository =
                new TeamMemberRepository(context);

            var result =
                (await repository.GetByTeamIdAsync(team.Id))
                .ToList();

            Assert.Empty(result);
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnTrue_WhenMemberExists()
        {
            await using var context = CreateContext();

            var (user, team) =
                await SeedUserAndTeamAsync(context);

            context.TeamMembers.Add(
                new TeamMember
                {
                    TeamId = team.Id,
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow
                });

            await context.SaveChangesAsync();

            var repository =
                new TeamMemberRepository(context);

            var result =
                await repository.ExistsAsync(
                    team.Id,
                    user.Id);

            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnFalse_WhenMemberDoesNotExist()
        {
            await using var context = CreateContext();

            var (user, team) =
                await SeedUserAndTeamAsync(context);

            var repository =
                new TeamMemberRepository(context);

            var result =
                await repository.ExistsAsync(
                    team.Id,
                    user.Id);

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteTeamMember()
        {
            await using var context = CreateContext();

            var (user, team) =
                await SeedUserAndTeamAsync(context);

            var teamMember = new TeamMember
            {
                TeamId = team.Id,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.TeamMembers.Add(teamMember);
            await context.SaveChangesAsync();

            var repository =
                new TeamMemberRepository(context);

            await repository.DeleteAsync(teamMember);

            var deletedMember =
                await context.TeamMembers
                    .FirstOrDefaultAsync(
                        x => x.Id == teamMember.Id);

            Assert.Null(deletedMember);
        }
    }
}