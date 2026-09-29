using Application.DTOs.TeamMembers;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TeamTaskManagementAPI.Controllers
{
    [ApiController]
    [Route("api/teams/{teamId}/members")]
    [Authorize]
    public class TeamMembersController : ControllerBase
    {
        private readonly ITeamMemberService _teamMemberService;
        private readonly ITeamService _teamService;

        public TeamMembersController(
     ITeamMemberService teamMemberService,
     ITeamService teamService)
        {
            _teamMemberService = teamMemberService;
            _teamService = teamService;
        }

        // GET: api/teams/{teamId}/members
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,User")]
        public async Task<IActionResult> GetMembers(int teamId)
        {
            try
            {
                var role = User.FindFirst(ClaimTypes.Role)?.Value;

                if (role == "User")
                {
                    var userIdClaim =
                        User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                    if (!int.TryParse(userIdClaim, out var userId))
                        return Unauthorized();

                    var userTeams =
                        await _teamService.GetByMemberUserIdAsync(userId);

                    if (!userTeams.Any(x => x.Id == teamId))
                        return Forbid();
                }

                var members =
                    await _teamMemberService.GetMembersAsync(teamId);

                return Ok(members);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST: api/teams/{teamId}/members
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> AddMember(
            int teamId,
            [FromBody] AddTeamMemberRequest request)
        {
            try
            {
                var member =
                    await _teamMemberService.AddMemberAsync(
                        teamId,
                        request);

                return Ok(member);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/teams/{teamId}/members/{teamMemberId}
        [HttpDelete("{teamMemberId}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> RemoveMember(
            int teamId,
            int teamMemberId)
        {
            try
            {
                await _teamMemberService.RemoveMemberAsync(
                    teamId,
                    teamMemberId);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}