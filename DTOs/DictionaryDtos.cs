namespace MedObhod.Backend.DTOs;

// ============ Request DTOs ============
public class GetDictionaryRequest
{
    public long? LocalMedicationVersion { get; set; }
    public long? LocalTemplateVersion { get; set; }
}

// ============ Medication DTOs ============
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

public class MedicationCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string Form { get; set; } = string.Empty;
    public string DefaultDosage { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

public class MedicationUpdateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Form { get; set; } = string.Empty;
    public string DefaultDosage { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

// ============ AppointmentTemplate DTOs ============
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

public class AppointmentTemplateCreateDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int DurationMin { get; set; }
    public bool RequiresMedication { get; set; }
    public string Color { get; set; } = string.Empty;
}

public class AppointmentTemplateUpdateDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int DurationMin { get; set; }
    public bool RequiresMedication { get; set; }
    public string Color { get; set; } = string.Empty;
}

// ============ Dictionary Version Response ============
public class DictionaryVersionResponse
{
    public long MedicationsVersion { get; set; }
    public long TemplatesVersion { get; set; }
    public DateTime LastUpdated { get; set; }
}

// ============ Dictionary Sync Response ============
public class DictionarySyncResponse
{
    public List<MedicationResponseDto> Medications { get; set; } = new();
    public List<AppointmentTemplateResponseDto> Templates { get; set; } = new();
    public bool HasChanges { get; set; }
}

public class DiagnosisDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}