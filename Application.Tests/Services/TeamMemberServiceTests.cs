using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.TeamMembers;
using Application.Interfaces.Repositories;
using Application.Services;
using Domain.Entities;
using Moq;

namespace Application.Tests.Services
{
    public class TeamMemberServiceTests
    {
        private readonly Mock<ITeamMemberRepository> _teamMemberRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ITeamRepository> _teamRepositoryMock;
        private readonly TeamMemberService _teamMemberService;

        public TeamMemberServiceTests()
        {
            _teamMemberRepositoryMock = new Mock<ITeamMemberRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _teamRepositoryMock = new Mock<ITeamRepository>();

            _teamMemberService = new TeamMemberService(
                _teamMemberRepositoryMock.Object,
                _userRepositoryMock.Object,
                _teamRepositoryMock.Object);
        }

        [Fact]
        public async Task AddMemberAsync_TeamAndUserExist_AddsMember()
        {
            // Arrange
            var team = new Team
            {
                Id = 1,
                Name = "Development Team"
            };

            var user = new User
            {
                Id = 2,
                Name = "John",
                Email = "john@test.com"
            };

            var request = new AddTeamMemberRequest
            {
                UserId = 2
            };

            var createdMember = new TeamMember
            {
                Id = 10,
                TeamId = 1,
                UserId = 2
            };

            _teamRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(team);

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(2))
                .ReturnsAsync(user);

            _teamMemberRepositoryMock
                .Setup(x => x.ExistsAsync(1, 2))
                .ReturnsAsync(false);

