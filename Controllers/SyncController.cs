using MedObhod.Backend.DTOs;
using MedObhod.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MedObhod.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SyncController : ControllerBase
{
    private readonly IDataSyncService _syncService;
    private readonly ILogger<SyncController> _logger;

    public SyncController(IDataSyncService syncService, ILogger<SyncController> logger)
    {
        _syncService = syncService;
        _logger = logger;
    }

    /// <summary>
    /// Получить пациентов для синхронизации
    /// </summary>
    [HttpGet("patients")]
    [Authorize]
    [ProducesResponseType(typeof(BaseResponse<List<PatientSyncDto>>), 200)]
    public async Task<IActionResult> GetPatientsForSync([FromQuery] DateTime? since)
    {
        try
        {
            var patients = await _syncService.GetChangedPatientsAsync(since);
            return Ok(BaseResponse<List<PatientSyncDto>>.Ok(patients));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get patients for sync error");
            return StatusCode(500, BaseResponse<object>.Error("Internal server error", 500));
        }
    }

    /// <summary>
    /// Получить связи пациент-диагноз для синхронизации
    /// </summary>
    [HttpGet("patientDiagnoses")]
    [Authorize]
    [ProducesResponseType(typeof(BaseResponse<List<PatientDiagnosisSyncDto>>), 200)]
    public async Task<IActionResult> GetPatientDiagnosesForSync([FromQuery] DateTime? since)
    {
        try
        {
            var patientDiagnoses = await _syncService.GetChangedPatientDiagnosesAsync(since);
            return Ok(BaseResponse<List<PatientDiagnosisSyncDto>>.Ok(patientDiagnoses));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get patient diagnoses sync error");
            return StatusCode(500, BaseResponse<object>.Error("Internal server error", 500));
        }
    }

    /// <summary>
    /// Создать связь пациент-диагноз
    /// </summary>
    [HttpPost("patientDiagnoses")]
    [Authorize(Roles = "doctor,head")]
    [ProducesResponseType(typeof(BaseResponse<object>), 201)]
    public async Task<IActionResult> CreatePatientDiagnosis([FromBody] PatientDiagnosisSyncDto dto)
    {
        try
        {
            var id = await _syncService.UpsertPatientDiagnosisAsync(dto);
            return Ok(BaseResponse<object>.Ok(new { id }, "Patient diagnosis created"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Create patient diagnosis error");
            return StatusCode(500, BaseResponse<object>.Error("Internal server error", 500));
        }
    }

    /// <summary>
    /// Обновить связь пациент-диагноз
    /// </summary>
    [HttpPut("patientDiagnoses/{id}")]
    [Authorize(Roles = "doctor,head")]
    [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
    [ProducesResponseType(typeof(BaseResponse<object>), 404)]
    public async Task<IActionResult> UpdatePatientDiagnosis(Guid id, [FromBody] PatientDiagnosisSyncDto dto)
    {
        try
        {
            dto.Id = id;
            var result = await _syncService.UpsertPatientDiagnosisAsync(dto);
            return Ok(BaseResponse<bool>.Ok(true, "Patient diagnosis updated"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Update patient diagnosis error");
            return StatusCode(500, BaseResponse<object>.Error("Internal server error", 500));
        }
    }


    /// <summary>
    /// Получить госпитализации для синхронизации
    /// </summary>
    [HttpGet("hospitalizations")]
    [Authorize]
    [ProducesResponseType(typeof(BaseResponse<List<HospitalizationSyncDto>>), 200)]
    public async Task<IActionResult> GetHospitalizationsForSync([FromQuery] DateTime? since)
    {
        try
        {
            var hospitalizations = await _syncService.GetChangedHospitalizationsAsync(since);
            return Ok(BaseResponse<List<HospitalizationSyncDto>>.Ok(hospitalizations));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get hospitalizations for sync error");
            return StatusCode(500, BaseResponse<object>.Error("Internal server error", 500));
        }
    }

    /// <summary>
    /// Обновить госпитализацию (назначить врача)
    /// </summary>
    [HttpPut("hospitalizations/{hospitalizationId}")]
    [Authorize(Roles = "head")]
    [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
    public async Task<IActionResult> UpdateHospitalization(Guid hospitalizationId, [FromBody] UpdateHospitalizationRequest request)
    {
        try
        {
            var result = await _syncService.UpdateHospitalizationAsync(hospitalizationId, request);
            if (!result)
                return NotFound(BaseResponse<object>.NotFound("Hospitalization not found"));
            return Ok(BaseResponse<bool>.Ok(true, "Hospitalization updated"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Update hospitalization error");
            return StatusCode(500, BaseResponse<object>.Error("Internal server error", 500));
        }
    }

    /// <summary>
    /// Получить витальные показатели для синхронизации
    /// </summary>
    [HttpGet("vitalSigns")]
    [Authorize]
    [ProducesResponseType(typeof(BaseResponse<List<VitalSignSyncDto>>), 200)]
    public async Task<IActionResult> GetVitalSignsForSync([FromQuery] Guid? hospitalizationId, [FromQuery] DateTime? since)
    {
        try
        {
            var vitalSigns = await _syncService.GetChangedVitalSignsAsync(hospitalizationId, since);
            return Ok(BaseResponse<List<VitalSignSyncDto>>.Ok(vitalSigns));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get vital signs for sync error");
            return StatusCode(500, BaseResponse<object>.Error("Internal server error", 500));
        }
    }

    /// <summary>
    /// Создать новые витальные показатели
    /// </summary>
    [HttpPost("vitalSigns")]
    [Authorize]
    [ProducesResponseType(typeof(BaseResponse<object>), 201)]
    [ProducesResponseType(typeof(BaseResponse<object>), 400)]
    public async Task<IActionResult> CreateVitalSign([FromBody] CreateVitalSignRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var vitalSignId = await _syncService.CreateVitalSignAsync(request, userId);

            return Ok(BaseResponse<object>.Ok(new { id = vitalSignId }, "Vital signs created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Create vital sign error");
            return StatusCode(500, BaseResponse<object>.Error("Internal server error", 500));
        }
    }

    /// <summary>
    /// Получить назначения с executions для синхронизации
    /// </summary>
    [HttpGet("appointments")]
    [Authorize]
    [ProducesResponseType(typeof(BaseResponse<List<AppointmentSyncFullDto>>), 200)]
    public async Task<IActionResult> GetAppointmentsForSync([FromQuery] Guid? hospitalizationId, [FromQuery] DateTime? since)
    {
        try
        {
            var appointments = await _syncService.GetAppointmentsWithExecutionsAsync(hospitalizationId, since);
            return Ok(BaseResponse<List<AppointmentSyncFullDto>>.Ok(appointments));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get appointments for sync error");
            return StatusCode(500, BaseResponse<object>.Error("Internal server error", 500));
        }
    }

    // SyncController.cs

    /// <summary>
    /// Обновить назначение (статус, завершение)
    /// </summary>
    [HttpPut("appointments/{appointmentId}")]
    [Authorize(Roles = "doctor,head,nurse")]
    [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
    public async Task<IActionResult> UpdateAppointment(Guid appointmentId, [FromBody] UpdateAppointmentRequest request)
    {
        try
        {
            var result = await _syncService.UpdateAppointmentAsync(appointmentId, request);
            if (!result)
                return NotFound(BaseResponse<object>.NotFound("Appointment not found"));

            return Ok(BaseResponse<bool>.Ok(true, "Appointment updated"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Update appointment error");
            return StatusCode(500, BaseResponse<object>.Error("Internal server error", 500));
        }
    }

    /// <summary>
    /// Создать назначение
    /// </summary>
    [HttpPost("appointments")]
    [Authorize(Roles = "doctor,head")]
    [ProducesResponseType(typeof(BaseResponse<object>), 201)]
    public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var appointmentId = await _syncService.CreateAppointmentWithExecutionsAsync(request, userId);
            return Ok(BaseResponse<object>.Ok(new { id = appointmentId }, "Appointment created with executions"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Create appointment error");
            return StatusCode(500, BaseResponse<object>.Error("Internal server error", 500));
        }
    }

    /// <summary>
    /// Обновить статус выполнения (от медсестры)
    /// </summary>
    [HttpPut("appointmentExecutions/{executionId}")]
    [Authorize(Roles = "nurse,doctor,head")]
    [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
    public async Task<IActionResult> UpdateExecutionStatus(Guid executionId, [FromBody] UpdateExecutionRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var result = await _syncService.UpdateExecutionStatusAsync(executionId, request.Status, userId);

            if (!result) return NotFound(BaseResponse<object>.NotFound("Execution not found"));

            return Ok(BaseResponse<bool>.Ok(true, "Execution updated"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Update execution error");
            return StatusCode(500, BaseResponse<object>.Error("Internal server error", 500));
        }
    }


    /// <summary>
    /// Получить заметки врача для синхронизации
    /// </summary>
    [HttpGet("doctorNotes")]
    [Authorize]
    [ProducesResponseType(typeof(BaseResponse<List<DoctorNoteSyncDto>>), 200)]
    public async Task<IActionResult> GetDoctorNotesForSync([FromQuery] DateTime? since)
    {
        try
        {
            var notes = await _syncService.GetChangedDoctorNotesAsync(since);
            return Ok(BaseResponse<List<DoctorNoteSyncDto>>.Ok(notes));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get doctor notes sync error");
            return StatusCode(500, BaseResponse<object>.Error("Internal server error", 500));
        }
    }

    /// <summary>
    /// Создать заметку врача
    /// </summary>
    [HttpPost("doctorNotes")]
    [Authorize]
    [ProducesResponseType(typeof(BaseResponse<object>), 201)]
    public async Task<IActionResult> CreateDoctorNote([FromBody] CreateDoctorNoteRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var noteId = await _syncService.CreateDoctorNoteAsync(request, userId);
            return Ok(BaseResponse<object>.Ok(new { id = noteId }, "Doctor note created"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Create doctor note error");
            return StatusCode(500, BaseResponse<object>.Error("Internal server error", 500));
        }
    }

    [HttpPost("doctorRounds/complete")]
    public async Task<IActionResult> CompleteRound(
        [FromBody] CompleteDoctorRoundRequest request)
    {
        var id =
            await _syncService
                .CompleteRoundAsync(request);

        return Ok(new
        {
            success = true,
            data = new
            {
                id
            }
        });
    }

    /// <summary>
    /// Получить аналитику отделения
    /// </summary>
    [HttpGet("analytics")]
    [Authorize(Roles = "head")]
    [ProducesResponseType(typeof(BaseResponse<DepartmentAnalyticsDto>), 200)]
    public async Task<IActionResult> GetDepartmentAnalytics()
    {
        try
        {
            var analytics = await _syncService.GetDepartmentAnalyticsAsync();
            return Ok(BaseResponse<DepartmentAnalyticsDto>.Ok(analytics, "Аналитика загружена"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get analytics error");
            return StatusCode(500, BaseResponse<object>.Error("Internal server error", 500));
        }
    }
}