using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MedObhod.Backend.Models;

[Index("RefreshToken", Name = "IX_UserSessions_RefreshToken")]
[Index("UserId", Name = "IX_UserSessions_User_ID")]
public partial class UserSession
{
    [Key]
    [Column("UserSession_ID")]
    public Guid UserSessionId { get; set; }

    [Column("User_ID")]
    public Guid UserId { get; set; }

    [StringLength(255)]
    public string RefreshToken { get; set; } = null!;

    [StringLength(100)]
    public string? DeviceId { get; set; }

    [StringLength(255)]
    public string? DeviceName { get; set; }

    [StringLength(50)]
    public string? IpAddress { get; set; }

    public DateTime LoginTime { get; set; }

    public DateTime LastActivity { get; set; }

    public bool IsActive { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedDt { get; set; }

    public DateTime? UpdatedDt { get; set; }

    public bool IsDeleted { get; set; }

    public long Version { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("UserSessions")]
    public virtual User User { get; set; } = null!;
}
