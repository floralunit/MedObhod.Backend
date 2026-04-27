using MedObhod.Backend.DTOs;

namespace MedObhod.Backend.Services;

public interface IDataSyncService
{
    Task<SyncResponse> ProcessSyncAsync(SyncRequest request);
    Task<List<PatientSyncDto>> GetChangedPatientsAsync(DateTime? since);
    Task<List<HospitalizationSyncDto>> GetChangedHospitalizationsAsync(DateTime? since);
    Task<List<VitalSignSyncDto>> GetChangedVitalSignsAsync(DateTime? since);
    Task<List<AppointmentSyncDto>> GetChangedAppointmentsAsync(DateTime? since);
    Task<List<DoctorNoteSyncDto>> GetChangedDoctorNotesAsync(DateTime? since);

    // Apply changes from client
    Task<Guid> UpsertPatientAsync(PatientSyncDto patient, Guid clientId);
    Task<Guid> UpsertHospitalizationAsync(HospitalizationSyncDto hospitalization);
    Task<Guid> UpsertVitalSignAsync(VitalSignSyncDto vitalSign);
    Task<Guid> UpsertAppointmentAsync(AppointmentSyncDto appointment);
    Task<Guid> UpsertDoctorNoteAsync(DoctorNoteSyncDto note);
    Task<bool> DeleteEntityAsync(string entityName, Guid id);
}