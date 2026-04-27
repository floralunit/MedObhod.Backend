using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MedObhod.Backend.Models;

public partial class AppointmentTemplate
{
    [Key]
    [Column("AppointmentTemplate_ID")]
    [StringLength(50)]
    public string AppointmentTemplateId { get; set; } = null!;

    [StringLength(255)]
    public string? Name { get; set; }

    [StringLength(50)]
    public string? Type { get; set; }

    public int? DurationMin { get; set; }

    public bool RequiresMedication { get; set; }

    public DateTime CreatedDt { get; set; }

    public DateTime? UpdatedDt { get; set; }

    public bool IsDeleted { get; set; }

    public long Version { get; set; }

    [InverseProperty("Template")]
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
