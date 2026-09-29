using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface ITeamRepository
    {
        Task<Team?> GetByIdAsync(int id);

        Task<IEnumerable<Team>> GetAllAsync();

        Task<IEnumerable<Team>> GetByMemberUserIdAsync(int userId);

        Task<Team> AddAsync(Team team);

        Task UpdateAsync(Team team);

        Task DeleteAsync(Team team);
    }
}