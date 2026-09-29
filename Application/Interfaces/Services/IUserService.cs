using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Users;

namespace Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<UserResponse?> GetByIdAsync(int id);

        Task<IEnumerable<UserResponse>> GetAllAsync();

        Task<UserResponse?> UpdateAsync(
            int id,
            UpdateUserRequest request);

        Task<UserResponse> CreateAsync(
            CreateUserRequest request);

        Task<UserResponse?> UpdateProfileAsync(
            int id,
            string name);

        Task ChangePasswordAsync(
            int id,
            ChangePasswordRequest request);
    }
}