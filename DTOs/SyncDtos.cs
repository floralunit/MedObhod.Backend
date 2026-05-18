namespace MedObhod.Backend.DTOs;

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
public class PatientDiagnosisSyncDto
{
    public Guid Id { get; set; }
    public Guid HospitalizationId { get; set; }
    public Guid DiagnosisId { get; set; }
    public bool IsPrimary { get; set; }
    public long Version { get; set; }
    public DateTime? UpdatedDt { get; set; }
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

public class UpdateHospitalizationRequest
{
    public Guid? AttendingDoctorId { get; set; }
    public string? Status { get; set; }
    public string? Room { get; set; }
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

public class AppointmentSyncDto
{
    public Guid Id { get; set; }
    public Guid HospitalizationId { get; set; }
    public string? TemplateId { get; set; }
    public Guid InsUserId { get; set; }
    public string? Type { get; set; }
    public string? Name { get; set; }
    public string? Priority { get; set; }
    public int DurationMin { get; set; }
    public string? Instructions { get; set; }
    public string? Notes { get; set; }
    public string? Status { get; set; }
    public long Version { get; set; }
    public DateTime CreatedDt { get; set; }
    public DateTime? UpdatedDt { get; set; }
    public bool IsDeleted { get; set; }

    // Связанные данные
    public AppointmentScheduleSyncDto? Schedule { get; set; }
    public AppointmentMedicationSyncDto? Medication { get; set; }
}

public class UpdateAppointmentRequest
{
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public string? Instructions { get; set; }
    public string? Notes { get; set; }
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
public class AppointmentScheduleDto
{
    public string Frequency { get; set; } = string.Empty;
    public string StartDate { get; set; } = string.Empty;
    public string? EndDate { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string? RelationToMeal { get; set; }
    public List<string> Times { get; set; } = new();
}

public class AppointmentMedicationDto
{
    public Guid? Id { get; set; }
    public string? CustomName { get; set; }
    public string? Dosage { get; set; }
    public string? Form { get; set; }
}

public class AppointmentScheduleSyncDto
{
    public Guid Id { get; set; }
    public string? Frequency { get; set; }
    public DateTime StartDt { get; set; }
    public DateTime? EndDt { get; set; }
    public TimeSpan? StartTime { get; set; }
    public string? RelationToMeal { get; set; }
    public List<string>? Times { get; set; }
}

public class AppointmentMedicationSyncDto
{
    public Guid Id { get; set; }
    public Guid? MedicationId { get; set; }
    public string? CustomName { get; set; }
    public string? Dosage { get; set; }
    public string? Form { get; set; }
}

public class AppointmentSyncFullDto
{
    public AppointmentSyncDto Appointment { get; set; }
    public List<AppointmentExecutionSyncDto> Executions { get; set; }
}
public class AppointmentExecutionSyncDto
{
    public Guid Id { get; set; }
    public Guid AppointmentId { get; set; }
    public DateTime ScheduledDateTime { get; set; }
    public DateTime? ExecutedAt { get; set; }
    public Guid? ExecutedUserId { get; set; }
    public string Status { get; set; }
    public string Notes { get; set; }
    public DateTime CreatedDt { get; set; }
    public DateTime UpdatedDt { get; set; }
    public bool IsDeleted { get; set; }
    public long Version { get; set; }
}

public class UpdateExecutionRequest
{
    public string Status { get; set; } = "completed";
    public string? Notes { get; set; }
}

public class DepartmentAnalyticsDto
{
    public int TotalPatients { get; set; }
    public int CriticalPatients { get; set; }
    public int WarningPatients { get; set; }
    public int StablePatients { get; set; }
    public int PendingAppointments { get; set; }
    public int CompletedToday { get; set; }
    public int ActiveRounds { get; set; }
    public List<DoctorAnalyticsDto> Doctors { get; set; } = new();
}

public class DoctorAnalyticsDto
{
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; }
    public int PatientsCount { get; set; }
    public int CriticalCount { get; set; }
    public int AppointmentsCount { get; set; }
    public int RoundsCompleted { get; set; }
}