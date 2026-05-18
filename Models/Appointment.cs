using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MedObhod.Backend.Models;

public partial class Appointment
{
    [Key]
    [Column("Appointment_ID")]
    public Guid AppointmentId { get; set; }

    [Column("Hospitalization_ID")]
    public Guid HospitalizationId { get; set; }

    [Column("Template_ID")]
    [StringLength(50)]
    public string? TemplateId { get; set; }

    [Column("InsUser_ID")]
    public Guid InsUserId { get; set; }

    [StringLength(50)]
    public string? Type { get; set; }

    [StringLength(255)]
    public string? Name { get; set; }

    [StringLength(20)]
    public string? Priority { get; set; }

    public int DurationMin { get; set; }

    public string? Instructions { get; set; }

    public string? Notes { get; set; }

    [StringLength(20)]
    public string? Status { get; set; }

    public DateTime CreatedDt { get; set; }

    public DateTime? UpdatedDt { get; set; }

    public bool IsDeleted { get; set; }

    public long Version { get; set; }

    [InverseProperty("Appointment")]
    public virtual ICollection<AppointmentExecution> AppointmentExecutions { get; set; } = new List<AppointmentExecution>();

    [InverseProperty("Appointment")]
    public virtual ICollection<AppointmentSchedule> AppointmentSchedules { get; set; } = new List<AppointmentSchedule>();

    [ForeignKey("HospitalizationId")]
    [InverseProperty("Appointments")]
    public virtual Hospitalization? Hospitalization { get; set; }

    [ForeignKey("InsUserId")]
    [InverseProperty("Appointments")]
    public virtual User? InsUser { get; set; }

    [ForeignKey("TemplateId")]
    [InverseProperty("Appointments")]
    public virtual AppointmentTemplate? Template { get; set; }
}
