using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductPriceTracker.Core.Entities;


namespace ProductPriceTracker.Core.Interface.IServices
{
    public interface IUserService
    {
        Task<List<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByUsernameAsync(string username);
        Task CreateUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(int id);
        Task<bool> RegisterAsync(RegisterDto registerDto);
        Task<TokenResponseDto?> LoginAsync(LoginDto dto);
        Task<TokenResponseDto?> RefreshAsync(TokenRequestDto tokenDto);
        bool CheckTokenValidity(string token);
        Task<bool> UserExistsAsync(string username);
    }
}