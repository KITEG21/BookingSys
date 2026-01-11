using Gateway.Api.DTOs;
using Gateway.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        _logger.LogInformation("Registration attempt for email {Email}", request.Email);

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Email and password are required" });
        }

        if (request.Password.Length < 6)
        {
            return BadRequest(new { message = "Password must be at least 6 characters" });
        }

        var result = await _authService.RegisterAsync(request);

        if (result is null)
        {
            _logger.LogWarning("Registration failed: User with email {Email} already exists", request.Email);
            return Conflict(new { message = "User with this email already exists" });
        }
        _logger.LogInformation("User {UserId} registered successfully", result.UserId);
        return CreatedAtAction(nameof(GetCurrentUser), new { id = result.UserId }, result);
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Login attempt for email {Email}", request.Email);
    
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Email and password are required" });
        }

        var result = await _authService.LoginAsync(request);

        if (result is null)
        {
            _logger.LogWarning("Login failed for email {Email}: Invalid credentials", request.Email);
            return Unauthorized(new { message = "Invalid email or password" });
        }
        _logger.LogInformation("User {UserId} logged in successfully", result.UserId);    
        return Ok(result);
    }

    /// <summary>
    /// Get current user info from token
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
    _logger.LogInformation("Get current user info attempt");

        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                       ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var user = await _authService.GetUserByIdAsync(userId);

        if (user is null)
        {
            _logger.LogWarning("User not found for ID {UserId}", userId);
            return NotFound(new { message = "User not found" });

        }

        return Ok(user);
    }

    /// <summary>
    /// Get all users (Admin only)
    /// </summary>
    [HttpGet("users")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllUsers()
    {
        _logger.LogInformation("Retrieving all users attempt by Admin");
        var users = await _authService.GetAllUsersAsync();
        return Ok(users);
    }

    /// <summary>
    /// Get user by ID (Admin only)
    /// </summary>
    [HttpGet("users/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        _logger.LogInformation("Retrieving user info attempt for UserId {UserId} by Admin", id);
        var user = await _authService.GetUserByIdAsync(id);

        if (user is null)
        {
            _logger.LogWarning("User not found for ID {UserId}", id);
            return NotFound(new { message = "User not found" });
        }

        return Ok(user);
    }
}
