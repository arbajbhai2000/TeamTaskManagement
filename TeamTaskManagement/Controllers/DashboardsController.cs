using Microsoft.AspNetCore.Mvc;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TeamTaskManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardsController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardsController(
            IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,User")]
        public async Task<IActionResult> GetDashboard(
            [FromQuery] string? status = null,
            [FromQuery] string? priority = null,
            [FromQuery] DateTime? deadline = null)
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var role =
                User.FindFirst(ClaimTypes.Role)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(role))
                return Unauthorized();

            var dashboard =
                await _dashboardService.GetDashboardAsync(
                    userId,
                    role,
                    status,
                    priority,
                    deadline);

            return Ok(dashboard);
        }
    }
}