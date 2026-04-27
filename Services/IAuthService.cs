using MedObhod.Backend.DTOs;

namespace MedObhod.Backend.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, string ipAddress);
    Task<RefreshTokenResponse?> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress);
    Task<bool> LogoutAsync(string refreshToken);
    Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
    Task<UserInfoResponse?> GetUserInfoAsync(Guid userId);
    Task<List<UserInfoResponse>> GetAllUsersAsync();
    Task<UserInfoResponse> CreateUserAsync(RegisterRequest request, Guid createdBy);
    Task<bool> UpdateUserRoleAsync(Guid userId, string newRole, Guid updatedBy);
    Task<bool> DeleteUserAsync(Guid userId, Guid deletedBy);
    Task<List<SessionInfoResponse>> GetUserSessionsAsync(Guid userId);
    Task<bool> RevokeSessionAsync(Guid sessionId, Guid userId);
}