using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.TeamMembers;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;

namespace Application.Services
{
    public class TeamMemberService : ITeamMemberService
    {
        private readonly ITeamMemberRepository _teamMemberRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITeamRepository _teamRepository;

        public TeamMemberService(
            ITeamMemberRepository teamMemberRepository,
            IUserRepository userRepository,
            ITeamRepository teamRepository)
        {
            _teamMemberRepository = teamMemberRepository;
            _userRepository = userRepository;
            _teamRepository = teamRepository;
        }

        public async Task<TeamMemberResponse> AddMemberAsync(
            int teamId,
            AddTeamMemberRequest request)
        {
            var team = await _teamRepository.GetByIdAsync(teamId);

            if (team == null)
                throw new KeyNotFoundException("Team not found.");

            var user = await _userRepository.GetByIdAsync(request.UserId);

            if (user == null)
                throw new KeyNotFoundException("User not found.");

            var exists = await _teamMemberRepository
                .ExistsAsync(teamId, request.UserId);

            if (exists)
                throw new InvalidOperationException(
                    "User is already a member of this team.");

            var teamMember = new TeamMember
            {
                TeamId = teamId,
                UserId = request.UserId,
                CreatedAt = DateTime.UtcNow
            };

            var createdMember =
                await _teamMemberRepository.AddAsync(teamMember);

            return new TeamMemberResponse
            {
                Id = createdMember.Id,
                TeamId = createdMember.TeamId,
                UserId = createdMember.UserId,
                UserName = user.Name,
                UserEmail = user.Email
            };
        }

        public async Task<IEnumerable<TeamMemberResponse>> GetMembersAsync(
            int teamId)
        {
            var team = await _teamRepository.GetByIdAsync(teamId);

            if (team == null)
                throw new KeyNotFoundException("Team not found.");

            var members =
                await _teamMemberRepository.GetByTeamIdAsync(teamId);

            return members.Select(x => new TeamMemberResponse
            {
                Id = x.Id,
                TeamId = x.TeamId,
                UserId = x.UserId,
                UserName = x.User.Name,
                UserEmail = x.User.Email
            });
        }

        public async Task RemoveMemberAsync(
    int teamId,
    int teamMemberId)
        {
            var teamMember =
                await _teamMemberRepository.GetByIdAsync(teamMemberId);

            if (teamMember == null)
                throw new KeyNotFoundException(
                    "Team member not found.");

            if (teamMember.TeamId != teamId)
                throw new InvalidOperationException(
                    "Team member does not belong to this team.");

            await _teamMemberRepository.DeleteAsync(teamMember);
        }
    }
}