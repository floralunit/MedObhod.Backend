using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MedObhod.Backend.Models;

public partial class DoctorRoundItem
{
    [Key]
    [Column("DoctorRoundItem_ID")]
    public Guid DoctorRoundItemId { get; set; }

    [Column("Round_ID")]
    public Guid? RoundId { get; set; }

    [Column("Hospitalization_ID")]
    public Guid? HospitalizationId { get; set; }

    public int? OrderIndex { get; set; }

    [Column("PlannedTime_Dt")]
    public DateTime? PlannedTimeDt { get; set; }

    public DateTime? StartVisitTime { get; set; }

    public DateTime? EndVisitTime { get; set; }

    [StringLength(20)]
    public string? Status { get; set; }

    public DateTime CreatedDt { get; set; }

    public DateTime? UpdatedDt { get; set; }

    public bool IsDeleted { get; set; }

    public long Version { get; set; }

    [InverseProperty("RoundItem")]
    public virtual ICollection<DoctorNote> DoctorNotes { get; set; } = new List<DoctorNote>();

    [ForeignKey("HospitalizationId")]
    [InverseProperty("DoctorRoundItems")]
    public virtual Hospitalization? Hospitalization { get; set; }

    [ForeignKey("RoundId")]
    [InverseProperty("DoctorRoundItems")]
    public virtual DoctorRound? Round { get; set; }
}
