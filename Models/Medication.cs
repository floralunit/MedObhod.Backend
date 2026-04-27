using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MedObhod.Backend.Models;

public partial class Medication
{
    [Key]
    [Column("Medication_ID")]
    public Guid MedicationId { get; set; }

    [StringLength(255)]
    public string? Name { get; set; }

    [StringLength(100)]
    public string? Form { get; set; }

    [StringLength(50)]
    public string? DefaultDosage { get; set; }

    [StringLength(50)]
    public string? Category { get; set; }

    public DateTime CreatedDt { get; set; }

    public DateTime? UpdatedDt { get; set; }

    public bool IsDeleted { get; set; }

    public long Version { get; set; }

    [InverseProperty("Medication")]
    public virtual ICollection<AppointmentMedication> AppointmentMedications { get; set; } = new List<AppointmentMedication>();
}
