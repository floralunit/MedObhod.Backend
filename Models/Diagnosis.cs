using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MedObhod.Backend.Models;

public partial class Diagnosis
{
    [Key]
    [Column("Diagnose_ID")]
    public Guid DiagnoseId { get; set; }

    [StringLength(255)]
    public string? Name { get; set; }

    [StringLength(20)]
    public string? Code { get; set; }

    public DateTime CreatedDt { get; set; }

    public DateTime? UpdatedDt { get; set; }

    public bool IsDeleted { get; set; }

    public long Version { get; set; }

    [InverseProperty("Diagnosis")]
    public virtual ICollection<PatientDiagnosis> PatientDiagnoses { get; set; } = new List<PatientDiagnosis>();
}
