using MedObhod.Backend.DTOs;

namespace MedObhod.Backend.Services;

public interface IDictionaryService
{
    Task<List<MedicationResponseDto>> GetAllMedicationsAsync();
    Task<MedicationResponseDto?> GetMedicationByIdAsync(Guid id);

    Task<List<AppointmentTemplateResponseDto>> GetAllTemplatesAsync();
    Task<AppointmentTemplateResponseDto?> GetTemplateByIdAsync(string id);

    Task<List<DiagnosisDto>> GetAllDiagnosesAsync();
}