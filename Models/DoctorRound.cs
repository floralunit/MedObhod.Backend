using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MedObhod.Backend.Models;

public partial class DoctorRound
{
    [Key]
    [Column("DoctorRound_ID")]
    public Guid DoctorRoundId { get; set; }

    [Column("Doctor_ID")]
    public Guid? DoctorId { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    [StringLength(20)]
    public string? Status { get; set; }

    public DateTime CreatedDt { get; set; }

    public DateTime? UpdatedDt { get; set; }

    public bool IsDeleted { get; set; }

    public long Version { get; set; }

    [ForeignKey("DoctorId")]
    [InverseProperty("DoctorRounds")]
    public virtual User? Doctor { get; set; }

    [InverseProperty("Round")]
    public virtual ICollection<DoctorRoundItem> DoctorRoundItems { get; set; } = new List<DoctorRoundItem>();
}
