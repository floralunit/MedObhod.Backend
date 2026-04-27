using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MedObhod.Backend.Models;

[Index("Login", Name = "UQ__Users__5E55825B4017C40A", IsUnique = true)]
public partial class User
{
    [Key]
    [Column("User_ID")]
    public Guid UserId { get; set; }

    [StringLength(50)]
    public string? Login { get; set; }

    [StringLength(255)]
    public string? PasswordHash { get; set; }

    [StringLength(255)]
    public string? FullName { get; set; }

    [StringLength(20)]
    public string? Role { get; set; }

    public DateTime CreatedDt { get; set; }

    public DateTime? UpdatedDt { get; set; }

    public bool IsDeleted { get; set; }

    public long Version { get; set; }

    [InverseProperty("ExecutedUser")]
    public virtual ICollection<AppointmentExecution> AppointmentExecutions { get; set; } = new List<AppointmentExecution>();

    [InverseProperty("InsUser")]
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    [InverseProperty("Doctor")]
    public virtual ICollection<DoctorNote> DoctorNotes { get; set; } = new List<DoctorNote>();

    [InverseProperty("Doctor")]
    public virtual ICollection<DoctorRound> DoctorRounds { get; set; } = new List<DoctorRound>();

    [InverseProperty("AttendingDoctor")]
    public virtual ICollection<Hospitalization> Hospitalizations { get; set; } = new List<Hospitalization>();

    [InverseProperty("User")]
    public virtual ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();

    [InverseProperty("InsUser")]
    public virtual ICollection<VitalSign> VitalSigns { get; set; } = new List<VitalSign>();
}
