using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task SaveAsync(
            int userId,
            string refreshToken,
            DateTime expiresAt);

        Task<int?> GetUserIdAsync(string refreshToken);

        Task DeleteAsync(string refreshToken);
    }
}