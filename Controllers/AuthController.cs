using MedObhod.Backend.DTOs;
using MedObhod.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MedObhod.Backend.Controllers;

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
    /// Login user and get tokens (requires internet)
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<LoginResponse>), 200)]
    [ProducesResponseType(typeof(BaseResponse<object>), 401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var result = await _authService.LoginAsync(request, ipAddress);

            if (result == null)
            {
                return Unauthorized(BaseResponse<object>.Error("Неверный логин и/или пароль", 401));
            }

            return Ok(BaseResponse<LoginResponse>.Ok(result, "Успешная авторизация"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login error for {Login}", request.Login);
            return StatusCode(500, BaseResponse<object>.Error("Внутренняя ошибка сервера", 500));
        }
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<RefreshTokenResponse>), 200)]
    [ProducesResponseType(typeof(BaseResponse<object>), 401)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var result = await _authService.RefreshTokenAsync(request, ipAddress);

            if (result == null)
            {
                return Unauthorized(BaseResponse<object>.Error("Invalid refresh token", 401));
            }

            return Ok(BaseResponse<RefreshTokenResponse>.Ok(result, "Token refreshed"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Refresh token error");
            return StatusCode(500, BaseResponse<object>.Error("Internal server error", 500));
        }
    }

    /// <summary>
    /// Logout user (revoke refresh token)
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
    public async Task<IActionResult> Logout([FromBody] string refreshToken)
    {
        try
        {
            await _authService.LogoutAsync(refreshToken);
            return Ok(BaseResponse<bool>.Ok(true, "Logged out successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Logout error");
            return StatusCode(500, BaseResponse<bool>.Error("Internal server error", 500));
        }
    }

    /// <summary>
    /// Change user password
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
    [ProducesResponseType(typeof(BaseResponse<object>), 400)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var result = await _authService.ChangePasswordAsync(userId, request);

            if (!result)
            {
                return BadRequest(BaseResponse<object>.Error("Current password is incorrect", 400));
            }

            return Ok(BaseResponse<bool>.Ok(true, "Password changed successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Change password error");
            return StatusCode(500, BaseResponse<bool>.Error("Internal server error", 500));
        }
    }

    /// <summary>
    /// Get current user info
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(BaseResponse<UserInfoResponse>), 200)]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var user = await _authService.GetUserInfoAsync(userId);

            if (user == null)
            {
                return NotFound(BaseResponse<object>.NotFound("User not found"));
            }

            return Ok(BaseResponse<UserInfoResponse>.Ok(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get current user error");
            return StatusCode(500, BaseResponse<object>.Error("Internal server error", 500));
        }
    }

    /// <summary>
    /// Get all users (Admin only)
    /// </summary>
    [HttpGet("users")]
    [Authorize(Roles = "head")]
    [ProducesResponseType(typeof(BaseResponse<List<UserInfoResponse>>), 200)]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            var users = await _authService.GetAllUsersAsync();
            return Ok(BaseResponse<List<UserInfoResponse>>.Ok(users));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get all users error");
            return StatusCode(500, BaseResponse<object>.Error("Internal server error", 500));
        }
    }

    /// <summary>
    /// Create new user (Admin only)
    /// </summary>
    [HttpPost("users")]
    [Authorize(Roles = "head")]
    [ProducesResponseType(typeof(BaseResponse<UserInfoResponse>), 201)]
    [ProducesResponseType(typeof(BaseResponse<object>), 400)]
    public async Task<IActionResult> CreateUser([FromBody] RegisterRequest request)
    {
        try
        {
            // Логируем входящий запрос для отладки
            _logger.LogInformation("Create user request: {@Request}", request);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(BaseResponse<object>.Error($"Invalid data: {string.Join(", ", errors)}", 400));
            }

            var createdBy = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var user = await _authService.CreateUserAsync(request, createdBy);

            return CreatedAtAction(nameof(GetCurrentUser), new { id = user.Id },
                BaseResponse<UserInfoResponse>.Created(user, "User created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(BaseResponse<object>.Error(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Create user error");
            return StatusCode(500, BaseResponse<object>.Error("Internal server error", 500));
        }
    }

    /// <summary>
    /// Update user role (Admin only)
    /// </summary>
    [HttpPut("users/{userId}/role")]
    [Authorize(Roles = "head")]
    [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
    [ProducesResponseType(typeof(BaseResponse<object>), 404)]
    public async Task<IActionResult> UpdateUserRole(Guid userId, [FromBody] string newRole)
    {
        try
        {
            var updatedBy = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var result = await _authService.UpdateUserRoleAsync(userId, newRole, updatedBy);

            if (!result)
            {
                return NotFound(BaseResponse<object>.NotFound("User not found"));
            }

            return Ok(BaseResponse<bool>.Ok(true, "User role updated"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Update user role error");
            return StatusCode(500, BaseResponse<object>.Error("Internal server error", 500));
        }
    }

    /// <summary>
    /// Delete user (Admin only)
    /// </summary>
    [HttpDelete("users/{userId}")]
    [Authorize(Roles = "head")]
    [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
    [ProducesResponseType(typeof(BaseResponse<object>), 404)]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        try
        {
            // Нельзя удалить самого себя
            var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            if (currentUserId == userId)
            {
                return BadRequest(BaseResponse<object>.Error("Cannot delete your own account", 400));
            }

            var deletedBy = currentUserId;
            var result = await _authService.DeleteUserAsync(userId, deletedBy);

            if (!result)
            {
                return NotFound(BaseResponse<object>.NotFound("User not found"));
            }

            return Ok(BaseResponse<bool>.Ok(true, "User deleted"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete user error");
            return StatusCode(500, BaseResponse<object>.Error("Internal server error", 500));
        }
    }

    /// <summary>
    /// Get user sessions (Admin only or own)
    /// </summary>
    [HttpGet("sessions")]
    [Authorize]
    [ProducesResponseType(typeof(BaseResponse<List<SessionInfoResponse>>), 200)]
    public async Task<IActionResult> GetMySessions()
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var sessions = await _authService.GetUserSessionsAsync(userId);
            return Ok(BaseResponse<List<SessionInfoResponse>>.Ok(sessions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get sessions error");
            return StatusCode(500, BaseResponse<object>.Error("Internal server error", 500));
        }
    }
}