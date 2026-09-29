using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.TeamMembers;

namespace Application.Interfaces.Services
{
    public interface ITeamMemberService
    {
        Task<TeamMemberResponse> AddMemberAsync(
            int teamId,
            AddTeamMemberRequest request);

        Task<IEnumerable<TeamMemberResponse>> GetMembersAsync(
            int teamId);

        Task RemoveMemberAsync(int teamId, int teamMemberId);
    }
}