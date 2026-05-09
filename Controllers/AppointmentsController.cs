using MedObhod.Backend.Data;
using MedObhod.Backend.DTOs;
using MedObhod.Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace MedObhod.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<AppointmentsController> _logger;

    public AppointmentsController(AppDbContext context, ILogger<AppointmentsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Получить все назначения пациента
    /// </summary>
    [HttpGet("patient/{hospitalizationId}")]
    [ProducesResponseType(typeof(BaseResponse<List<AppointmentResponseDto>>), 200)]
    public async Task<IActionResult> GetPatientAppointments(Guid hospitalizationId)
    {
        try
        {
            var appointments = await _context.Appointments
                .Where(a => a.HospitalizationId == hospitalizationId && !a.IsDeleted)
                .OrderByDescending(a => a.CreatedDt)
                .Select(a => new AppointmentResponseDto
                {
                    Id = a.AppointmentId,
                    HospitalizationId = a.HospitalizationId,
                    TemplateId = a.TemplateId,
                    Type = a.Type,
                    Name = a.Name,
                    Priority = a.Priority,
                    DurationMin = a.DurationMin,
                    Instructions = a.Instructions,
                    Notes = a.Notes,
                    Status = a.Status,
                    CreatedAt = a.CreatedDt,
                    UpdatedAt = a.UpdatedDt
                })
                .ToListAsync();

            return Ok(BaseResponse<List<AppointmentResponseDto>>.Ok(appointments));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get patient appointments error");
            return StatusCode(500, BaseResponse<object>.Error("Internal server error", 500));
        }
    }

    /// <summary>
    /// Получить одно назначение по ID
    /// </summary>
    [HttpGet("{appointmentId}")]
    [ProducesResponseType(typeof(BaseResponse<AppointmentResponseDto>), 200)]
    [ProducesResponseType(typeof(BaseResponse<object>), 404)]
    public async Task<IActionResult> GetAppointmentById(Guid appointmentId)
    {
        try
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && !a.IsDeleted);

            if (appointment == null)
                return NotFound(BaseResponse<object>.NotFound("Appointment not found"));

            var result = new AppointmentResponseDto
            {
                Id = appointment.AppointmentId,
                HospitalizationId = appointment.HospitalizationId,
                TemplateId = appointment.TemplateId,
                Type = appointment.Type,
                Name = appointment.Name,
                Priority = appointment.Priority,
                DurationMin = appointment.DurationMin,
                Instructions = appointment.Instructions,
                Notes = appointment.Notes,
                Status = appointment.Status,
                CreatedAt = appointment.CreatedDt,
                UpdatedAt = appointment.UpdatedDt
            };

            return Ok(BaseResponse<AppointmentResponseDto>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get appointment by id error");
            return StatusCode(500, BaseResponse<object>.Error("Internal server error", 500));
        }
    }

    /// <summary>
    /// Создать новое назначение
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BaseResponse<AppointmentResponseDto>), 201)]
    [ProducesResponseType(typeof(BaseResponse<object>), 400)]
    public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(BaseResponse<object>.Error("Invalid model state", 400));

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var appointment = new Appointment
            {
                AppointmentId = Guid.NewGuid(),
                HospitalizationId = request.HospitalizationId,
                TemplateId = request.TemplateId,
                InsUserId = userId,
                Type = request.Type,
                Name = request.Name,
                Priority = request.Priority,
                DurationMin = request.DurationMin,
                Instructions = request.Instructions,
                Notes = request.Notes,
                Status = "pending",
                CreatedDt = DateTime.UtcNow,
                UpdatedDt = DateTime.UtcNow,
                IsDeleted = false,
                Version = 1
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // Создаем расписание если есть
            if (request.Schedule != null)
            {
                var schedule = new AppointmentSchedule
                {
                    AppointmentScheduleId = Guid.NewGuid(),
                    AppointmentId = appointment.AppointmentId,
                    Frequency = request.Schedule.Frequency,
                    StartDt = DateTime.Parse(request.Schedule.StartDate),
                    EndDt = string.IsNullOrEmpty(request.Schedule.EndDate) ? null : DateTime.Parse(request.Schedule.EndDate),
                    StartTime = TimeOnly.Parse(request.Schedule.StartTime),
                    RelationToMeal = request.Schedule.RelationToMeal,
                    CreatedDt = DateTime.UtcNow,
                    UpdatedDt = DateTime.UtcNow,
                    IsDeleted = false,
                    Version = 1
                };
                _context.AppointmentSchedules.Add(schedule);
                await _context.SaveChangesAsync();

                // Добавляем временные слоты
                if (request.Schedule.Times != null && request.Schedule.Times.Any())
                {
                    foreach (var time in request.Schedule.Times)
                    {
                        var appointmentTime = new AppointmentTime
                        {
                            AppointmentTimeId = Guid.NewGuid(),
                            ScheduleId = schedule.AppointmentScheduleId,
                            TimeValue = TimeOnly.Parse(time),
                            CreatedDt = DateTime.UtcNow,
                            UpdatedDt = DateTime.UtcNow,
                            IsDeleted = false,
                            Version = 1
                        };
                        _context.AppointmentTimes.Add(appointmentTime);
                    }
                    await _context.SaveChangesAsync();
                }
            }

            // Сохраняем лекарство если есть
            if (request.Medication != null)
            {
                var appointmentMedication = new AppointmentMedication
                {
                    AppointmentMedicationId = Guid.NewGuid(),
                    AppointmentId = appointment.AppointmentId,
                    MedicationId = request.Medication.Id,
                    CustomName = request.Medication.CustomName,
                    Dosage = request.Medication.Dosage,
                    Form = request.Medication.Form,
                    CreatedDt = DateTime.UtcNow,
                    UpdatedDt = DateTime.UtcNow,
                    IsDeleted = false,
                    Version = 1
                };
                _context.AppointmentMedications.Add(appointmentMedication);
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("Appointment created: {Id} for patient {HospitalizationId}", appointment.AppointmentId, request.HospitalizationId);

            return CreatedAtAction(nameof(GetAppointmentById), new { appointmentId = appointment.AppointmentId },
                BaseResponse<object>.Ok(new { id = appointment.AppointmentId }, "Appointment created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Create appointment error");
            return StatusCode(500, BaseResponse<object>.Error("Internal server error", 500));
        }
    }

    /// <summary>
    /// Отметить назначение как выполненное
    /// </summary>
    [HttpPut("{appointmentId}/complete")]
    [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
    [ProducesResponseType(typeof(BaseResponse<object>), 404)]
    public async Task<IActionResult> CompleteAppointment(Guid appointmentId, [FromBody] Guid completedBy)
    {
        try
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && !a.IsDeleted);

            if (appointment == null)
                return NotFound(BaseResponse<object>.NotFound("Appointment not found"));

            appointment.Status = "completed";
            appointment.UpdatedDt = DateTime.UtcNow;
            appointment.Version++;

            // Добавляем запись о выполнении
            var execution = new AppointmentExecution
            {
                AppointmentExecutionId = Guid.NewGuid(),
                AppointmentId = appointmentId,
                ExecutedAt = DateTime.UtcNow,
                ExecutedUserId = completedBy,
                Status = "completed",
                CreatedDt = DateTime.UtcNow,
                UpdatedDt = DateTime.UtcNow,
                IsDeleted = false,
                Version = 1
            };
            _context.AppointmentExecutions.Add(execution);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Appointment completed: {Id}", appointmentId);

            return Ok(BaseResponse<bool>.Ok(true, "Appointment completed successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Complete appointment error");
            return StatusCode(500, BaseResponse<object>.Error("Internal server error", 500));
        }
    }

    /// <summary>
    /// Отменить назначение
    /// </summary>
    [HttpPut("{appointmentId}/cancel")]
    [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
    [ProducesResponseType(typeof(BaseResponse<object>), 404)]
    public async Task<IActionResult> CancelAppointment(Guid appointmentId)
    {
        try
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && !a.IsDeleted);

            if (appointment == null)
                return NotFound(BaseResponse<object>.NotFound("Appointment not found"));

            appointment.Status = "cancelled";
            appointment.UpdatedDt = DateTime.UtcNow;
            appointment.Version++;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Appointment cancelled: {Id}", appointmentId);

            return Ok(BaseResponse<bool>.Ok(true, "Appointment cancelled successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cancel appointment error");
            return StatusCode(500, BaseResponse<object>.Error("Internal server error", 500));
        }
    }

    /// <summary>
    /// Удалить назначение
    /// </summary>
    [HttpDelete("{appointmentId}")]
    [Authorize(Roles = "head,doctor")]
    [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
    [ProducesResponseType(typeof(BaseResponse<object>), 404)]
    public async Task<IActionResult> DeleteAppointment(Guid appointmentId)
    {
        try
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && !a.IsDeleted);

            if (appointment == null)
                return NotFound(BaseResponse<object>.NotFound("Appointment not found"));

            appointment.IsDeleted = true;
            appointment.UpdatedDt = DateTime.UtcNow;
            appointment.Version++;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Appointment deleted: {Id}", appointmentId);

            return Ok(BaseResponse<bool>.Ok(true, "Appointment deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete appointment error");
            return StatusCode(500, BaseResponse<object>.Error("Internal server error", 500));
        }
    }

    /// <summary>
    /// Получить активные назначения для медсестры
    /// </summary>
    [HttpGet("nurse/today")]
    [Authorize(Roles = "nurse")]
    [ProducesResponseType(typeof(BaseResponse<List<AppointmentResponseDto>>), 200)]
    public async Task<IActionResult> GetNurseTodaysAppointments()
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var appointments = await _context.Appointments
                .Where(a => !a.IsDeleted && a.Status == "pending")
                .Join(_context.Hospitalizations,
                    a => a.HospitalizationId,
                    h => h.HospitalizationId,
                    (a, h) => new { Appointment = a, Hospitalization = h })
                .Where(x => x.Hospitalization.AttendingDoctorId != null)
                .Select(x => new AppointmentResponseDto
                {
                    Id = x.Appointment.AppointmentId,
                    HospitalizationId = x.Appointment.HospitalizationId,
                    TemplateId = x.Appointment.TemplateId,
                    Type = x.Appointment.Type,
                    Name = x.Appointment.Name,
                    Priority = x.Appointment.Priority,
                    DurationMin = x.Appointment.DurationMin,
                    Instructions = x.Appointment.Instructions,
                    Notes = x.Appointment.Notes,
                    Status = x.Appointment.Status,
                    CreatedAt = x.Appointment.CreatedDt,
                    UpdatedAt = x.Appointment.UpdatedDt
                })
                .Take(20)
                .ToListAsync();

            return Ok(BaseResponse<List<AppointmentResponseDto>>.Ok(appointments));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get nurse appointments error");
            return StatusCode(500, BaseResponse<object>.Error("Internal server error", 500));
        }
    }
}