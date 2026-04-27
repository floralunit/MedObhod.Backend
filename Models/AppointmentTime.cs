using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MedObhod.Backend.Models;

public partial class AppointmentTime
{
    [Key]
    [Column("AppointmentTime_ID")]
    public Guid AppointmentTimeId { get; set; }

    [Column("Schedule_ID")]
    public Guid? ScheduleId { get; set; }

    public TimeOnly? TimeValue { get; set; }

    public DateTime CreatedDt { get; set; }

    public DateTime? UpdatedDt { get; set; }

    public bool IsDeleted { get; set; }

    public long Version { get; set; }

    [ForeignKey("ScheduleId")]
    [InverseProperty("AppointmentTimes")]
    public virtual AppointmentSchedule? Schedule { get; set; }
}