            _teamMemberRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<TeamMember>()))
                .ReturnsAsync(createdMember);

            // Act
            var result = await _teamMemberService.AddMemberAsync(1, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.Id);
            Assert.Equal(1, result.TeamId);
            Assert.Equal(2, result.UserId);
            Assert.Equal("John", result.UserName);
            Assert.Equal("john@test.com", result.UserEmail);

            _teamMemberRepositoryMock.Verify(
                x => x.AddAsync(It.Is<TeamMember>(m =>
                    m.TeamId == 1 &&
                    m.UserId == 2)),
                Times.Once);
        }

        [Fact]
        public async Task AddMemberAsync_TeamDoesNotExist_ThrowsKeyNotFoundException()
        {
            // Arrange
            var request = new AddTeamMemberRequest
            {
                UserId = 2
            };

            _teamRepositoryMock
                .Setup(x => x.GetByIdAsync(99))
                .ReturnsAsync((Team?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _teamMemberService.AddMemberAsync(99, request));

            Assert.Equal("Team not found.", exception.Message);

            _userRepositoryMock.Verify(
                x => x.GetByIdAsync(It.IsAny<int>()),
                Times.Never);

            _teamMemberRepositoryMock.Verify(
                x => x.AddAsync(It.IsAny<TeamMember>()),
                Times.Never);
        }

        [Fact]
        public async Task AddMemberAsync_UserDoesNotExist_ThrowsKeyNotFoundException()
        {
            // Arrange
            var team = new Team
            {
                Id = 1,
                Name = "Development Team"
            };

            var request = new AddTeamMemberRequest
            {
                UserId = 99
            };

            _teamRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(team);

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(99))
                .ReturnsAsync((User?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _teamMemberService.AddMemberAsync(1, request));

            Assert.Equal("User not found.", exception.Message);

            _teamMemberRepositoryMock.Verify(
                x => x.ExistsAsync(It.IsAny<int>(), It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task AddMemberAsync_UserAlreadyMember_ThrowsInvalidOperationException()
        {
            // Arrange
            var team = new Team
            {
                Id = 1,
                Name = "Development Team"
            };

            var user = new User
            {
                Id = 2,
                Name = "John",
                Email = "john@test.com"
            };

            var request = new AddTeamMemberRequest
            {
                UserId = 2
            };

            _teamRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(team);

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(2))
                .ReturnsAsync(user);

            _teamMemberRepositoryMock
                .Setup(x => x.ExistsAsync(1, 2))
                .ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _teamMemberService.AddMemberAsync(1, request));

            Assert.Equal(
                "User is already a member of this team.",
                exception.Message);

            _teamMemberRepositoryMock.Verify(
                x => x.AddAsync(It.IsAny<TeamMember>()),
                Times.Never);
        }

        [Fact]
        public async Task GetMembersAsync_TeamExists_ReturnsMembers()
        {
            // Arrange
            var team = new Team
            {
                Id = 1,
                Name = "Development Team"
            };

            var members = new List<TeamMember>
            {
                new TeamMember
                {
                    Id = 10,
                    TeamId = 1,
                    UserId = 2,
                    User = new User
                    {
                        Id = 2,
                        Name = "John",
                        Email = "john@test.com"
                    }
                },
                new TeamMember
                {
                    Id = 11,
                    TeamId = 1,
                    UserId = 3,
                    User = new User
                    {
                        Id = 3,
                        Name = "Sarah",
                        Email = "sarah@test.com"
                    }
                }
            };

            _teamRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(team);

            _teamMemberRepositoryMock
                .Setup(x => x.GetByTeamIdAsync(1))
                .ReturnsAsync(members);

            // Act
            var result = (await _teamMemberService.GetMembersAsync(1))
                .ToList();

            // Assert
            Assert.Equal(2, result.Count);

            Assert.Equal(10, result[0].Id);
            Assert.Equal("John", result[0].UserName);
            Assert.Equal("john@test.com", result[0].UserEmail);

            Assert.Equal(11, result[1].Id);
            Assert.Equal("Sarah", result[1].UserName);
            Assert.Equal("sarah@test.com", result[1].UserEmail);

            _teamMemberRepositoryMock.Verify(
                x => x.GetByTeamIdAsync(1),
                Times.Once);
        }

        [Fact]
        public async Task GetMembersAsync_TeamDoesNotExist_ThrowsKeyNotFoundException()
        {
            // Arrange
            _teamRepositoryMock
                .Setup(x => x.GetByIdAsync(99))
                .ReturnsAsync((Team?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _teamMemberService.GetMembersAsync(99));

            Assert.Equal("Team not found.", exception.Message);

            _teamMemberRepositoryMock.Verify(
                x => x.GetByTeamIdAsync(It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task RemoveMemberAsync_MemberExists_DeletesMember()
        {
            // Arrange
            var teamMember = new TeamMember
            {
                Id = 10,
                TeamId = 1,
                UserId = 2
            };

            _teamMemberRepositoryMock
                .Setup(x => x.GetByIdAsync(10))
                .ReturnsAsync(teamMember);

            // Act
            await _teamMemberService.RemoveMemberAsync(1, 10);

            // Assert
            _teamMemberRepositoryMock.Verify(
                x => x.DeleteAsync(teamMember),
                Times.Once);
        }

        [Fact]
        public async Task RemoveMemberAsync_MemberBelongsToDifferentTeam_ThrowsInvalidOperationException()
        {
            // Arrange
            var teamMember = new TeamMember
            {
                Id = 10,
                TeamId = 5,
                UserId = 2
            };

            _teamMemberRepositoryMock
                .Setup(x => x.GetByIdAsync(10))
                .ReturnsAsync(teamMember);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _teamMemberService.RemoveMemberAsync(1, 10));

            Assert.Equal(
                "Team member does not belong to this team.",
                exception.Message);

            _teamMemberRepositoryMock.Verify(
                x => x.DeleteAsync(It.IsAny<TeamMember>()),
                Times.Never);
        }

        [Fact]
        public async Task RemoveMemberAsync_MemberDoesNotExist_ThrowsKeyNotFoundException()
        {
            // Arrange
            _teamMemberRepositoryMock
                .Setup(x => x.GetByIdAsync(99))
                .ReturnsAsync((TeamMember?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _teamMemberService.RemoveMemberAsync(1,99));

            Assert.Equal("Team member not found.", exception.Message);

            _teamMemberRepositoryMock.Verify(
                x => x.DeleteAsync(It.IsAny<TeamMember>()),
                Times.Never);
        }
    }
}