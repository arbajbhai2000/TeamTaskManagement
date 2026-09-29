using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Tasks;

namespace Application.Interfaces.Services
{
    public interface ITaskService
    {
        Task<TaskResponse?> GetByIdAsync(int id);

        Task<IEnumerable<TaskResponse>> GetAllAsync();

        Task<IEnumerable<TaskResponse>> GetByAssignedUserIdAsync(
            int userId);

        Task<TaskResponse> CreateAsync(
            CreateTaskRequest request);

        Task UpdateAsync(
               int id,
               UpdateTaskRequest request);
        Task DeleteAsync(int id);
    }
}

