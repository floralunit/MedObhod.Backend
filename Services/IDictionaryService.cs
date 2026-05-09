using MedObhod.Backend.DTOs;

namespace MedObhod.Backend.Services;

public interface IDictionaryService
{
    // Version and sync
    Task<DictionaryVersionResponse> GetCurrentVersionAsync();
    Task<DictionarySyncResponse> GetChangedDictionaryAsync(long? localMedicationVersion, long? localTemplateVersion);
    Task<(List<MedicationResponseDto> Medications, List<AppointmentTemplateResponseDto> Templates)> GetFullDictionaryAsync();

    // Medication CRUD
    Task<List<MedicationResponseDto>> GetAllMedicationsAsync();
    Task<MedicationResponseDto?> GetMedicationByIdAsync(Guid id);
    Task<MedicationResponseDto> CreateMedicationAsync(MedicationCreateDto dto);
    Task<MedicationResponseDto?> UpdateMedicationAsync(MedicationUpdateDto dto);
    Task<bool> DeleteMedicationAsync(Guid id);

    // Template CRUD
    Task<List<AppointmentTemplateResponseDto>> GetAllTemplatesAsync();
    Task<AppointmentTemplateResponseDto?> GetTemplateByIdAsync(string id);
    Task<AppointmentTemplateResponseDto> CreateTemplateAsync(AppointmentTemplateCreateDto dto);
    Task<AppointmentTemplateResponseDto?> UpdateTemplateAsync(AppointmentTemplateUpdateDto dto);
    Task<bool> DeleteTemplateAsync(string id);

    Task<List<DiagnosisDto>> GetAllDiagnosesAsync();
}