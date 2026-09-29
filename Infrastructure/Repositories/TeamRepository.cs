using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class TeamRepository : ITeamRepository
    {
        private readonly ApplicationDbContext _context;

        public TeamRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Team?> GetByIdAsync(int id)
        {
            return await _context.Teams
                .Include(x => x.CreatedBy)
                .Include(x => x.Members)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<Team>> GetAllAsync()
        {
            return await _context.Teams
                .Include(x => x.CreatedBy)
                .Include(x => x.Members)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Team>> GetByMemberUserIdAsync(int userId)
        {
            return await _context.Teams
                .Include(x => x.CreatedBy)
                .Include(x => x.Members)
                .Where(x => x.Members.Any(m => m.UserId == userId))
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<Team> AddAsync(Team team)
        {
            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(team.Id)
                ?? team;
        }

        public async Task UpdateAsync(Team team)
        {
            _context.Teams.Update(team);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Team team)
        {
            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();
        }
    }
}