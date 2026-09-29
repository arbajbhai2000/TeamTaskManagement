using Application.Interfaces.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _context;

    public RefreshTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SaveAsync(
        int userId,
        string token,
        DateTime expiresAt)
    {
        var refreshToken = new Domain.Entities.RefreshToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        };

        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task<int?> GetUserIdAsync(
        string token)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(x =>
                x.Token == token &&
                x.ExpiresAt > DateTime.UtcNow);

        return refreshToken?.UserId;
    }

    public async Task DeleteAsync(
        string token)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == token);

        if (refreshToken != null)
        {
            _context.RefreshTokens.Remove(refreshToken);
            await _context.SaveChangesAsync();
        }
    }
}