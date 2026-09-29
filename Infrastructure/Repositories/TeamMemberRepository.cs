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
    public class TeamMemberRepository : ITeamMemberRepository
    {
        private readonly ApplicationDbContext _context;

        public TeamMemberRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TeamMember?> GetByIdAsync(int id)
        {
            return await _context.TeamMembers
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<TeamMember>> GetByTeamIdAsync(int teamId)
        {
            return await _context.TeamMembers
                .Include(x => x.User)
                .Where(x => x.TeamId == teamId)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(int teamId, int userId)
        {
            return await _context.TeamMembers
                .AnyAsync(x => x.TeamId == teamId && x.UserId == userId);
        }

        public async Task<TeamMember> AddAsync(TeamMember teamMember)
        {
            await _context.TeamMembers.AddAsync(teamMember);
            await _context.SaveChangesAsync();

            return teamMember;
        }

        public async Task DeleteAsync(TeamMember teamMember)
        {
            _context.TeamMembers.Remove(teamMember);
            await _context.SaveChangesAsync();
        }
    }
}