using Application.DTOs.Auth;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtService _jwtService;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtService jwtService,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtService = jwtService;
        _configuration = configuration;
    }

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request)
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
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                request.Password),
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow
        };

        var createdUser =
            await _userRepository.AddAsync(user);

        return await GenerateAuthResponseAsync(createdUser);
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request)
    {
        var user =
            await _userRepository.GetByEmailAsync(request.Email);

        if (user == null)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        var passwordValid =
            BCrypt.Net.BCrypt.Verify(
                request.Password,
                user.PasswordHash);

        if (!passwordValid)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<AuthResponse> RefreshTokenAsync(
        string refreshToken)
    {
        var userId =
            await _refreshTokenRepository
                .GetUserIdAsync(refreshToken);

        if (userId == null)
        {
            throw new UnauthorizedAccessException(
                "Invalid or expired refresh token.");
        }

        var user =
            await _userRepository.GetByIdAsync(userId.Value);

        if (user == null)
        {
            throw new UnauthorizedAccessException(
                "User not found.");
        }

        // Delete old refresh token
        await _refreshTokenRepository.DeleteAsync(
            refreshToken);

        // Generate new access + refresh tokens
        return await GenerateAuthResponseAsync(user);
    }

    private async Task<AuthResponse> GenerateAuthResponseAsync(
        User user)
    {
        var accessToken =
            _jwtService.GenerateAccessToken(user);

        var refreshToken =
            _jwtService.GenerateRefreshToken();

        var accessTokenExpiry =
            _jwtService.GetAccessTokenExpiry();

        var refreshTokenExpiry =
            DateTime.UtcNow.AddDays(
                Convert.ToDouble(
                    _configuration[
                        "Jwt:RefreshTokenExpiryDays"] ?? "7"));

        await _refreshTokenRepository.SaveAsync(
            user.Id,
            refreshToken,
            refreshTokenExpiry);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = accessTokenExpiry
        };
    }
}