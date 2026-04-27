using MedObhod.Backend.DTOs;

namespace MedObhod.Backend.Services;

public interface ITokenService
{
    string GenerateAccessToken(Guid userId, string login, string role);
    string GenerateRefreshToken();
    Task SaveRefreshTokenAsync(Guid userId, string refreshToken, string deviceId, string deviceName, string ipAddress);
    Task<bool> ValidateRefreshTokenAsync(string refreshToken, Guid userId, string deviceId);
    Task RevokeRefreshTokenAsync(string refreshToken);
    Task RevokeAllUserTokensAsync(Guid userId);
}