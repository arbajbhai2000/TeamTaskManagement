using Application.DTOs.Users;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserResponse?> GetByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
                return null;

            return MapToResponse(user);
        }

        public async Task<IEnumerable<UserResponse>> GetAllAsync()
        {
            var users = await _userRepository.GetAllAsync();

            return users.Select(MapToResponse);
        }

        public async Task<UserResponse?> UpdateAsync(
            int id,
            UpdateUserRequest request)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
                return null;

            user.Name = request.Name;
            user.Role = request.Role;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            return MapToResponse(user);
        }

        public async Task<UserResponse?> UpdateProfileAsync(
            int id,
            string name)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
                return null;

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name is required.");

            user.Name = name.Trim();
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            return MapToResponse(user);
        }

        public async Task ChangePasswordAsync(
            int id,
            ChangePasswordRequest request)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
                throw new KeyNotFoundException("User not found.");

            var currentPasswordValid =
                BCrypt.Net.BCrypt.Verify(
                    request.CurrentPassword,
                    user.PasswordHash);

            if (!currentPasswordValid)
            {
                throw new UnauthorizedAccessException(
                    "Current password is incorrect.");
            }

            if (request.NewPassword != request.ConfirmPassword)
            {
                throw new ArgumentException(
                    "New password and confirm password do not match.");
            }

            if (request.NewPassword.Length < 6)
            {
                throw new ArgumentException(
                    "New password must be at least 6 characters.");
            }

            user.PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(
                    request.NewPassword);

            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
        }

        public async Task<UserResponse> CreateAsync(
            CreateUserRequest request)
        {
            var existingUser =
                await _userRepository.GetByEmailAsync(request.Email);

            if (existingUser != null)
            {
                throw new InvalidOperationException(
                    "Email already registered.");
            }

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash =
                    BCrypt.Net.BCrypt.HashPassword(
                        request.Password),
                Role = request.Role,
                CreatedAt = DateTime.UtcNow
            };

            var createdUser =
                await _userRepository.AddAsync(user);

            return MapToResponse(createdUser);
        }

        private static UserResponse MapToResponse(User user)
        {
            return new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };
        }
    }
}