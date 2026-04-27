using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MedObhod.Backend.Models;

[Table("PatientStatusHistory")]
public partial class PatientStatusHistory
{
    [Key]
    [Column("PatientStatusHistory_ID")]
    public Guid PatientStatusHistoryId { get; set; }

    [Column("Hospitalization_ID")]
    public Guid? HospitalizationId { get; set; }

    [StringLength(20)]
    public string? Status { get; set; }

    [Column("NEWSScore")]
    public int? Newsscore { get; set; }

    public DateTime? RecordedDt { get; set; }

    public DateTime CreatedDt { get; set; }

    public DateTime? UpdatedDt { get; set; }

    public bool IsDeleted { get; set; }

    public long Version { get; set; }

    [ForeignKey("HospitalizationId")]
    [InverseProperty("PatientStatusHistories")]
    public virtual Hospitalization? Hospitalization { get; set; }
}
