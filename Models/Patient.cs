using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MedObhod.Backend.Models;

public partial class Patient
{
    [Key]
    [Column("Patient_ID")]
    public Guid PatientId { get; set; }

    [StringLength(255)]
    public string? FullName { get; set; }

    public DateTime? BirthDt { get; set; }

    [StringLength(10)]
    public string? Gender { get; set; }

    public DateTime CreatedDt { get; set; }

    public DateTime? UpdatedDt { get; set; }

    public bool IsDeleted { get; set; }

    public long Version { get; set; }

    [InverseProperty("Patient")]
    public virtual ICollection<Hospitalization> Hospitalizations { get; set; } = new List<Hospitalization>();
}
