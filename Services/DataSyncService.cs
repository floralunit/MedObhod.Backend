using MedObhod.Backend.Data;
using MedObhod.Backend.DTOs;
using MedObhod.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace MedObhod.Backend.Services;

public class DataSyncService : IDataSyncService
{
    private readonly AppDbContext _context;
    private readonly ILogger<DataSyncService> _logger;

    public DataSyncService(AppDbContext context, ILogger<DataSyncService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<PatientSyncDto>> GetChangedPatientsAsync(DateTime? since)
    {
        var query = _context.Patients
            .Where(p => !p.IsDeleted)
            .AsQueryable();

        if (since.HasValue)
        {
            query = query.Where(p => p.UpdatedDt > since.Value || p.CreatedDt > since.Value);
        }

        return await query
            .OrderBy(p => p.UpdatedDt)
            .Select(p => new PatientSyncDto
            {
                Id = p.PatientId,
                FullName = p.FullName,
                BirthDt = p.BirthDt,
                Gender = p.Gender,
                Version = p.Version,
                UpdatedDt = p.UpdatedDt
            })
            .ToListAsync();
    }
    public async Task<List<PatientDiagnosisSyncDto>> GetChangedPatientDiagnosesAsync(DateTime? since)
    {
        var query = _context.PatientDiagnoses
            .Where(pd => !pd.IsDeleted)
            .AsQueryable();

        if (since.HasValue)
        {
            query = query.Where(pd => pd.UpdatedDt > since.Value || pd.CreatedDt > since.Value);
        }

        return await query
            .Select(pd => new PatientDiagnosisSyncDto
            {
                Id = pd.PatientDiagnoseId,
                HospitalizationId = pd.HospitalizationId,
                DiagnosisId = pd.DiagnosisId,
                IsPrimary = pd.IsPrimary,
                Version = pd.Version,
                UpdatedDt = pd.UpdatedDt
            })
            .ToListAsync();
    }

    /// <summary>
    /// Создать или обновить связь пациент-диагноз
    /// </summary>
    public async Task<Guid> UpsertPatientDiagnosisAsync(PatientDiagnosisSyncDto dto)
    {
        var existing = await _context.PatientDiagnoses
            .FirstOrDefaultAsync(pd => pd.PatientDiagnoseId == dto.Id);

        if (existing == null)
        {
            // Создаем новую связь
            var newPd = new PatientDiagnosis
            {
                PatientDiagnoseId = dto.Id != Guid.Empty ? dto.Id : Guid.NewGuid(),
                HospitalizationId = dto.HospitalizationId,
                DiagnosisId = dto.DiagnosisId,
                IsPrimary = dto.IsPrimary,
                CreatedDt = DateTime.Now,
                UpdatedDt = DateTime.Now,
                IsDeleted = false,
                Version = 1
            };

            // Если это первичный диагноз — снимаем флаг isPrimary с других
            if (dto.IsPrimary)
            {
                var otherDiagnoses = await _context.PatientDiagnoses
                    .Where(pd => pd.HospitalizationId == dto.HospitalizationId
                        && pd.PatientDiagnoseId != newPd.PatientDiagnoseId
                        && !pd.IsDeleted)
                    .ToListAsync();

                foreach (var other in otherDiagnoses)
                {
                    other.IsPrimary = false;
                    other.UpdatedDt = DateTime.Now;
                    other.Version++;
                }
            }

            _context.PatientDiagnoses.Add(newPd);
            await _context.SaveChangesAsync();
            return newPd.PatientDiagnoseId;
        }
        else
        {
            // Обновляем существующую
            existing.DiagnosisId = dto.DiagnosisId;
            existing.IsPrimary = dto.IsPrimary;
            existing.UpdatedDt = DateTime.Now;
            existing.Version++;

            // Если это первичный диагноз — снимаем флаг с других
            if (dto.IsPrimary)
            {
                var otherDiagnoses = await _context.PatientDiagnoses
                    .Where(pd => pd.HospitalizationId == dto.HospitalizationId
                        && pd.PatientDiagnoseId != dto.Id
                        && !pd.IsDeleted)
                    .ToListAsync();

                foreach (var other in otherDiagnoses)
                {
                    other.IsPrimary = false;
                    other.UpdatedDt = DateTime.Now;
                    other.Version++;
                }
            }

            await _context.SaveChangesAsync();
            return existing.PatientDiagnoseId;
        }
    }

    public async Task<List<HospitalizationSyncDto>> GetChangedHospitalizationsAsync(DateTime? since)
    {
        var query = _context.Hospitalizations
            .Where(h => !h.IsDeleted)
            .AsQueryable();

        if (since.HasValue)
        {
            query = query.Where(h => h.UpdatedDt > since.Value || h.CreatedDt > since.Value);
        }

        return await query
            .Select(h => new HospitalizationSyncDto
            {
                Id = h.HospitalizationId,
                PatientId = h.PatientId,
                AdmissionDt = h.AdmissionDt,
                DischargeDt = h.DischargeDt,
                Room = h.Room,
                Bed = h.Bed,
                AttendingDoctorId = h.AttendingDoctorId,
                Status = h.Status,
                Version = h.Version,
                UpdatedDt = h.UpdatedDt
            })
            .ToListAsync();
    }
    public async Task<bool> UpdateHospitalizationAsync(Guid hospitalizationId, UpdateHospitalizationRequest request)
    {
        var hosp = await _context.Hospitalizations
            .FirstOrDefaultAsync(h => h.HospitalizationId == hospitalizationId && !h.IsDeleted);

        if (hosp == null) return false;

        if (request.AttendingDoctorId.HasValue)
            hosp.AttendingDoctorId = request.AttendingDoctorId.Value;
        if (!string.IsNullOrEmpty(request.Status))
            hosp.Status = request.Status;
        if (!string.IsNullOrEmpty(request.Room))
            hosp.Room = request.Room;

        hosp.UpdatedDt = DateTime.Now;
        hosp.Version++;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<VitalSignSyncDto>> GetChangedVitalSignsAsync(DateTime? since)
    {
        var query = _context.VitalSigns
            .Where(v => !v.IsDeleted)
            .AsQueryable();

        if (since.HasValue)
        {
            query = query.Where(v => v.CreatedDt > since.Value);
        }

        return await query
            .OrderByDescending(v => v.CreatedDt)
            .Select(v => new VitalSignSyncDto
            {
                Id = v.VitalSignId,
                HospitalizationId = v.HospitalizationId,
                CreatedDt = v.CreatedDt,
                Temperature = v.Temperature,
                Pulse = v.Pulse,
                SpO2 = v.SpO2,
                RespiratoryRate = v.RespiratoryRate,
                InsUserId = v.InsUserId,
                Version = v.Version
            })
            .ToListAsync();
    }
    public async Task<Guid> CreateVitalSignAsync(CreateVitalSignRequest request, Guid userId)
    {
        var newsScore = CalculateNEWSScore(request);

        var vitalSign = new VitalSign
        {
            VitalSignId = Guid.NewGuid(),
            HospitalizationId = request.HospitalizationId,
            CreatedDt = DateTime.Now,
            Temperature = request.Temperature,
            Pulse = request.Pulse,
            SystolicBp = request.SystolicBP,
            DiastolicBp = request.DiastolicBP,
            SpO2 = request.SpO2,
            RespiratoryRate = request.RespiratoryRate,
            Newsscore = newsScore,
            InsUserId = userId,
            UpdatedDt = DateTime.Now,
            IsDeleted = false,
            Version = 1
        };

        _context.VitalSigns.Add(vitalSign);

        // Обновляем статус госпитализации
        var hospitalization = await _context.Hospitalizations
            .FirstOrDefaultAsync(h => h.HospitalizationId == request.HospitalizationId);

        if (hospitalization != null)
        {
            if (newsScore >= 7)
                hospitalization.Status = "critical";
            else if (newsScore >= 5)
                hospitalization.Status = "warning";
            else
                hospitalization.Status = "stable";

            hospitalization.UpdatedDt = DateTime.Now;
            hospitalization.Version++;
        }

        await _context.SaveChangesAsync();

        return vitalSign.VitalSignId;
    }

    private int CalculateNEWSScore(CreateVitalSignRequest vital)
    {
        int score = 0;

        if (vital.Temperature <= 35.0) score += 3;
        else if (vital.Temperature < 36.0) score += 1;
        else if (vital.Temperature <= 38.0) score += 0;
        else if (vital.Temperature <= 39.0) score += 1;
        else score += 2;

        if (vital.Pulse <= 40) score += 3;
        else if (vital.Pulse <= 50) score += 1;
        else if (vital.Pulse <= 90) score += 0;
        else if (vital.Pulse <= 110) score += 1;
        else if (vital.Pulse <= 130) score += 2;
        else score += 3;

        if (vital.SpO2 <= 91) score += 3;
        else if (vital.SpO2 <= 93) score += 2;
        else if (vital.SpO2 <= 95) score += 1;

        if (vital.RespiratoryRate <= 8) score += 3;
        else if (vital.RespiratoryRate <= 11) score += 1;
        else if (vital.RespiratoryRate <= 20) score += 0;
        else if (vital.RespiratoryRate <= 24) score += 2;
        else score += 3;

        if (vital.SystolicBP <= 90) score += 3;
        else if (vital.SystolicBP <= 100) score += 2;
        else if (vital.SystolicBP <= 110) score += 1;
        else if (vital.SystolicBP <= 219) score += 0;
        else score += 2;

        return score;
    }

    public async Task<List<VitalSignSyncDto>> GetChangedVitalSignsAsync(Guid? hospitalizationId, DateTime? since)
    {
        var query = _context.VitalSigns.AsQueryable();

        if (hospitalizationId.HasValue)
        {
            query = query.Where(v => v.HospitalizationId == hospitalizationId.Value);
        }

        if (since.HasValue)
        {
            query = query.Where(v => v.UpdatedDt > since.Value || v.CreatedDt > since.Value);
        }

        return await query
            .OrderByDescending(v => v.CreatedDt)
            .Select(v => new VitalSignSyncDto
            {
                Id = v.VitalSignId,
                HospitalizationId = v.HospitalizationId,
                Temperature = v.Temperature,
                Pulse = v.Pulse,
                SystolicBP = v.SystolicBp,
                DiastolicBP = v.DiastolicBp,
                SpO2 = v.SpO2,
                RespiratoryRate = v.RespiratoryRate,
                NEWSScore = v.Newsscore,
                InsUserId = v.InsUserId,
                Version = v.Version,
                CreatedDt = v.CreatedDt,
                UpdatedDt = v.UpdatedDt,
                IsDeleted = v.IsDeleted
            })
            .ToListAsync();
    }

    public async Task<List<DoctorNoteSyncDto>> GetChangedDoctorNotesAsync(DateTime? since)
    {
        var query = _context.DoctorNotes
            //.Where(n => !n.IsDeleted)
            .AsQueryable();

        if (since.HasValue)
        {
            query = query.Where(n => n.UpdatedDt > since.Value || n.CreatedDt > since.Value);
        }

        return await query
            .Select(n => new DoctorNoteSyncDto
            {
                Id = n.DoctorNoteId,
                HospitalizationId = n.HospitalizationId,
                DoctorId = n.DoctorId,
                Complaints = n.Complaints,
                GeneralCondition = n.GeneralCondition,
                MentalStatus = n.MentalStatus,
                ExaminationSummary = n.ExaminationSummary,
                TreatmentEffectiveness = n.TreatmentEffectiveness,
                PlanNote = n.PlanNote,
                Version = n.Version,
                UpdatedDt = n.UpdatedDt,
                IsDeleted = n.IsDeleted
            })
            .ToListAsync();
    }

    public async Task<Guid> CreateDoctorNoteAsync(CreateDoctorNoteRequest request, Guid doctorId)
    {
        var noteId = Guid.NewGuid();

        var doctorNote = new DoctorNote
        {
            DoctorNoteId = noteId,
            HospitalizationId = request.HospitalizationId,
            DoctorId = doctorId,
            Complaints = request.Complaints,
            ExaminationSummary = request.ExaminationSummary,
            TreatmentEffectiveness = request.TreatmentEffectiveness,
            //PlanNote = request.PlanNote,
            Notes = request.Notes,
            CreatedDt = DateTime.Now,
            UpdatedDt = DateTime.Now,
            IsDeleted = false,
            Version = 1
        };

        _context.DoctorNotes.Add(doctorNote);
        await _context.SaveChangesAsync();

        return noteId;
    }

    public async Task<Guid> CompleteRoundAsync(
      CompleteDoctorRoundRequest request)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var round = new DoctorRound
            {
                DoctorRoundId = Guid.NewGuid(),
                DoctorId = request.DoctorId,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Status = "completed",
                CreatedDt = DateTime.UtcNow,
                UpdatedDt = DateTime.UtcNow,
                IsDeleted = false,
                Version = 1
            };

            _context.DoctorRounds.Add(round);

            foreach (var item in request.Items)
            {
                var roundItem = new DoctorRoundItem
                {
                    DoctorRoundItemId = Guid.NewGuid(),
                    RoundId = round.DoctorRoundId,
                    HospitalizationId =
                        item.HospitalizationId,

                    OrderIndex = item.OrderIndex,
                    PlannedTimeDt =
                        item.PlannedTime,

                    StartVisitTime =
                        item.StartVisitTime,

                    EndVisitTime =
                        item.EndVisitTime,

                    Status = item.Status,

                    CreatedDt = DateTime.UtcNow,
                    UpdatedDt = DateTime.UtcNow,
                    IsDeleted = false,
                    Version = 1
                };

                _context.DoctorRoundItems.Add(roundItem);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return round.DoctorRoundId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<AppointmentSyncFullDto>> GetAppointmentsWithExecutionsAsync(Guid? hospitalizationId, DateTime? since)
    {
        var query = _context.Appointments
            .Where(a => !a.IsDeleted)
            .AsQueryable();

        if (hospitalizationId.HasValue)
            query = query.Where(a => a.HospitalizationId == hospitalizationId.Value);

        if (since.HasValue)
            query = query.Where(a => a.UpdatedDt > since.Value || a.CreatedDt > since.Value);

        var appointments = await query
            .Include(a => a.AppointmentSchedules)
            .ToListAsync();

        var result = new List<AppointmentSyncFullDto>();

        foreach (var apt in appointments)
        {
            // Получаем расписание
            var schedule = apt.AppointmentSchedules.FirstOrDefault(s => !s.IsDeleted);
            var scheduleDto = schedule != null ? new AppointmentScheduleSyncDto
            {
                Id = schedule.AppointmentScheduleId,
                Frequency = schedule.Frequency,
                StartDt = schedule.StartDt.Value,
                EndDt = schedule.EndDt,
                StartTime = schedule.StartTime?.ToTimeSpan(),
                RelationToMeal = schedule.RelationToMeal,
                Times = await _context.AppointmentTimes
                    .Where(t => t.ScheduleId == schedule.AppointmentScheduleId && !t.IsDeleted)
                    .Select(t => t.TimeValue.ToString())
                    .ToListAsync()
            } : null;

            // Получаем executions
            var executions = await _context.AppointmentExecutions
                .Where(e => e.AppointmentId == apt.AppointmentId && !e.IsDeleted)
                .Select(e => new AppointmentExecutionSyncDto
                {
                    Id = e.AppointmentExecutionId,
                    AppointmentId = e.AppointmentId,
                    ScheduledDateTime = e.ScheduledDatetime.Value,
                    ExecutedAt = e.ExecutedAt,
                    ExecutedUserId = e.ExecutedUserId,
                    Status = e.Status,
                    Notes = e.Notes,
                    CreatedDt = e.CreatedDt,
                    UpdatedDt = e.UpdatedDt.Value,
                    IsDeleted = e.IsDeleted,
                    Version = e.Version
                })
                .ToListAsync();

            result.Add(new AppointmentSyncFullDto
            {
                Appointment = new AppointmentSyncDto
                {
                    Id = apt.AppointmentId,
                    HospitalizationId = apt.HospitalizationId,
                    TemplateId = apt.TemplateId,
                    InsUserId = apt.InsUserId,
                    Type = apt.Type,
                    Name = apt.Name,
                    Priority = apt.Priority,
                    DurationMin = apt.DurationMin,
                    Instructions = apt.Instructions,
                    Notes = apt.Notes,
                    Status = apt.Status,
                    Schedule = scheduleDto,
                    Version = apt.Version,
                    CreatedDt = apt.CreatedDt,
                    UpdatedDt = apt.UpdatedDt,
                    IsDeleted = apt.IsDeleted
                },
                Executions = executions
            });
        }

        return result;
    }

    public async Task<Guid> CreateAppointmentWithExecutionsAsync(CreateAppointmentRequest request, Guid userId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var appointmentId = Guid.NewGuid();
            var now = DateTime.Now;

            var appointment = new Appointment
            {
                AppointmentId = appointmentId,
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
                CreatedDt = now,
                UpdatedDt = now,
                IsDeleted = false,
                Version = 1
            };
            _context.Appointments.Add(appointment);

            if (request.Schedule != null)
            {
                var schedule = new AppointmentSchedule
                {
                    AppointmentScheduleId = Guid.NewGuid(),
                    AppointmentId = appointmentId,
                    Frequency = request.Schedule.Frequency,
                    StartDt = DateTime.Parse(request.Schedule.StartDate),
                    EndDt = string.IsNullOrEmpty(request.Schedule.EndDate) ? null : DateTime.Parse(request.Schedule.EndDate),
                    StartTime = TimeOnly.Parse(request.Schedule.StartTime),
                    RelationToMeal = request.Schedule.RelationToMeal,
                    CreatedDt = now,
                    UpdatedDt = now,
                    IsDeleted = false,
                    Version = 1
                };
                _context.AppointmentSchedules.Add(schedule);

                var times = request.Schedule.Times ?? new List<string> { request.Schedule.StartTime };
                var startDate = DateTime.Parse(request.Schedule.StartDate).Date;

                // Если endDate не указан, берем startDate + 1 день
                // Если endDate указан, используем его
                var endDate = string.IsNullOrEmpty(request.Schedule.EndDate)
                    ? startDate.AddDays(1)
                    : DateTime.Parse(request.Schedule.EndDate).Date;

                // Начинаем с startDate (дата из карточки)
                var currentDate = startDate;

                while (currentDate <= endDate)
                {
                    foreach (var time in times)
                    {
                        var parts = time.Split(':');
                        var scheduledDateTime = currentDate.AddHours(int.Parse(parts[0])).AddMinutes(int.Parse(parts[1]));

                        // Создаем execution ТОЛЬКО если время в будущем
                        // ИЛИ если это сегодня и время еще не прошло
                        if (scheduledDateTime > now)
                        {
                            var execution = new AppointmentExecution
                            {
                                AppointmentExecutionId = Guid.NewGuid(),
                                AppointmentId = appointmentId,
                                ScheduledDatetime = scheduledDateTime,
                                Status = "pending",
                                CreatedDt = now,
                                UpdatedDt = now,
                                IsDeleted = false,
                                Version = 1
                            };
                            _context.AppointmentExecutions.Add(execution);
                        }
                    }
                    currentDate = currentDate.AddDays(1);
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return appointmentId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> UpdateExecutionStatusAsync(Guid executionId, string status, Guid userId)
    {
        var execution = await _context.AppointmentExecutions
            .FirstOrDefaultAsync(e => e.AppointmentExecutionId == executionId && !e.IsDeleted);

        if (execution == null) return false;

        execution.Status = status;
        execution.ExecutedAt = DateTime.Now;
        execution.ExecutedUserId = userId;
        execution.UpdatedDt = DateTime.Now;
        execution.Version++;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateAppointmentAsync(Guid appointmentId, UpdateAppointmentRequest request)
    {
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && !a.IsDeleted);

        if (appointment == null) return false;

        if (!string.IsNullOrEmpty(request.Status))
        {
            appointment.Status = request.Status;

            // Если завершаем — завершаем все pending executions
            if (request.Status == "completed")
            {
                var pendingExecutions = await _context.AppointmentExecutions
                    .Where(e => e.AppointmentId == appointmentId && e.Status == "pending" && !e.IsDeleted)
                    .ToListAsync();

                foreach (var exec in pendingExecutions)
                {
                    exec.Status = "cancelled";
                    exec.UpdatedDt = DateTime.Now;
                    exec.Version++;
                }
            }
        }

        appointment.UpdatedDt = DateTime.Now;
        appointment.Version++;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<DepartmentAnalyticsDto> GetDepartmentAnalyticsAsync()
    {
        var now = DateTime.Now;
        var today = now.Date;

        var analytics = new DepartmentAnalyticsDto();

        // Пациенты
        var activeHospitalizations = await _context.Hospitalizations
            .Where(h => !h.IsDeleted && h.DischargeDt == null)
            .ToListAsync();

        analytics.TotalPatients = activeHospitalizations.Count;
        analytics.CriticalPatients = activeHospitalizations.Count(h => h.Status == "critical");
        analytics.WarningPatients = activeHospitalizations.Count(h => h.Status == "warning");
        analytics.StablePatients = activeHospitalizations.Count(h => h.Status == "stable");

        // Назначения
        analytics.PendingAppointments = await _context.Appointments
            .CountAsync(a => !a.IsDeleted && a.Status == "pending");

        analytics.CompletedToday = await _context.AppointmentExecutions
            .CountAsync(e => !e.IsDeleted && e.Status == "completed" && e.ExecutedAt >= today);

        // Активные обходы
        analytics.ActiveRounds = await _context.DoctorRounds
            .CountAsync(r => !r.IsDeleted && r.Status == "in_progress" && r.StartTime >= today);

        // Статистика по врачам
        var doctors = await _context.Users
            .Where(u => u.Role == "doctor" && !u.IsDeleted)
            .ToListAsync();

        foreach (var doc in doctors)
        {
            var docPatients = await _context.Hospitalizations
                .CountAsync(h => h.AttendingDoctorId == doc.UserId && !h.IsDeleted && h.DischargeDt == null);

            var docCritical = await _context.Hospitalizations
                .CountAsync(h => h.AttendingDoctorId == doc.UserId && h.Status == "critical" && !h.IsDeleted && h.DischargeDt == null);

            var docAppointments = await _context.Appointments
                .CountAsync(a => a.InsUserId == doc.UserId && !a.IsDeleted && a.Status == "pending");

            var docRounds = await _context.DoctorRounds
                .CountAsync(r => r.DoctorId == doc.UserId && !r.IsDeleted);

            analytics.Doctors.Add(new DoctorAnalyticsDto
            {
                DoctorId = doc.UserId,
                DoctorName = doc.FullName,
                PatientsCount = docPatients,
                CriticalCount = docCritical,
                AppointmentsCount = docAppointments,
                RoundsCompleted = docRounds
            });
        }

        return analytics;
    }
}