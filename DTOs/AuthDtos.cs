using System.ComponentModel.DataAnnotations;

namespace MedObhod.Backend.DTOs;

// ============ Request DTOs ============
public class LoginRequest
{
    [Required(ErrorMessage = "Login is required")]
    [MinLength(3, ErrorMessage = "Login must be at least 3 characters")]
    [MaxLength(50, ErrorMessage = "Login cannot exceed 50 characters")]
    public string Login { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(4, ErrorMessage = "Password must be at least 4 characters")]
    public string Password { get; set; } = string.Empty;

    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
}

public class RegisterRequest
{
    [Required(ErrorMessage = "Login is required")]
    [MinLength(3, ErrorMessage = "Login must be at least 3 characters")]
    [MaxLength(50, ErrorMessage = "Login cannot exceed 50 characters")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Login can only contain letters, numbers and underscore")]
    public string Login { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(4, ErrorMessage = "Password must be at least 4 characters")]
    [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full name is required")]
    [MinLength(2, ErrorMessage = "Full name must be at least 2 characters")]
    [MaxLength(255, ErrorMessage = "Full name cannot exceed 255 characters")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role is required")]
    [RegularExpression("^(doctor|nurse|head)$", ErrorMessage = "Role must be doctor, nurse or head")]
    public string Role { get; set; } = string.Empty;

    public string? CreatedBy { get; set; }
}

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;

    public string DeviceId { get; set; } = string.Empty;
}

public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [MinLength(4, ErrorMessage = "New password must be at least 4 characters")]
    public string NewPassword { get; set; } = string.Empty;
}

// ============ Response DTOs ============
public class LoginResponse
{
    public Guid UserId { get; set; }
    public string Login { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; set; }
    public DateTime RefreshTokenExpiresAt { get; set; }

    // Для офлайн-работы
    public long UserVersion { get; set; }
    public Dictionary<string, List<string>> Permissions { get; set; } = new();
}

public class RefreshTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiresAt { get; set; }
}

public class UserInfoResponse
{
    public Guid Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

public class SessionInfoResponse
{
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public DateTime LoginTime { get; set; }
    public DateTime LastActivity { get; set; }
    public string IpAddress { get; set; } = string.Empty;
}