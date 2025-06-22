using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using ProductPriceTracker.Core.Dtos;
using ProductPriceTracker.Core.Entities;
using ProductPriceTracker.Core.Interface.IServices;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IConfiguration _config;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, IConfiguration config, IJwtService jwtService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _config = config;
        _jwtService = jwtService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        var success = await _userService.RegisterAsync(registerDto);
        if (!success)
            return BadRequest("User already exists.");

        return Ok("User registered.");
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenResponseDto>> Login([FromBody] LoginDto dto)
    {
        _logger.LogInformation("Attempting to log in user with username: {Username}", dto.Username);

        var token = await _userService.LoginAsync(dto);
        if (token == null)
            return Unauthorized("Invalid credentials");

        return Ok(token);
    }

    [HttpPost("google-login")]
    public async Task<ActionResult<TokenResponseDto>> GoogleLogin([FromBody] GoogleLoginDto googleLoginDto)
    {
        if (string.IsNullOrWhiteSpace(googleLoginDto.GoogleToken))
        {
            _logger.LogWarning("Google token is missing.");
            return BadRequest("Google token is required.");
        }

        try
        {
            _logger.LogInformation("Attempting Google login with token: {GoogleToken}", googleLoginDto.GoogleToken);

            // 驗證並產生 Token
            var token = await _userService.GoogleLoginAsync(googleLoginDto);

            if (token == null)
            {
                _logger.LogWarning("Google token validation failed.");
                return Unauthorized("Invalid Google token");
            }

            _logger.LogInformation("Google login successful for user: {Username}", token.Username);
            return Ok(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during Google login.");
            return StatusCode(500, "Internal server error");
        }
    }


    [HttpPost("refresh")]
    public async Task<ActionResult<TokenResponseDto>> Refresh([FromBody] TokenRequestDto tokenDto)
    {
        var token = await _userService.RefreshAsync(tokenDto);
        if (token == null)
            return Unauthorized("Refresh token expired or invalid");

        return Ok(token);
    }

    [HttpPost("check-token")]
    public IActionResult CheckTokenValidity([FromBody] CheckTokenRequestDto requestToken)
    {
        if (string.IsNullOrEmpty(requestToken.Token))
            return BadRequest("Token is required");

        var isValid = _jwtService.CheckTokenValidity(requestToken.Token);
        if (!isValid)
            return Unauthorized("Invalid token");

        return Ok("Token is valid");
    }
}