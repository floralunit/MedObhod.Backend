using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MedObhod.Backend.Models;

public partial class PatientDiagnosis
{
    [Key]
    [Column("PatientDiagnose_ID")]
    public Guid PatientDiagnoseId { get; set; }

    [Column("Hospitalization_ID")]
    public Guid HospitalizationId { get; set; }

    [Column("Diagnosis_ID")]
    public Guid DiagnosisId { get; set; }

    public bool IsPrimary { get; set; }

    public DateTime CreatedDt { get; set; }

    public DateTime? UpdatedDt { get; set; }

    public bool IsDeleted { get; set; }

    public long Version { get; set; }

    [ForeignKey("DiagnosisId")]
    [InverseProperty("PatientDiagnoses")]
    public virtual Diagnosis? Diagnosis { get; set; }

    [ForeignKey("HospitalizationId")]
    [InverseProperty("PatientDiagnoses")]
    public virtual Hospitalization? Hospitalization { get; set; }
}
