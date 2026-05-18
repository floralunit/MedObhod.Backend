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