using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ProductPriceTracker.Core.Entities;

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateAccessToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role) // 添加角色聲明
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        double tokenExpirationMinutes = double.TryParse(_config["Jwt:UserAccessTokenExpiration"], out var minutes) ? minutes : 60; // 預設為 60 分鐘
        if (tokenExpirationMinutes <= 0)
        {
            throw new ArgumentException("Token expiration time must be greater than zero.");
        }

        if (user.Role == "Admin")
        {
            tokenExpirationMinutes = double.TryParse(_config["Jwt:AdminAccessTokenExpirationMinutes"], out var adminMinutes) ? adminMinutes : 5; // 管理員預設為 5 分鐘
            if (adminMinutes <= 0)
            {
                throw new ArgumentException("Admin token expiration time must be greater than zero.");
            }
        }

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(tokenExpirationMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    public bool CheckTokenValidity(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);
        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero // Disable clock skew
            }, out SecurityToken validatedToken);
            return true;
        }
        catch
        {
            return false;
        }
    }
}