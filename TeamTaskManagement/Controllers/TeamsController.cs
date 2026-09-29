using Application.DTOs.Teams;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TeamTaskManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TeamsController : ControllerBase
    {
        private readonly ITeamService _teamService;

        public TeamsController(ITeamService teamService)
        {
            _teamService = teamService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,User")]
        public async Task<IActionResult> GetAll()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            if (role == "User")
            {
                var userTeams =
                    await _teamService.GetByMemberUserIdAsync(userId);

                return Ok(userTeams);
            }

            var teams = await _teamService.GetAllAsync();

            return Ok(teams);
        }


        // Admin and Manager can view any team.
        // User can view only teams they belong to.
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager,User")]
        public async Task<IActionResult> GetById(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var team = await _teamService.GetByIdAsync(id);

            if (team == null)
                return NotFound(new { message = "Team not found" });

            if (role == "User")
            {
                var userTeams =
                    await _teamService.GetByMemberUserIdAsync(userId);

                if (!userTeams.Any(x => x.Id == id))
                    return Forbid();
            }

            return Ok(team);
        }

        // Only Admin can create a team
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(
            [FromBody] CreateTeamRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Unauthorized();

            var createdById = int.Parse(userIdClaim.Value);

            var team = await _teamService.CreateAsync(
                request,
                createdById);

            return CreatedAtAction(
                nameof(GetById),
                new { id = team.Id },
                team);
        }

        // Only Admin can update a team
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateTeamRequest request)
        {
            try
            {
                await _teamService.UpdateAsync(id, request);

                var updatedTeam = await _teamService.GetByIdAsync(id);

                return Ok(updatedTeam);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Team not found" });
            }
        }

        // Only Admin can delete a team
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _teamService.DeleteAsync(id);

                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Team not found" });
            }
        }
    }
}

