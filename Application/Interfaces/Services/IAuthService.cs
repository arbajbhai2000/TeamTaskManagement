using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Auth;

namespace Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginRequest request);

        Task<AuthResponse> RegisterAsync(RegisterRequest request);

        Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    }
}