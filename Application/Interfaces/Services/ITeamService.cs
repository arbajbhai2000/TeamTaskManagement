using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Teams;

namespace Application.Interfaces.Services
{
    public interface ITeamService
    {
        Task<TeamResponse?> GetByIdAsync(int id);
        Task<IEnumerable<TeamResponse>> GetAllAsync();

        Task<IEnumerable<TeamResponse>> GetByMemberUserIdAsync(int userId);
        Task<TeamResponse> CreateAsync(CreateTeamRequest request, int createdById);
        Task UpdateAsync(int id, UpdateTeamRequest request);
        Task DeleteAsync(int id);
    }
}