using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MedObhod.Backend.Models;

public partial class DoctorNote
{
    [Key]
    [Column("DoctorNote_ID")]
    public Guid DoctorNoteId { get; set; }

    [Column("Hospitalization_ID")]
    public Guid HospitalizationId { get; set; }

    [Column("Doctor_ID")]
    public Guid DoctorId { get; set; }

    [Column("RoundItem_ID")]
    public Guid? RoundItemId { get; set; }

    public DateTime CreatedDt { get; set; }

    public DateTime? UpdatedDt { get; set; }

    public bool IsDeleted { get; set; }

    public long Version { get; set; }

    public string? Complaints { get; set; }

    public string? GeneralCondition { get; set; }

    public string? MentalStatus { get; set; }

    public string? ExaminationSummary { get; set; }

    public string? TreatmentEffectiveness { get; set; }

    public string? PlanNote { get; set; }
    public string? Notes { get; set; }

    [ForeignKey("DoctorId")]
    [InverseProperty("DoctorNotes")]
    public virtual User? Doctor { get; set; }

    [ForeignKey("HospitalizationId")]
    [InverseProperty("DoctorNotes")]
    public virtual Hospitalization? Hospitalization { get; set; }

    [ForeignKey("RoundItemId")]
    [InverseProperty("DoctorNotes")]
    public virtual DoctorRoundItem? RoundItem { get; set; }
}
