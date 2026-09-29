using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Teams;
using Application.Interfaces.Repositories;
using Application.Services;
using Domain.Entities;
using Moq;

namespace Application.Tests.Services
{
    public class TeamServiceTests
    {
        private readonly Mock<ITeamRepository> _teamRepositoryMock;
        private readonly TeamService _teamService;

        public TeamServiceTests()
        {
            _teamRepositoryMock = new Mock<ITeamRepository>();
            _teamService = new TeamService(_teamRepositoryMock.Object);
        }

        [Fact]
        public async Task GetByIdAsync_TeamExists_ReturnsTeamResponse()
        {
            // Arrange
            var team = new Team
            {
                Id = 1,
                Name = "Development Team",
                Description = "Software development team",
                CreatedById = 10,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = new User
                {
                    Id = 10,
                    Name = "Admin User"
                },
                Members = new List<TeamMember>
                {
                    new TeamMember { Id = 1, UserId = 20 },
                    new TeamMember { Id = 2, UserId = 21 }
                }
            };

            _teamRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(team);

            // Act
            var result = await _teamService.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Development Team", result.Name);
            Assert.Equal("Software development team", result.Description);
            Assert.Equal(10, result.CreatedById);
            Assert.Equal("Admin User", result.CreatedByName);
            Assert.Equal(2, result.MemberCount);

            _teamRepositoryMock.Verify(
                x => x.GetByIdAsync(1),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_TeamDoesNotExist_ReturnsNull()
        {
            // Arrange
            _teamRepositoryMock
                .Setup(x => x.GetByIdAsync(99))
                .ReturnsAsync((Team?)null);

            // Act
            var result = await _teamService.GetByIdAsync(99);

            // Assert
            Assert.Null(result);

            _teamRepositoryMock.Verify(
                x => x.GetByIdAsync(99),
                Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllTeams()
        {
            // Arrange
            var teams = new List<Team>
            {
                new Team
                {
                    Id = 1,
                    Name = "Team One",
                    Description = "First team",
                    CreatedById = 10,
                    CreatedAt = DateTime.UtcNow,
                    Members = new List<TeamMember>()
                },
                new Team
                {
                    Id = 2,
                    Name = "Team Two",
                    Description = "Second team",
                    CreatedById = 11,
                    CreatedAt = DateTime.UtcNow,
                    Members = new List<TeamMember>()
                }
            };

            _teamRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(teams);

            // Act
            var result = (await _teamService.GetAllAsync()).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("Team One", result[0].Name);
            Assert.Equal("Team Two", result[1].Name);

            _teamRepositoryMock.Verify(
                x => x.GetAllAsync(),
                Times.Once);
        }

        [Fact]
        public async Task CreateAsync_CreatesTeam_ReturnsTeamResponse()
        {
            // Arrange
            var request = new CreateTeamRequest
            {
                Name = "New Team",
                Description = "New team description"
            };

            var createdTeam = new Team
            {
                Id = 1,
                Name = "New Team",
                Description = "New team description",
                CreatedById = 10,
                CreatedAt = DateTime.UtcNow,
                Members = new List<TeamMember>()
            };

            _teamRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Team>()))
                .ReturnsAsync(createdTeam);

            // Act
            var result = await _teamService.CreateAsync(request, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("New Team", result.Name);
            Assert.Equal("New team description", result.Description);
            Assert.Equal(10, result.CreatedById);

            _teamRepositoryMock.Verify(
                x => x.AddAsync(It.Is<Team>(t =>
                    t.Name == "New Team" &&
                    t.Description == "New team description" &&
                    t.CreatedById == 10)),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_TeamExists_UpdatesTeam()
        {
            // Arrange
            var team = new Team
            {
                Id = 1,
                Name = "Old Team",
                Description = "Old description",
                CreatedById = 10,
                CreatedAt = DateTime.UtcNow
            };

            var request = new UpdateTeamRequest
            {
                Name = "Updated Team",
                Description = "Updated description"
            };

            _teamRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(team);

            // Act
            await _teamService.UpdateAsync(1, request);

            // Assert
            Assert.Equal("Updated Team", team.Name);
            Assert.Equal("Updated description", team.Description);
            Assert.NotNull(team.UpdatedAt);

            _teamRepositoryMock.Verify(
                x => x.UpdateAsync(team),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_TeamDoesNotExist_ThrowsKeyNotFoundException()
        {
            // Arrange
            var request = new UpdateTeamRequest
            {
                Name = "Updated Team",
                Description = "Updated description"
            };

            _teamRepositoryMock
                .Setup(x => x.GetByIdAsync(99))
                .ReturnsAsync((Team?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _teamService.UpdateAsync(99, request));

            Assert.Equal("Team not found.", exception.Message);

            _teamRepositoryMock.Verify(
                x => x.UpdateAsync(It.IsAny<Team>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_TeamExists_DeletesTeam()
        {
            // Arrange
            var team = new Team
            {
                Id = 1,
                Name = "Team To Delete"
            };

            _teamRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(team);

            // Act
            await _teamService.DeleteAsync(1);

            // Assert
            _teamRepositoryMock.Verify(
                x => x.DeleteAsync(team),
                Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_TeamDoesNotExist_ThrowsKeyNotFoundException()
        {
            // Arrange
            _teamRepositoryMock
                .Setup(x => x.GetByIdAsync(99))
                .ReturnsAsync((Team?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _teamService.DeleteAsync(99));

            Assert.Equal("Team not found.", exception.Message);

            _teamRepositoryMock.Verify(
                x => x.DeleteAsync(It.IsAny<Team>()),
                Times.Never);
        }
    }
}