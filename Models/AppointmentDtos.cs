namespace MedObhod.Backend.DTOs;

public class AppointmentResponseDto
{
    public Guid Id { get; set; }
    public Guid HospitalizationId { get; set; }
    public string TemplateId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public int DurationMin { get; set; }
    public string? Instructions { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateAppointmentRequest
{
    public Guid HospitalizationId { get; set; }
    public string TemplateId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public int DurationMin { get; set; }
    public string? Instructions { get; set; }
    public string? Notes { get; set; }
    public AppointmentScheduleDto? Schedule { get; set; }
    public AppointmentMedicationDto? Medication { get; set; }
}