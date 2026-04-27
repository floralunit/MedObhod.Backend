using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MedObhod.Backend.Models;

public partial class AppointmentMedication
{
    [Key]
    [Column("AppointmentMedication_ID")]
    public Guid AppointmentMedicationId { get; set; }

    [Column("Appointment_ID")]
    public Guid? AppointmentId { get; set; }

    [Column("Medication_ID")]
    public Guid? MedicationId { get; set; }

    [StringLength(255)]
    public string? CustomName { get; set; }

    [StringLength(50)]
    public string? Dosage { get; set; }

    [StringLength(100)]
    public string? Form { get; set; }

    public DateTime CreatedDt { get; set; }

    public DateTime? UpdatedDt { get; set; }

    public bool IsDeleted { get; set; }

    public long Version { get; set; }

    [ForeignKey("AppointmentId")]
    [InverseProperty("AppointmentMedications")]
    public virtual Appointment? Appointment { get; set; }

    [ForeignKey("MedicationId")]
    [InverseProperty("AppointmentMedications")]
    public virtual Medication? Medication { get; set; }
}
