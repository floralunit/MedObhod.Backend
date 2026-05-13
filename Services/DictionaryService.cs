using MedObhod.Backend.Data;
using MedObhod.Backend.DTOs;
using MedObhod.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace MedObhod.Backend.Services;

public class DictionaryService : IDictionaryService
{
    private readonly AppDbContext _context;
    private readonly ILogger<DictionaryService> _logger;

    public DictionaryService(AppDbContext context, ILogger<DictionaryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DictionaryVersionResponse> GetCurrentVersionAsync()
    {
        var medicationsVersion = await _context.Medications
            .Where(m => !m.IsDeleted)
            .MaxAsync(m => (long?)m.Version) ?? 0;

        var templatesVersion = await _context.AppointmentTemplates
            .Where(t => !t.IsDeleted)
            .MaxAsync(t => (long?)t.Version) ?? 0;

        return new DictionaryVersionResponse
        {
            MedicationsVersion = medicationsVersion,
            TemplatesVersion = templatesVersion,
            LastUpdated = DateTime.Now
        };
    }

    public async Task<DictionarySyncResponse> GetChangedDictionaryAsync(long? localMedicationVersion, long? localTemplateVersion)
    {
        var response = new DictionarySyncResponse();

        // Get changed medications (with version > local)
        if (localMedicationVersion.HasValue)
        {
            var medications = await _context.Medications
                .Where(m => !m.IsDeleted && m.Version > localMedicationVersion.Value)
                .OrderBy(m => m.Name)
                .Select(m => new MedicationResponseDto
                {
                    Id = m.MedicationId,
                    Name = m.Name,
                    Form = m.Form ?? string.Empty,
                    DefaultDosage = m.DefaultDosage ?? string.Empty,
                    Category = m.Category ?? string.Empty,
                    Version = m.Version,
                    CreatedDt = m.CreatedDt,
                    UpdatedDt = m.UpdatedDt
                })
                .ToListAsync();

            response.Medications = medications;
        }
        else
        {
            // No local version - return all
            response.Medications = await GetAllMedicationsAsync();
        }

        // Get changed templates
        if (localTemplateVersion.HasValue)
        {
            var templates = await _context.AppointmentTemplates
                .Where(t => !t.IsDeleted && t.Version > localTemplateVersion.Value)
                .OrderBy(t => t.Name)
                .Select(t => new AppointmentTemplateResponseDto
                {
                    Id = t.AppointmentTemplateId,
                    Name = t.Name,
                    Type = t.Type,
                    DurationMin = t.DurationMin,
                    RequiresMedication = t.RequiresMedication,
                    Version = t.Version,
                    CreatedDt = t.CreatedDt,
                    UpdatedDt = t.UpdatedDt
                })
                .ToListAsync();

            response.Templates = templates;
        }
        else
        {
            response.Templates = await GetAllTemplatesAsync();
        }

        response.HasChanges = response.Medications.Any() || response.Templates.Any();

        return response;
    }

    public async Task<(List<MedicationResponseDto> Medications, List<AppointmentTemplateResponseDto> Templates)> GetFullDictionaryAsync()
    {
        var medications = await GetAllMedicationsAsync();
        var templates = await GetAllTemplatesAsync();

        return (medications, templates);
    }

    public async Task<List<MedicationResponseDto>> GetAllMedicationsAsync()
    {
        return await _context.Medications
            .Where(m => !m.IsDeleted)
            .OrderBy(m => m.Name)
            .Select(m => new MedicationResponseDto
            {
                Id = m.MedicationId,
                Name = m.Name,
                Form = m.Form ?? string.Empty,
                DefaultDosage = m.DefaultDosage ?? string.Empty,
                Category = m.Category ?? string.Empty,
                Version = m.Version,
                CreatedDt = m.CreatedDt,
                UpdatedDt = m.UpdatedDt
            })
            .ToListAsync();
    }

    public async Task<MedicationResponseDto?> GetMedicationByIdAsync(Guid id)
    {
        var medication = await _context.Medications
            .FirstOrDefaultAsync(m => m.MedicationId == id && !m.IsDeleted);

        if (medication == null) return null;

        return new MedicationResponseDto
        {
            Id = medication.MedicationId,
            Name = medication.Name,
            Form = medication.Form ?? string.Empty,
            DefaultDosage = medication.DefaultDosage ?? string.Empty,
            Category = medication.Category ?? string.Empty,
            Version = medication.Version,
            CreatedDt = medication.CreatedDt,
            UpdatedDt = medication.UpdatedDt
        };
    }

    public async Task<MedicationResponseDto> CreateMedicationAsync(MedicationCreateDto dto)
    {
        var medication = new Medication
        {
            MedicationId = Guid.NewGuid(),
            Name = dto.Name,
            Form = dto.Form,
            DefaultDosage = dto.DefaultDosage,
            Category = dto.Category,
            CreatedDt = DateTime.Now,
            UpdatedDt = DateTime.Now,
            IsDeleted = false,
            Version = 1
        };

        _context.Medications.Add(medication);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created medication: {Name} (ID: {Id})", medication.Name, medication.MedicationId);

        return new MedicationResponseDto
        {
            Id = medication.MedicationId,
            Name = medication.Name,
            Form = medication.Form ?? string.Empty,
            DefaultDosage = medication.DefaultDosage ?? string.Empty,
            Category = medication.Category ?? string.Empty,
            Version = medication.Version,
            CreatedDt = medication.CreatedDt,
            UpdatedDt = medication.UpdatedDt
        };
    }

    public async Task<MedicationResponseDto?> UpdateMedicationAsync(MedicationUpdateDto dto)
    {
        var medication = await _context.Medications
            .FirstOrDefaultAsync(m => m.MedicationId == dto.Id && !m.IsDeleted);

        if (medication == null) return null;

        medication.Name = dto.Name;
        medication.Form = dto.Form;
        medication.DefaultDosage = dto.DefaultDosage;
        medication.Category = dto.Category;
        medication.UpdatedDt = DateTime.Now;
        medication.Version++;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated medication: {Name} (ID: {Id})", medication.Name, medication.MedicationId);

        return new MedicationResponseDto
        {
            Id = medication.MedicationId,
            Name = medication.Name,
            Form = medication.Form ?? string.Empty,
            DefaultDosage = medication.DefaultDosage ?? string.Empty,
            Category = medication.Category ?? string.Empty,
            Version = medication.Version,
            CreatedDt = medication.CreatedDt,
            UpdatedDt = medication.UpdatedDt
        };
    }

    public async Task<bool> DeleteMedicationAsync(Guid id)
    {
        var medication = await _context.Medications
            .FirstOrDefaultAsync(m => m.MedicationId == id && !m.IsDeleted);

        if (medication == null) return false;

        medication.IsDeleted = true;
        medication.UpdatedDt = DateTime.Now;
        medication.Version++;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted medication: {Name} (ID: {Id})", medication.Name, id);

        return true;
    }

    public async Task<List<AppointmentTemplateResponseDto>> GetAllTemplatesAsync()
    {
        return await _context.AppointmentTemplates
            .Where(t => !t.IsDeleted)
            .OrderBy(t => t.Name)
            .Select(t => new AppointmentTemplateResponseDto
            {
                Id = t.AppointmentTemplateId,
                Name = t.Name,
                Type = t.Type,
                DurationMin = t.DurationMin,
                RequiresMedication = t.RequiresMedication,
                Version = t.Version,
                CreatedDt = t.CreatedDt,
                UpdatedDt = t.UpdatedDt
            })
            .ToListAsync();
    }

    public async Task<AppointmentTemplateResponseDto?> GetTemplateByIdAsync(string id)
    {
        var template = await _context.AppointmentTemplates
            .FirstOrDefaultAsync(t => t.AppointmentTemplateId == id && !t.IsDeleted);

        if (template == null) return null;

        return new AppointmentTemplateResponseDto
        {
            Id = template.AppointmentTemplateId,
            Name = template.Name,
            Type = template.Type,
            DurationMin = template.DurationMin,
            RequiresMedication = template.RequiresMedication,
            Version = template.Version,
            CreatedDt = template.CreatedDt,
            UpdatedDt = template.UpdatedDt
        };
    }

    public async Task<AppointmentTemplateResponseDto> CreateTemplateAsync(AppointmentTemplateCreateDto dto)
    {
        var template = new AppointmentTemplate
        {
            AppointmentTemplateId = dto.Id,
            Name = dto.Name,
            Type = dto.Type,
            DurationMin = dto.DurationMin,
            RequiresMedication = dto.RequiresMedication,
            CreatedDt = DateTime.Now,
            UpdatedDt = DateTime.Now,
            IsDeleted = false,
            Version = 1
        };

        _context.AppointmentTemplates.Add(template);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created template: {Name} (ID: {Id})", template.Name, template.AppointmentTemplateId);

        return new AppointmentTemplateResponseDto
        {
            Id = template.AppointmentTemplateId,
            Name = template.Name,
            Type = template.Type,
            DurationMin = template.DurationMin,
            RequiresMedication = template.RequiresMedication,
            Version = template.Version,
            CreatedDt = template.CreatedDt,
            UpdatedDt = template.UpdatedDt
        };
    }

    public async Task<AppointmentTemplateResponseDto?> UpdateTemplateAsync(AppointmentTemplateUpdateDto dto)
    {
        var template = await _context.AppointmentTemplates
            .FirstOrDefaultAsync(t => t.AppointmentTemplateId == dto.Id && !t.IsDeleted);

        if (template == null) return null;

        template.Name = dto.Name;
        template.Type = dto.Type;
        template.DurationMin = dto.DurationMin;
        template.RequiresMedication = dto.RequiresMedication;
        template.UpdatedDt = DateTime.Now;
        template.Version++;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated template: {Name} (ID: {Id})", template.Name, template.AppointmentTemplateId);

        return new AppointmentTemplateResponseDto
        {
            Id = template.AppointmentTemplateId,
            Name = template.Name,
            Type = template.Type,
            DurationMin = template.DurationMin,
            RequiresMedication = template.RequiresMedication,
            Version = template.Version,
            CreatedDt = template.CreatedDt,
            UpdatedDt = template.UpdatedDt
        };
    }

    public async Task<bool> DeleteTemplateAsync(string id)
    {
        var template = await _context.AppointmentTemplates
            .FirstOrDefaultAsync(t => t.AppointmentTemplateId == id && !t.IsDeleted);

        if (template == null) return false;

        template.IsDeleted = true;
        template.UpdatedDt = DateTime.Now;
        template.Version++;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted template: {Name} (ID: {Id})", template.Name, id);

        return true;
    }

    public async Task<List<DiagnosisDto>> GetAllDiagnosesAsync()
    {
        return await _context.Diagnoses
                .Where(d => !d.IsDeleted)
                .Select(d => new DiagnosisDto
                {
                    Id = d.DiagnoseId,
                    Name = d.Name,
                    Code = d.Code
                })
                .OrderBy(d => d.Name)
                .ToListAsync();
    }
}