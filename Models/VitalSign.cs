using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MedObhod.Backend.Models;

public partial class VitalSign
{
    [Key]
    [Column("VitalSign_ID")]
    public Guid VitalSignId { get; set; }

    [Column("Hospitalization_ID")]
    public Guid HospitalizationId { get; set; }

    public DateTime MeasuredDt { get; set; }

    public double? Temperature { get; set; }

    public int? Pulse { get; set; }

    [Column("SystolicBP")]
    public int? SystolicBp { get; set; }

    [Column("DiastolicBP")]
    public int? DiastolicBp { get; set; }

    public int? SpO2 { get; set; }

    public int? RespiratoryRate { get; set; }

    [Column("NEWSScore")]
    public int? Newsscore { get; set; }

    [Column("InsUser_ID")]
    public Guid InsUserId { get; set; }

    public DateTime CreatedDt { get; set; }

    public DateTime? UpdatedDt { get; set; }

    public bool IsDeleted { get; set; }

    public long Version { get; set; }

    [ForeignKey("HospitalizationId")]
    [InverseProperty("VitalSigns")]
    public virtual Hospitalization? Hospitalization { get; set; }

    [ForeignKey("InsUserId")]
    [InverseProperty("VitalSigns")]
    public virtual User? InsUser { get; set; }
}
