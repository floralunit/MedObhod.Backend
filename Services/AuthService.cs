using MedObhod.Backend.Data;
using MedObhod.Backend.DTOs;
using MedObhod.Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace MedObhod.Backend.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(AppDbContext context, ITokenService tokenService, ILogger<AuthService> logger)
    {
        _context = context;
        _tokenService = tokenService;
        _logger = logger;
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        var hashString = Convert.ToBase64String(hashedBytes);
        return hashString;
    }

    private bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }

    private Dictionary<string, List<string>> GetUserPermissions(string role)
    {
        var permissions = new Dictionary<string, List<string>>();

        switch (role.ToLower())
        {
            case "head":
                permissions["Patients"] = new List<string> { "read", "write", "delete" };
                permissions["Appointments"] = new List<string> { "read", "write", "delete" };
                permissions["VitalSigns"] = new List<string> { "read", "write" };
                permissions["Users"] = new List<string> { "read", "write", "delete" };
                break;

            case "doctor":
                permissions["Patients"] = new List<string> { "read", "write" };
                permissions["Appointments"] = new List<string> { "read", "write" };
                permissions["VitalSigns"] = new List<string> { "read" };
                permissions["DoctorNotes"] = new List<string> { "read", "write" };
                break;

            case "nurse":
                permissions["Patients"] = new List<string> { "read" };
                permissions["Appointments"] = new List<string> { "read", "write" };
                permissions["VitalSigns"] = new List<string> { "read", "write" };
                break;
        }

        return permissions;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, string ipAddress)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Login == request.Login && !u.IsDeleted);

        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for {Login} from {Ip}", request.Login, ipAddress);
            return null;
        }

        var accessToken = _tokenService.GenerateAccessToken(user.UserId, user.Login, user.Role);
        var refreshToken = _tokenService.GenerateRefreshToken();

        await _tokenService.SaveRefreshTokenAsync(
            user.UserId,
            refreshToken,
            string.IsNullOrEmpty(request.DeviceId) ? $"device_{Guid.NewGuid()}" : request.DeviceId,
            string.IsNullOrEmpty(request.DeviceName) ? "Unknown Device" : request.DeviceName,
            ipAddress
        );

        // Обновляем версию пользователя при каждом входе
        user.Version++;
        user.UpdatedDt = DateTime.Now;
        await _context.SaveChangesAsync();

        var permissions = GetUserPermissions(user.Role);

        return new LoginResponse
        {
            UserId = user.UserId,
            Login = user.Login,
            FullName = user.FullName,
            Role = user.Role,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAt = DateTime.Now.AddHours(24),
            RefreshTokenExpiresAt = DateTime.Now.AddDays(30),
            UserVersion = user.Version,
            Permissions = permissions
        };
    }

    public async Task<RefreshTokenResponse?> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress)
    {
        // Находим сессию по refresh token
        var session = await _context.UserSessions
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.RefreshToken == request.RefreshToken &&
                                      s.IsActive &&
                                      s.ExpiresAt > DateTime.Now);

        if (session == null)
        {
            return null;
        }

        // Проверяем deviceId если передан
        if (!string.IsNullOrEmpty(request.DeviceId) && session.DeviceId != request.DeviceId)
        {
            _logger.LogWarning("Device mismatch for refresh token. User: {UserId}", session.UserId);
            return null;
        }

        // Генерируем новые токены
        var newAccessToken = _tokenService.GenerateAccessToken(
            session.UserId,
            session.User.Login,
            session.User.Role
        );

        var newRefreshToken = _tokenService.GenerateRefreshToken();

        // Обновляем refresh token в сессии
        session.RefreshToken = newRefreshToken;
        session.LastActivity = DateTime.Now;
        session.ExpiresAt = DateTime.Now.AddDays(30);

        await _context.SaveChangesAsync();

        return new RefreshTokenResponse
        {
            AccessToken = newAccessToken,
            AccessTokenExpiresAt = DateTime.Now.AddHours(24),
            RefreshToken = newRefreshToken,
            RefreshTokenExpiresAt = DateTime.Now.AddDays(30)
        };
    }

    public async Task<bool> LogoutAsync(string refreshToken)
    {
        await _tokenService.RevokeRefreshTokenAsync(refreshToken);
        return true;
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null || !VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            return false;
        }

        user.PasswordHash = HashPassword(request.NewPassword);
        user.UpdatedDt = DateTime.Now;
        user.Version++;

        await _context.SaveChangesAsync();

        // Отзываем все сессии пользователя для безопасности
        await _tokenService.RevokeAllUserTokensAsync(userId);

        return true;
    }

    public async Task<UserInfoResponse?> GetUserInfoAsync(Guid userId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted);

        if (user == null) return null;

        return new UserInfoResponse
        {
            Id = user.UserId,
            Login = user.Login,
            FullName = user.FullName,
            Role = user.Role,
            CreatedAt = user.CreatedDt,
            IsActive = !user.IsDeleted
        };
    }

    public async Task<List<UserInfoResponse>> GetAllUsersAsync()
    {
        return await _context.Users
            .Where(u => !u.IsDeleted)
            .Select(u => new UserInfoResponse
            {
                Id = u.UserId,
                Login = u.Login,
                FullName = u.FullName,
                Role = u.Role,
                CreatedAt = u.CreatedDt,
                IsActive = !u.IsDeleted
            })
            .ToListAsync();
    }

    public async Task<UserInfoResponse> CreateUserAsync(RegisterRequest request, Guid createdBy)
    {
        // Проверяем, существует ли пользователь
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Login == request.Login);

        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this login already exists");
        }

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Login = request.Login,
            PasswordHash = HashPassword(request.Password),
            FullName = request.FullName,
            Role = request.Role,
            CreatedDt = DateTime.Now,
            UpdatedDt = DateTime.Now,
            IsDeleted = false,
            Version = 1
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("New user created: {Login} by {Creator}", request.Login, createdBy);

        return new UserInfoResponse
        {
            Id = user.UserId,
            Login = user.Login,
            FullName = user.FullName,
            Role = user.Role,
            CreatedAt = user.CreatedDt,
            IsActive = true
        };
    }

    public async Task<bool> UpdateUserRoleAsync(Guid userId, string newRole, Guid updatedBy)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null || user.IsDeleted) return false;

        user.Role = newRole;
        user.UpdatedDt = DateTime.Now;
        user.Version++;

        // Отзываем все сессии пользователя
        var sessions = await _context.UserSessions
            .Where(s => s.UserId == userId && s.IsActive)
            .ToListAsync();

        foreach (var session in sessions)
        {
            session.IsActive = false;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteUserAsync(Guid userId, Guid deletedBy)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null || user.IsDeleted) return false;

        user.IsDeleted = true;
        user.UpdatedDt = DateTime.Now;
        user.Version++;

        await _context.SaveChangesAsync();

        // Отзываем все сессии
        await _tokenService.RevokeAllUserTokensAsync(userId);

        _logger.LogInformation("User deleted: {UserId} by {DeletedBy}", userId, deletedBy);

        return true;
    }

    public async Task<List<SessionInfoResponse>> GetUserSessionsAsync(Guid userId)
    {
        return await _context.UserSessions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.LastActivity)
            .Select(s => new SessionInfoResponse
            {
                DeviceId = s.DeviceId,
                DeviceName = s.DeviceName,
                LoginTime = s.LoginTime,
                LastActivity = s.LastActivity,
                IpAddress = s.IpAddress ?? string.Empty
            })
            .ToListAsync();
    }

    public async Task<bool> RevokeSessionAsync(Guid sessionId, Guid userId)
    {
        var session = await _context.UserSessions
            .FirstOrDefaultAsync(s => s.UserSessionId == sessionId && s.UserId == userId);

        if (session == null) return false;

        session.IsActive = false;
        await _context.SaveChangesAsync();

        return true;
    }

}