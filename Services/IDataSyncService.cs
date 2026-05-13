using MedObhod.Backend.DTOs;

namespace MedObhod.Backend.Services;

public interface IDataSyncService
{
    Task<List<PatientSyncDto>> GetChangedPatientsAsync(DateTime? since);
    Task<List<HospitalizationSyncDto>> GetChangedHospitalizationsAsync(DateTime? since);

    //Task<Guid> UpsertPatientAsync(PatientSyncDto patient);
    Task<Guid> UpsertHospitalizationAsync(HospitalizationSyncDto hospitalization);


    Task<Guid> UpsertAppointmentAsync(AppointmentSyncDto appointment);

    Task<List<VitalSignSyncDto>> GetChangedVitalSignsAsync(Guid? hospitalizationId, DateTime? since);
    Task<Guid> CreateVitalSignAsync(CreateVitalSignRequest request, Guid userId);

    Task<List<AppointmentSyncDto>> GetChangedAppointmentsAsync(Guid? hospitalizationId, DateTime? since);



    Task<bool> DeleteEntityAsync(string entityName, Guid id);
    Task<List<UserSyncDto>> GetUsersForSyncAsync(DateTime? since);

    Task<bool> UpdateAppointmentStatusAsync(Guid appointmentId, string status, Guid? completedBy);

    Task<List<PatientDiagnosisSyncDto>> GetChangedPatientDiagnosesAsync(DateTime? since);

    Task<List<DoctorNoteSyncDto>> GetChangedDoctorNotesAsync(DateTime? since);

    Task<Guid> CreateDoctorNoteAsync(CreateDoctorNoteRequest request, Guid doctorId);

    Task<Guid> CompleteRoundAsync(CompleteDoctorRoundRequest request);

}