namespace MedObhod.Backend.DTOs;

public class MedicationResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Form { get; set; } = string.Empty;
    public string DefaultDosage { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public long Version { get; set; }
    public DateTime CreatedDt { get; set; }
    public DateTime? UpdatedDt { get; set; }
}

public class AppointmentTemplateResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int? DurationMin { get; set; }
    public bool RequiresMedication { get; set; }
    public long Version { get; set; }
    public DateTime CreatedDt { get; set; }
    public DateTime? UpdatedDt { get; set; }
}

public class DiagnosisDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}