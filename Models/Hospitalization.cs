using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MedObhod.Backend.Models;

public partial class Hospitalization
{
    [Key]
    [Column("Hospitalization_ID")]
    public Guid HospitalizationId { get; set; }

    [Column("Patient_ID")]
    public Guid PatientId { get; set; }

    public DateTime AdmissionDt { get; set; }

    public DateTime? DischargeDt { get; set; }

    [StringLength(20)]
    public string? Room { get; set; }

    [StringLength(10)]
    public string? Bed { get; set; }

    [Column("AttendingDoctor_ID")]
    public Guid? AttendingDoctorId { get; set; }

    [StringLength(20)]
    public string? Status { get; set; }

    public DateTime CreatedDt { get; set; }

    public DateTime? UpdatedDt { get; set; }

    public bool IsDeleted { get; set; }

    public long Version { get; set; }

    [InverseProperty("Hospitalization")]
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    [ForeignKey("AttendingDoctorId")]
    [InverseProperty("Hospitalizations")]
    public virtual User? AttendingDoctor { get; set; }

    [InverseProperty("Hospitalization")]
    public virtual ICollection<DoctorNote> DoctorNotes { get; set; } = new List<DoctorNote>();

    [InverseProperty("Hospitalization")]
    public virtual ICollection<DoctorRoundItem> DoctorRoundItems { get; set; } = new List<DoctorRoundItem>();

    [ForeignKey("PatientId")]
    [InverseProperty("Hospitalizations")]
    public virtual Patient? Patient { get; set; }

    [InverseProperty("Hospitalization")]
    public virtual ICollection<PatientDiagnosis> PatientDiagnoses { get; set; } = new List<PatientDiagnosis>();

    [InverseProperty("Hospitalization")]
    public virtual ICollection<VitalSign> VitalSigns { get; set; } = new List<VitalSign>();
}
