using MedObhod.Backend.DTOs;
using MedObhod.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedObhod.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DictionaryController : ControllerBase
{
    private readonly IDictionaryService _dictionaryService;
    private readonly ILogger<DictionaryController> _logger;

    public DictionaryController(IDictionaryService dictionaryService, ILogger<DictionaryController> logger)
    {
        _dictionaryService = dictionaryService;
        _logger = logger;
    }

    /// <summary>
    /// Get all medications
    /// </summary>
    [HttpGet("medications")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<List<MedicationResponseDto>>), 200)]
    public async Task<IActionResult> GetAllMedications()
    {
        try
        {
            var medications = await _dictionaryService.GetAllMedicationsAsync();
            return Ok(BaseResponse<List<MedicationResponseDto>>.Ok(medications));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all medications");
            return StatusCode(500, BaseResponse<List<MedicationResponseDto>>.Error("Internal server error", 500));
        }
    }

    /// <summary>
    /// Get medication by ID
    /// </summary>
    [HttpGet("medications/{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<MedicationResponseDto>), 200)]
    [ProducesResponseType(typeof(BaseResponse<object>), 404)]
    public async Task<IActionResult> GetMedicationById(Guid id)
    {
        try
        {
            var medication = await _dictionaryService.GetMedicationByIdAsync(id);

            if (medication == null)
                return NotFound(BaseResponse<object>.NotFound($"Medication with ID {id} not found"));

            return Ok(BaseResponse<MedicationResponseDto>.Ok(medication));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting medication {Id}", id);
            return StatusCode(500, BaseResponse<MedicationResponseDto>.Error("Internal server error", 500));
        }
    }

    /// <summary>
    /// Get all appointment templates
    /// </summary>
    [HttpGet("templates")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<List<AppointmentTemplateResponseDto>>), 200)]
    public async Task<IActionResult> GetAllTemplates()
    {
        try
        {
            var templates = await _dictionaryService.GetAllTemplatesAsync();
            return Ok(BaseResponse<List<AppointmentTemplateResponseDto>>.Ok(templates));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all templates");
            return StatusCode(500, BaseResponse<List<AppointmentTemplateResponseDto>>.Error("Internal server error", 500));
        }
    }

    /// <summary>
    /// Get template by ID
    /// </summary>
    [HttpGet("templates/{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<AppointmentTemplateResponseDto>), 200)]
    [ProducesResponseType(typeof(BaseResponse<object>), 404)]
    public async Task<IActionResult> GetTemplateById(string id)
    {
        try
        {
            var template = await _dictionaryService.GetTemplateByIdAsync(id);

            if (template == null)
                return NotFound(BaseResponse<object>.NotFound($"Template with ID {id} not found"));

            return Ok(BaseResponse<AppointmentTemplateResponseDto>.Ok(template));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template {Id}", id);
            return StatusCode(500, BaseResponse<AppointmentTemplateResponseDto>.Error("Internal server error", 500));
        }
    }

    /// <summary>
    /// Получить все диагнозы
    /// </summary>
    [HttpGet("diagnoses")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<List<DiagnosisDto>>), 200)]
    public async Task<IActionResult> GetAllDiagnoses()
    {
        try
        {
            var diagnoses = await _dictionaryService.GetAllDiagnosesAsync();
            return Ok(BaseResponse<List<DiagnosisDto>>.Ok(diagnoses));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all templates");
            return StatusCode(500, BaseResponse<List<AppointmentTemplateResponseDto>>.Error("Internal server error", 500));
        }
    }
}