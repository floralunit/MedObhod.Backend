using MedObhod.Backend.Data;
using MedObhod.Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MedObhod.Backend.Services;

public class TokenService : ITokenService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TokenService> _logger;

    public TokenService(AppDbContext context, IConfiguration configuration, ILogger<TokenService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public string GenerateAccessToken(Guid userId, string login, string role)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, login),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.Now.ToUnixTimeSeconds().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(24), // Токен живёт 24 часа
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public async Task SaveRefreshTokenAsync(Guid userId, string refreshToken, string deviceId, string deviceName, string ipAddress)
    {
        var userSession = new UserSession
        {
            UserSessionId = Guid.NewGuid(),
            UserId = userId,
            RefreshToken = refreshToken,
            DeviceId = deviceId,
            DeviceName = deviceName,
            IpAddress = ipAddress,
            LoginTime = DateTime.Now,
            LastActivity = DateTime.Now,
            IsActive = true,
            ExpiresAt = DateTime.Now.AddDays(30)
        };

        _context.UserSessions.Add(userSession);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ValidateRefreshTokenAsync(string refreshToken, Guid userId, string deviceId)
    {
        var session = await _context.UserSessions
            .FirstOrDefaultAsync(s => s.RefreshToken == refreshToken &&
                                      s.UserId == userId &&
                                      s.DeviceId == deviceId &&
                                      s.IsActive &&
                                      s.ExpiresAt > DateTime.Now);

        if (session != null)
        {
            session.LastActivity = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        var session = await _context.UserSessions
            .FirstOrDefaultAsync(s => s.RefreshToken == refreshToken);

        if (session != null)
        {
            session.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }

    public async Task RevokeAllUserTokensAsync(Guid userId)
    {
        var sessions = await _context.UserSessions
            .Where(s => s.UserId == userId && s.IsActive)
            .ToListAsync();

        foreach (var session in sessions)
        {
            session.IsActive = false;
        }

        await _context.SaveChangesAsync();
    }
}