using MedObhod.Backend.DTOs;

namespace MedObhod.Backend.Services;

public interface IDataSyncService
{
    Task<List<PatientSyncDto>> GetChangedPatientsAsync(DateTime? since);
    Task<List<PatientDiagnosisSyncDto>> GetChangedPatientDiagnosesAsync(DateTime? since);
    Task<Guid> UpsertPatientDiagnosisAsync(PatientDiagnosisSyncDto dto);

    Task<List<HospitalizationSyncDto>> GetChangedHospitalizationsAsync(DateTime? since);
    Task<bool> UpdateHospitalizationAsync(Guid hospitalizationId, UpdateHospitalizationRequest request);

    Task<List<AppointmentSyncFullDto>> GetAppointmentsWithExecutionsAsync(Guid? hospitalizationId, DateTime? since);
    Task<bool> UpdateAppointmentAsync(Guid appointmentId, UpdateAppointmentRequest request);
    Task<bool> UpdateExecutionStatusAsync(Guid executionId, string status, Guid userId);
    Task<Guid> CreateAppointmentWithExecutionsAsync(CreateAppointmentRequest request, Guid userId);

    Task<List<VitalSignSyncDto>> GetChangedVitalSignsAsync(Guid? hospitalizationId, DateTime? since);
    Task<Guid> CreateVitalSignAsync(CreateVitalSignRequest request, Guid userId);

    Task<List<DoctorNoteSyncDto>> GetChangedDoctorNotesAsync(DateTime? since);

    Task<Guid> CreateDoctorNoteAsync(CreateDoctorNoteRequest request, Guid doctorId);


    Task<Guid> CompleteRoundAsync(CompleteDoctorRoundRequest request);

    Task<DepartmentAnalyticsDto> GetDepartmentAnalyticsAsync();

}