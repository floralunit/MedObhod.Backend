using MedObhod.Backend.DTOs;
using MedObhod.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

            var appointments = await _syncService.GetChangedAppointmentsAsync(lastSyncTime);
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
}