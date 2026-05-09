using MedObhod.Backend.DTOs;
using MedObhod.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    /// Get current dictionary versions (for mobile app to check if update needed)
    /// </summary>
    [HttpGet("version")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<DictionaryVersionResponse>), 200)]
    public async Task<IActionResult> GetVersion()
    {
        try
        {
            var version = await _dictionaryService.GetCurrentVersionAsync();
            return Ok(BaseResponse<DictionaryVersionResponse>.Ok(version));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dictionary version");
            return StatusCode(500, BaseResponse<DictionaryVersionResponse>.Error("Internal server error", 500));
        }
    }

    /// <summary>
    /// Get dictionary changes since specific version (incremental sync)
    /// </summary>
    [HttpPost("sync")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<DictionarySyncResponse>), 200)]
    public async Task<IActionResult> SyncDictionary([FromBody] GetDictionaryRequest request)
    {
        try
        {
            var changes = await _dictionaryService.GetChangedDictionaryAsync(
                request.LocalMedicationVersion,
                request.LocalTemplateVersion);

            return Ok(BaseResponse<DictionarySyncResponse>.Ok(changes));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing dictionary");
            return StatusCode(500, BaseResponse<DictionarySyncResponse>.Error("Internal server error", 500));
        }
    }

    /// <summary>
    /// Get full dictionary (all medications and templates)
    /// </summary>
    [HttpGet("all")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<object>), 200)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var (medications, templates) = await _dictionaryService.GetFullDictionaryAsync();

            return Ok(BaseResponse<object>.Ok(new { medications, templates }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting full dictionary");
            return StatusCode(500, BaseResponse<object>.Error("Internal server error", 500));
        }
    }

    // ==================== MEDICATIONS ENDPOINTS ====================

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
    /// Create new medication (Admin only)
    /// </summary>
    [HttpPost("medications")]
    [Authorize(Roles = "head,admin")]
    [ProducesResponseType(typeof(BaseResponse<MedicationResponseDto>), 201)]
    [ProducesResponseType(typeof(BaseResponse<object>), 400)]
    public async Task<IActionResult> CreateMedication([FromBody] MedicationCreateDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(BaseResponse<object>.Error("Invalid model state", 400));

            var medication = await _dictionaryService.CreateMedicationAsync(dto);
            return CreatedAtAction(nameof(GetMedicationById), new { id = medication.Id },
                BaseResponse<MedicationResponseDto>.Created(medication, "Medication created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating medication");
            return StatusCode(500, BaseResponse<MedicationResponseDto>.Error("Internal server error", 500));
        }
    }

    /// <summary>
    /// Update medication (Admin only)
    /// </summary>
    [HttpPut("medications")]
    [Authorize(Roles = "head,admin")]
    [ProducesResponseType(typeof(BaseResponse<MedicationResponseDto>), 200)]
    [ProducesResponseType(typeof(BaseResponse<object>), 404)]
    [ProducesResponseType(typeof(BaseResponse<object>), 400)]
    public async Task<IActionResult> UpdateMedication([FromBody] MedicationUpdateDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(BaseResponse<object>.Error("Invalid model state", 400));

            var medication = await _dictionaryService.UpdateMedicationAsync(dto);

            if (medication == null)
                return NotFound(BaseResponse<object>.NotFound($"Medication with ID {dto.Id} not found"));

            return Ok(BaseResponse<MedicationResponseDto>.Ok(medication, "Medication updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating medication {Id}", dto.Id);
            return StatusCode(500, BaseResponse<MedicationResponseDto>.Error("Internal server error", 500));
        }
    }

    /// <summary>
    /// Delete medication (Admin only)
    /// </summary>
    [HttpDelete("medications/{id}")]
    [Authorize(Roles = "head,admin")]
    [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
    [ProducesResponseType(typeof(BaseResponse<object>), 404)]
    public async Task<IActionResult> DeleteMedication(Guid id)
    {
        try
        {
            var result = await _dictionaryService.DeleteMedicationAsync(id);

            if (!result)
                return NotFound(BaseResponse<object>.NotFound($"Medication with ID {id} not found"));

            return Ok(BaseResponse<bool>.Ok(true, "Medication deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting medication {Id}", id);
            return StatusCode(500, BaseResponse<bool>.Error("Internal server error", 500));
        }
    }

    // ==================== TEMPLATES ENDPOINTS ====================

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
    /// Create new appointment template (Admin only)
    /// </summary>
    [HttpPost("templates")]
    [Authorize(Roles = "head,admin")]
    [ProducesResponseType(typeof(BaseResponse<AppointmentTemplateResponseDto>), 201)]
    [ProducesResponseType(typeof(BaseResponse<object>), 400)]
    public async Task<IActionResult> CreateTemplate([FromBody] AppointmentTemplateCreateDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(BaseResponse<object>.Error("Invalid model state", 400));

            var template = await _dictionaryService.CreateTemplateAsync(dto);
            return CreatedAtAction(nameof(GetTemplateById), new { id = template.Id },
                BaseResponse<AppointmentTemplateResponseDto>.Created(template, "Template created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating template");
            return StatusCode(500, BaseResponse<AppointmentTemplateResponseDto>.Error("Internal server error", 500));
        }
    }

    /// <summary>
    /// Update appointment template (Admin only)
    /// </summary>
    [HttpPut("templates")]
    [Authorize(Roles = "head,admin")]
    [ProducesResponseType(typeof(BaseResponse<AppointmentTemplateResponseDto>), 200)]
    [ProducesResponseType(typeof(BaseResponse<object>), 404)]
    [ProducesResponseType(typeof(BaseResponse<object>), 400)]
    public async Task<IActionResult> UpdateTemplate([FromBody] AppointmentTemplateUpdateDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(BaseResponse<object>.Error("Invalid model state", 400));

            var template = await _dictionaryService.UpdateTemplateAsync(dto);

            if (template == null)
                return NotFound(BaseResponse<object>.NotFound($"Template with ID {dto.Id} not found"));

            return Ok(BaseResponse<AppointmentTemplateResponseDto>.Ok(template, "Template updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating template {Id}", dto.Id);
            return StatusCode(500, BaseResponse<AppointmentTemplateResponseDto>.Error("Internal server error", 500));
        }
    }

    /// <summary>
    /// Delete appointment template (Admin only)
    /// </summary>
    [HttpDelete("templates/{id}")]
    [Authorize(Roles = "head,admin")]
    [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
    [ProducesResponseType(typeof(BaseResponse<object>), 404)]
    public async Task<IActionResult> DeleteTemplate(string id)
    {
        try
        {
            var result = await _dictionaryService.DeleteTemplateAsync(id);

            if (!result)
                return NotFound(BaseResponse<object>.NotFound($"Template with ID {id} not found"));

            return Ok(BaseResponse<bool>.Ok(true, "Template deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting template {Id}", id);
            return StatusCode(500, BaseResponse<bool>.Error("Internal server error", 500));
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