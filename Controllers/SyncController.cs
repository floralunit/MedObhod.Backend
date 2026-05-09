using MedObhod.Backend.DTOs;
using MedObhod.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    /// Полная синхронизация данных (push + pull)
    /// </summary>
    [HttpPost("full")]
    [ProducesResponseType(typeof(BaseResponse<SyncResponse>), 200)]
    public async Task<IActionResult> FullSync([FromBody] SyncRequest request)
    {
        try
        {
            _logger.LogInformation("Full sync started for device {DeviceId}", request.DeviceId);

            var result = await _syncService.ProcessSyncAsync(request);

            if (result.Success)
            {
                return Ok(BaseResponse<SyncResponse>.Ok(result, "Sync completed successfully"));
            }

            return StatusCode(500, BaseResponse<SyncResponse>.Error(result.Message, 500));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sync failed for device {DeviceId}", request.DeviceId);
            return StatusCode(500, BaseResponse<SyncResponse>.Error("Internal server error", 500));
        }
    }

    /// <summary>
    /// Получить изменения с сервера (pull only)
    /// </summary>
    [HttpPost("pull")]
    [ProducesResponseType(typeof(BaseResponse<SyncResponse>), 200)]
    public async Task<IActionResult> PullChanges([FromBody] DateTime? lastSyncTime)
    {
        try
        {
            var changes = new SyncResponse
            {
                Success = true,
                ServerTime = DateTime.UtcNow,
                ServerChanges = new List<SyncChangeItem>()
            };

            var patients = await _syncService.GetChangedPatientsAsync(lastSyncTime);
            foreach (var patient in patients)
            {
                changes.ServerChanges.Add(new SyncChangeItem
                {
                    EntityName = "Patient",
                    Operation = patient.Id.HasValue ? "UPDATE" : "INSERT",
                    ServerId = patient.Id,
                    Data = System.Text.Json.JsonSerializer.Serialize(patient)
                });
            }

            var appointments = await _syncService.GetChangedAppointmentsAsync(null, lastSyncTime);
            foreach (var appointment in appointments)
            {
                changes.ServerChanges.Add(new SyncChangeItem
                {
                    EntityName = "Appointment",
                    Operation = appointment.Id.HasValue ? "UPDATE" : "INSERT",
                    ServerId = appointment.Id,
                    Data = System.Text.Json.JsonSerializer.Serialize(appointment)
                });
            }

            return Ok(BaseResponse<SyncResponse>.Ok(changes));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Pull failed");
            return StatusCode(500, BaseResponse<SyncResponse>.Error("Internal server error", 500));
        }
    }

    /// <summary>
    /// Отправить изменения на сервер (push only)
    /// </summary>
    [HttpPost("push")]
    [ProducesResponseType(typeof(BaseResponse<SyncResponse>), 200)]
    public async Task<IActionResult> PushChanges([FromBody] List<SyncChangeItem> changes)
    {
        try
        {
            var request = new SyncRequest
            {
                DeviceId = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                Changes = changes
            };

            var result = await _syncService.ProcessSyncAsync(request);

            if (result.Success)
            {
                return Ok(BaseResponse<SyncResponse>.Ok(result, "Push completed successfully"));
            }

            return StatusCode(500, BaseResponse<SyncResponse>.Error(result.Message, 500));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Push failed");
            return StatusCode(500, BaseResponse<SyncResponse>.Error("Internal server error", 500));
        }
    }

    /// <summary>
    /// Получить всех пользователей для синхронизации
    /// </summary>
    [HttpGet("users")]
    [Authorize(Roles = "head")]
    [ProducesResponseType(typeof(BaseResponse<List<UserSyncDto>>), 200)]
    public async Task<IActionResult> GetUsersForSync([FromQuery] DateTime? since)
    {
        try
        {
            var users = await _syncService.GetUsersForSyncAsync(since);
            return Ok(BaseResponse<List<UserSyncDto>>.Ok(users));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get users for sync error");
            return StatusCode(500, BaseResponse<object>.Error("Internal server error", 500));
        }
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
    /// Получить назначения для синхронизации
    /// </summary>
    [HttpGet("appointments")]
    [Authorize]
    [ProducesResponseType(typeof(BaseResponse<List<AppointmentSyncDto>>), 200)]
    public async Task<IActionResult> GetAppointmentsForSync([FromQuery] Guid? hospitalizationId, [FromQuery] DateTime? since)
    {
        try
        {
            var appointments = await _syncService.GetChangedAppointmentsAsync(hospitalizationId, since);
            return Ok(BaseResponse<List<AppointmentSyncDto>>.Ok(appointments));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get appointments for sync error");
            return StatusCode(500, BaseResponse<object>.Error("Internal server error", 500));
        }
    }

/// <summary>
/// Обновить статус назначения
/// </summary>
[HttpPut("appointments/{appointmentId}")]
    [Authorize]
    [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
    [ProducesResponseType(typeof(BaseResponse<object>), 404)]
    public async Task<IActionResult> UpdateAppointmentStatus(Guid appointmentId, [FromBody] UpdateAppointmentRequest request)
    {
        try
        {
            var result = await _syncService.UpdateAppointmentStatusAsync(appointmentId, request.Status, request.CompletedBy);

            if (!result)
            {
                return NotFound(BaseResponse<object>.NotFound("Appointment not found"));
            }

            return Ok(BaseResponse<bool>.Ok(true, "Appointment updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Update appointment error");
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
}