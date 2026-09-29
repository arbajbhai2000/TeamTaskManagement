using Application.DTOs.Teams;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;

namespace Application.Services
{
    public class TeamService : ITeamService
    {
        private readonly ITeamRepository _teamRepository;

        public TeamService(ITeamRepository teamRepository)
        {
            _teamRepository = teamRepository;
        }

        public async Task<TeamResponse?> GetByIdAsync(int id)
        {
            var team = await _teamRepository.GetByIdAsync(id);
            if (team == null)
                return null;

            return MapToResponse(team);
        }

        public async Task<IEnumerable<TeamResponse>> GetAllAsync()
        {
            var teams = await _teamRepository.GetAllAsync();
            return teams.Select(MapToResponse);
        }

        public async Task<IEnumerable<TeamResponse>> GetByMemberUserIdAsync(int userId)
        {
            var teams = await _teamRepository.GetByMemberUserIdAsync(userId);
            return teams.Select(MapToResponse);
        }

        public async Task<TeamResponse> CreateAsync(
            CreateTeamRequest request,
            int createdById)
        {
            var team = new Team
            {
                Name = request.Name,
                Description = request.Description,
                CreatedById = createdById,
                CreatedAt = DateTime.UtcNow
            };

            var createdTeam = await _teamRepository.AddAsync(team);
            return MapToResponse(createdTeam);
        }

        public async Task UpdateAsync(
            int id,
            UpdateTeamRequest request)
        {
            var team = await _teamRepository.GetByIdAsync(id);

            if (team == null)
                throw new KeyNotFoundException("Team not found.");

            team.Name = request.Name;
            team.Description = request.Description;
            team.UpdatedAt = DateTime.UtcNow;

            await _teamRepository.UpdateAsync(team);
        }

        public async Task DeleteAsync(int id)
        {
            var team = await _teamRepository.GetByIdAsync(id);

            if (team == null)
                throw new KeyNotFoundException("Team not found.");

            await _teamRepository.DeleteAsync(team);
        }

        private static TeamResponse MapToResponse(Team team)
        {
            return new TeamResponse
            {
                Id = team.Id,
                Name = team.Name,
                Description = team.Description,
                CreatedById = team.CreatedById,
                CreatedByName = team.CreatedBy?.Name ?? string.Empty,
                MemberCount = team.Members?.Count ?? 0,
                CreatedAt = team.CreatedAt
            };
        }
    }
}