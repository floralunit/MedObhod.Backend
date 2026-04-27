using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MedObhod.Backend.Models;

public partial class AppointmentExecution
{
    [Key]
    [Column("AppointmentExecution_ID")]
    public Guid AppointmentExecutionId { get; set; }

    [Column("Appointment_ID")]
    public Guid? AppointmentId { get; set; }

    [Column("ScheduledDATETIME")]
    public DateTime? ScheduledDatetime { get; set; }

    public DateTime? ExecutedAt { get; set; }

    [Column("ExecutedUser_ID")]
    public Guid? ExecutedUserId { get; set; }

    [StringLength(20)]
    public string? Status { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedDt { get; set; }

    public DateTime? UpdatedDt { get; set; }

    public bool IsDeleted { get; set; }

    public long Version { get; set; }

    [ForeignKey("AppointmentId")]
    [InverseProperty("AppointmentExecutions")]
    public virtual Appointment? Appointment { get; set; }

    [ForeignKey("ExecutedUserId")]
    [InverseProperty("AppointmentExecutions")]
    public virtual User? ExecutedUser { get; set; }
}
