using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MedObhod.Backend.Models;

public partial class AppointmentSchedule
{
    [Key]
    [Column("AppointmentSchedule_ID")]
    public Guid AppointmentScheduleId { get; set; }

    [Column("Appointment_ID")]
    public Guid? AppointmentId { get; set; }

    [StringLength(50)]
    public string? Frequency { get; set; }

    public DateTime? StartDt { get; set; }

    public DateTime? EndDt { get; set; }

    public TimeOnly? StartTime { get; set; }

    [StringLength(100)]
    public string? RelationToMeal { get; set; }

    public DateTime CreatedDt { get; set; }

    public DateTime? UpdatedDt { get; set; }

    public bool IsDeleted { get; set; }

    public long Version { get; set; }

    [ForeignKey("AppointmentId")]
    [InverseProperty("AppointmentSchedules")]
    public virtual Appointment? Appointment { get; set; }

    [InverseProperty("Schedule")]
    public virtual ICollection<AppointmentTime> AppointmentTimes { get; set; } = new List<AppointmentTime>();
}
