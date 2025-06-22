using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProductPriceTracker.Core.Dtos;
using ProductPriceTracker.Core.Entities;
using ProductPriceTracker.Core.Interface.IRepositories;
using ProductPriceTracker.Core.Interface.IServices;


namespace ProductPriceTracker.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;
        private readonly ILogger<UserService> _logger;
        private readonly GoogleAuthService _googleAuthService;
        private readonly IConfiguration _config;


        public UserService(IUnitOfWork unitOfWork, IJwtService jwtService, ILogger<UserService> logger, GoogleAuthService googleAuthService, IConfiguration config)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _logger = logger;
            _googleAuthService = googleAuthService;
            _config = config;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _unitOfWork.Users.GetAllAsync(
                includeProperties: "RefreshTokens");
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _unitOfWork.Users.GetByIdAsync(id);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _unitOfWork.Users.GetUserByUsernameAsync(username);
        }

        public async Task CreateUserAsync(User user)
        {

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateUserAsync(User user)
        {
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user != null)
            {
                _unitOfWork.Users.Remove(user);
                await _unitOfWork.SaveAsync();
            }
        }

        public async Task<bool> RegisterAsync(RegisterDto registerDto)
        {
            var exists = await _unitOfWork.Users
                .GetFirstOrDefaultAsync(u => u.Username == registerDto.Username, tracked: false);

            if (exists != null)
                return false;

            var user = new User
            {
                Username = registerDto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                Role = "User", // 預設角色為 User
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<TokenResponseDto?> LoginAsync(LoginDto dto)
        {
            if (string.IsNullOrEmpty(dto.Username) || string.IsNullOrEmpty(dto.Password))
            {
                _logger.LogWarning("Login attempt with empty username or password.");
                return null;
            }
            else
            {
                _logger.LogInformation($"Login attempt for user: {dto.Username}");
            }
            var user = await _unitOfWork.Users
                .GetFirstOrDefaultAsync(u => u.Username == dto.Username, includeProperties: "RefreshTokens");

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return null;

            _logger.LogInformation($"User {dto.Username} logged in successfully.");

            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            _logger.LogInformation($"Generated access token and refresh token for user {dto.Username}.");

            user.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });

            _logger.LogInformation($"Added refresh token for user {dto.Username}.");

            await _unitOfWork.SaveAsync();

            _logger.LogInformation($"Saved user {dto.Username} with new refresh token.");

            return new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<TokenResponseDto?> RefreshAsync(TokenRequestDto tokenDto)
        {
            if (string.IsNullOrEmpty(tokenDto.RefreshToken))
            {
                _logger.LogWarning("Refresh token is null or empty.");
                return null;
            }

            var user = await _unitOfWork.Users
                .GetFirstOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == tokenDto.RefreshToken),
                    includeProperties: "RefreshTokens");

            if (user == null)
            {
                _logger.LogWarning("No user found with provided refresh token.");
                return null;
            }

            var refreshToken = user.RefreshTokens.FirstOrDefault(rt => rt.Token == tokenDto.RefreshToken);

            if (refreshToken == null || refreshToken.IsRevoked || refreshToken.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("Refresh token expired or revoked.");
                return null;
            }
            _logger.LogInformation($"User {user.Username} is refreshing tokens.");

            // 產生新的 access token
            var newAccessToken = _jwtService.GenerateAccessToken(user);

            // 判斷是否要更新 refresh token（例如只剩 1 天有效期時再換）
            var daysLeft = (refreshToken.ExpiresAt - DateTime.UtcNow).TotalDays;
            string newRefreshTokenString = null;

            if (daysLeft < 1)  // 剩一天就更新 refresh token
            {
                // 作廢舊的 refresh token
                refreshToken.IsRevoked = true;

                // 產生新的 refresh token
                newRefreshTokenString = _jwtService.GenerateRefreshToken();

                user.RefreshTokens.Add(new RefreshToken
                {
                    Token = newRefreshTokenString,
                    ExpiresAt = DateTime.UtcNow.AddDays(7)
                });

                await _unitOfWork.SaveAsync();
            }

            return new TokenResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshTokenString ?? tokenDto.RefreshToken // 若沒更新，回傳原本的
            };
        }

        public bool CheckTokenValidity(string token)
        {
            return _jwtService.CheckTokenValidity(token);
        }

        public async Task<bool> UserExistsAsync(string username)
        {
            return await _unitOfWork.Users
                .GetFirstOrDefaultAsync(u => u.Username == username, tracked: false) != null;
        }

        public async Task<TokenResponseDto?> GoogleLoginAsync(GoogleLoginDto dto)
        {
            GoogleJsonWebSignature.Payload payload;
            try
            {
                // 驗證 Google Token 並取得 Payload
                payload = await _googleAuthService.VerifyTokenAsync(dto.GoogleToken) 
                    ?? throw new Exception("Invalid Google token");
                _logger.LogInformation($"Google token validated for user: {payload.Email}");
            }
            catch
            {
                _logger.LogWarning("Google token validation failed.");
                // Token 驗證失敗
                return null;
            }

            // 先查詢資料庫是否有該使用者（以 GoogleId 或 Email 辨識）
            var user = await _unitOfWork.Users
                .GetFirstOrDefaultAsync(u => u.GoogleId == payload.Subject || u.Email == payload.Email);

            if (user == null)
            {
                // 第一次登入，新增使用者
                user = new User
                {
                    Username = "google_" + payload.Subject,
                    PasswordHash = "GOOGLE", // 因為是第三方登入，密碼欄位可用特殊值或留空
                    GoogleId = payload.Subject,
                    Email = payload.Email,
                    Name = payload.Name,
                    AvatarUrl = payload.Picture,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveAsync();
            }

            // 產生 JWT Token
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // 你也可以把 refreshToken 存到資料庫

            return new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Username = user.Username
            };
        }
    }
}