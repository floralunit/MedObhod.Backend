namespace MedObhod.Backend.DTOs;

// ============ Request DTOs ============
public class SyncRequest
{
    public Guid UserId { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public DateTime? LastSyncTime { get; set; }
    public List<SyncChangeItem> Changes { get; set; } = new();
}

public class SyncChangeItem
{
    public string EntityName { get; set; } = string.Empty; // Patient, Hospitalization, VitalSign, Appointment
    public string Operation { get; set; } = string.Empty; // INSERT, UPDATE, DELETE
    public Guid LocalId { get; set; }
    public Guid? ServerId { get; set; }
    public string Data { get; set; } = string.Empty; // JSON data
    public DateTime ChangedAt { get; set; }
}

// ============ Response DTOs ============
public class SyncResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime ServerTime { get; set; }
    public List<SyncChangeItem> ServerChanges { get; set; } = new();
    public Dictionary<Guid, Guid> IdMapping { get; set; } = new(); // LocalId -> ServerId
}

// ============ Entity Sync DTOs ============
public class PatientSyncDto
{
    public Guid? Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime? BirthDt { get; set; }
    public string Gender { get; set; } = string.Empty;
    public long Version { get; set; }
    public DateTime? UpdatedDt { get; set; }
    public bool IsDeleted { get; set; }
}

public class HospitalizationSyncDto
{
    public Guid? Id { get; set; }
    public Guid PatientId { get; set; }
    public DateTime AdmissionDt { get; set; }
    public DateTime? DischargeDt { get; set; }
    public string Room { get; set; } = string.Empty;
    public string Bed { get; set; } = string.Empty;
    public Guid? AttendingDoctorId { get; set; }
    public string Status { get; set; } = string.Empty;
    public long Version { get; set; }
    public DateTime? UpdatedDt { get; set; }
    public bool IsDeleted { get; set; }
}

public class VitalSignSyncDto
{
    public Guid Id { get; set; }
    public Guid HospitalizationId { get; set; }
    public double? Temperature { get; set; }
    public int? Pulse { get; set; }
    public int? SystolicBP { get; set; }
    public int? DiastolicBP { get; set; }
    public int? SpO2 { get; set; }
    public int? RespiratoryRate { get; set; }
    public int? NEWSScore { get; set; }
    public Guid? InsUserId { get; set; }
    public long Version { get; set; }
    public DateTime? CreatedDt { get; set; }
    public DateTime? UpdatedDt { get; set; }
    public bool IsDeleted { get; set; }
}

public class AppointmentSyncDto
{
    public Guid? Id { get; set; }
    public Guid HospitalizationId { get; set; }
    public string TemplateId { get; set; } = string.Empty;
    public Guid InsUserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public int DurationMin { get; set; }
    public string? Instructions { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = string.Empty;
    public long Version { get; set; }
    public DateTime? UpdatedDt { get; set; }
}

public class UserSyncDto
{
    public Guid Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public long Version { get; set; }
    public DateTime? UpdatedDt { get; set; }
    public bool IsDeleted { get; set; }
}

public class UpdateAppointmentRequest
{
    public string Status { get; set; } = string.Empty;
    public Guid? CompletedBy { get; set; }
}

public class PatientDiagnosisSyncDto
{
    public Guid Id { get; set; }
    public Guid HospitalizationId { get; set; }
    public Guid DiagnosisId { get; set; }
    public bool IsPrimary { get; set; }
    public long Version { get; set; }
    public DateTime? UpdatedDt { get; set; }
}

public class DoctorNoteSyncDto
{
    public Guid Id { get; set; }
    public Guid HospitalizationId { get; set; }
    public Guid DoctorId { get; set; }
    public string? Complaints { get; set; }
    public string? GeneralCondition { get; set; }
    public string? MentalStatus { get; set; }
    public double? Temperature { get; set; }
    public int? Pulse { get; set; }
    public string? BP { get; set; }
    public int? RespiratoryRate { get; set; }
    public string? ExaminationSummary { get; set; }
    public string? TreatmentEffectiveness { get; set; }
    public string? PlanNote { get; set; }
    public long Version { get; set; }
    public DateTime? UpdatedDt { get; set; }
    public bool IsDeleted { get; set; }
}

public class CreateVitalSignRequest
{
    public Guid HospitalizationId { get; set; }
    public DateTime CreatedDt { get; set; }
    public double? Temperature { get; set; }
    public int? Pulse { get; set; }
    public int? SystolicBP { get; set; }
    public int? DiastolicBP { get; set; }
    public int? SpO2 { get; set; }
    public int? RespiratoryRate { get; set; }
}

public class CreateDoctorNoteRequest
{
    public Guid HospitalizationId { get; set; }
    public string? Complaints { get; set; }
    public string? ExaminationSummary { get; set; }
    public string? TreatmentEffectiveness { get; set; }
    public string? Notes { get; set; }
}

public class CompleteDoctorRoundRequest
{
    public Guid DoctorId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = "completed";

    public List<CompleteDoctorRoundItemRequest> Items { get; set; }
        = new();
}

public class CompleteDoctorRoundItemRequest
{
    public Guid HospitalizationId { get; set; }

    public int OrderIndex { get; set; }

    public DateTime? PlannedTime { get; set; }

    public DateTime? StartVisitTime { get; set; }

    public DateTime? EndVisitTime { get; set; }

    public string Status { get; set; } = "pending";
}