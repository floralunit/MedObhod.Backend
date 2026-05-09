using MedObhod.Backend.DTOs;

namespace MedObhod.Backend.Services;

public interface IDataSyncService
{
    Task<SyncResponse> ProcessSyncAsync(SyncRequest request);

    Task<List<DoctorNoteSyncDto>> GetChangedDoctorNotesAsync(DateTime? since);

    Task<List<PatientSyncDto>> GetChangedPatientsAsync(DateTime? since);
    Task<List<HospitalizationSyncDto>> GetChangedHospitalizationsAsync(DateTime? since);

    //Task<Guid> UpsertPatientAsync(PatientSyncDto patient);
    Task<Guid> UpsertHospitalizationAsync(HospitalizationSyncDto hospitalization);
    Task<Guid> UpsertVitalSignAsync(VitalSignSyncDto vitalSign);


    Task<Guid> UpsertAppointmentAsync(AppointmentSyncDto appointment);
    Task<Guid> UpsertDoctorNoteAsync(DoctorNoteSyncDto note);

    Task<List<VitalSignSyncDto>> GetChangedVitalSignsAsync(Guid? hospitalizationId, DateTime? since);
    Task<List<AppointmentSyncDto>> GetChangedAppointmentsAsync(Guid? hospitalizationId, DateTime? since);



    Task<bool> DeleteEntityAsync(string entityName, Guid id);
    Task<List<UserSyncDto>> GetUsersForSyncAsync(DateTime? since);

    Task<bool> UpdateAppointmentStatusAsync(Guid appointmentId, string status, Guid? completedBy);

    Task<List<PatientDiagnosisSyncDto>> GetChangedPatientDiagnosesAsync(DateTime? since);
    Task<Guid> UpsertPatientDiagnosisAsync(PatientDiagnosisSyncDto patientDiagnosis);
}