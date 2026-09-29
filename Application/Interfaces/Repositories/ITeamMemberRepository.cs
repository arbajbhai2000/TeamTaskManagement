using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface ITeamMemberRepository
    {
        Task<TeamMember?> GetByIdAsync(int id);

        Task<IEnumerable<TeamMember>> GetByTeamIdAsync(int teamId);

        Task<bool> ExistsAsync(int teamId, int userId);

        Task<TeamMember> AddAsync(TeamMember teamMember);

        Task DeleteAsync(TeamMember teamMember);
    }
}