using System;
using Application.DTOs.Dashboard;

namespace Application.Interfaces.Services
{
    public interface IDashboardService
    {
        Task<DashboardResponse> GetDashboardAsync(
            int userId,
            string role,
            string? status = null,
            string? priority = null,
            DateTime? deadline = null);
    }
}