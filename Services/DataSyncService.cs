using MedObhod.Backend.Data;
using MedObhod.Backend.DTOs;
using MedObhod.Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

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

    public async Task<SyncResponse> ProcessSyncAsync(SyncRequest request)
    {
        var response = new SyncResponse
        {
            Success = true,
            ServerTime = DateTime.UtcNow,
            IdMapping = new Dictionary<Guid, Guid>()
        };

        try
        {
            // Process each change from client
            foreach (var change in request.Changes)
            {
                try
                {
                    switch (change.EntityName.ToLower())
                    {
                        case "patient":
                            var patient = JsonSerializer.Deserialize<PatientSyncDto>(change.Data);
                            if (patient != null)
                            {
                                var serverId = await UpsertPatientAsync(patient, change.LocalId);
                                response.IdMapping[change.LocalId] = serverId;
                            }
                            break;

                        case "hospitalization":
                            var hospitalization = JsonSerializer.Deserialize<HospitalizationSyncDto>(change.Data);
                            if (hospitalization != null)
                            {
                                var serverId = await UpsertHospitalizationAsync(hospitalization);
                                response.IdMapping[change.LocalId] = serverId;
                            }
                            break;

                        case "vitalsign":
                            var vitalSign = JsonSerializer.Deserialize<VitalSignSyncDto>(change.Data);
                            if (vitalSign != null)
                            {
                                var serverId = await UpsertVitalSignAsync(vitalSign);
                                response.IdMapping[change.LocalId] = serverId;
                            }
                            break;

                        case "appointment":
                            var appointment = JsonSerializer.Deserialize<AppointmentSyncDto>(change.Data);
                            if (appointment != null)
                            {
                                var serverId = await UpsertAppointmentAsync(appointment);
                                response.IdMapping[change.LocalId] = serverId;
                            }
                            break;

                        case "doctornote":
                            var note = JsonSerializer.Deserialize<DoctorNoteSyncDto>(change.Data);
                            if (note != null)
                            {
                                var serverId = await UpsertDoctorNoteAsync(note);
                                response.IdMapping[change.LocalId] = serverId;
                            }
                            break;

                        case "delete":
                            await DeleteEntityAsync(change.EntityName, change.ServerId ?? change.LocalId);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing change for entity {Entity}", change.EntityName);
                    response.Success = false;
                    response.Message = $"Error processing {change.EntityName}";
                }
            }

            // Get changes from server that client doesn't have
            if (response.Success)
            {
                var serverChanges = await GetServerChangesAsync(request.LastSyncTime);
                response.ServerChanges = serverChanges;
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sync failed");
            response.Success = false;
            response.Message = "Sync failed: " + ex.Message;
        }

        return response;
    }

    private async Task<List<SyncChangeItem>> GetServerChangesAsync(DateTime? since)
    {
        var changes = new List<SyncChangeItem>();
        var syncTime = since ?? DateTime.MinValue;

        // Get changed patients
        var patients = await GetChangedPatientsAsync(syncTime);
        foreach (var patient in patients)
        {
            changes.Add(new SyncChangeItem
            {
                EntityName = "Patient",
                Operation = patient.Id.HasValue ? "UPDATE" : "INSERT",
                ServerId = patient.Id,
                Data = JsonSerializer.Serialize(patient),
                ChangedAt = patient.UpdatedDt ?? DateTime.UtcNow
            });
        }

        // Get changed hospitalizations
        var hospitalizations = await GetChangedHospitalizationsAsync(syncTime);
        foreach (var hospitalization in hospitalizations)
        {
            changes.Add(new SyncChangeItem
            {
                EntityName = "Hospitalization",
                Operation = hospitalization.Id.HasValue ? "UPDATE" : "INSERT",
                ServerId = hospitalization.Id,
                Data = JsonSerializer.Serialize(hospitalization),
                ChangedAt = hospitalization.UpdatedDt ?? DateTime.UtcNow
            });
        }

        // Get changed vital signs
        var vitals = await GetChangedVitalSignsAsync(syncTime);
        foreach (var vital in vitals)
        {
            changes.Add(new SyncChangeItem
            {
                EntityName = "VitalSign",
                Operation = vital.Id.HasValue ? "UPDATE" : "INSERT",
                ServerId = vital.Id,
                Data = JsonSerializer.Serialize(vital),
                ChangedAt = vital.MeasuredDt
            });
        }

        // Get changed appointments
        var appointments = await GetChangedAppointmentsAsync(syncTime);
        foreach (var appointment in appointments)
        {
            changes.Add(new SyncChangeItem
            {
                EntityName = "Appointment",
                Operation = appointment.Id.HasValue ? "UPDATE" : "INSERT",
                ServerId = appointment.Id,
                Data = JsonSerializer.Serialize(appointment),
                ChangedAt = appointment.UpdatedDt ?? DateTime.UtcNow
            });
        }

        return changes;
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

    public async Task<List<VitalSignSyncDto>> GetChangedVitalSignsAsync(DateTime? since)
    {
        var query = _context.VitalSigns
            .Where(v => !v.IsDeleted)
            .AsQueryable();

        if (since.HasValue)
        {
            query = query.Where(v => v.MeasuredDt > since.Value);
        }

        return await query
            .OrderByDescending(v => v.MeasuredDt)
            .Select(v => new VitalSignSyncDto
            {
                Id = v.VitalSignId,
                HospitalizationId = v.HospitalizationId,
                MeasuredDt = v.MeasuredDt,
                Temperature = v.Temperature,
                Pulse = v.Pulse,
                SpO2 = v.SpO2,
                RespiratoryRate = v.RespiratoryRate,
                InsUserId = v.InsUserId,
                Version = v.Version
            })
            .ToListAsync();
    }

    public async Task<List<AppointmentSyncDto>> GetChangedAppointmentsAsync(DateTime? since)
    {
        var query = _context.Appointments
            .Where(a => !a.IsDeleted)
            .AsQueryable();

        if (since.HasValue)
        {
            query = query.Where(a => a.UpdatedDt > since.Value || a.CreatedDt > since.Value);
        }

        return await query
            .Select(a => new AppointmentSyncDto
            {
                Id = a.AppointmentId,
                HospitalizationId = a.HospitalizationId,
                TemplateId = a.TemplateId,
                InsUserId = a.InsUserId,
                Type = a.Type,
                Name = a.Name,
                Priority = a.Priority,
                DurationMin = a.DurationMin,
                Instructions = a.Instructions,
                Notes = a.Notes,
                Status = a.Status,
                Version = a.Version,
                UpdatedDt = a.UpdatedDt
            })
            .ToListAsync();
    }

    public async Task<List<DoctorNoteSyncDto>> GetChangedDoctorNotesAsync(DateTime? since)
    {
        var query = _context.DoctorNotes
            .Where(n => !n.IsDeleted)
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
                Temperature = n.Temperature,
                Pulse = n.Pulse,
                RespiratoryRate = n.RespiratoryRate,
                ExaminationSummary = n.ExaminationSummary,
                TreatmentEffectiveness = n.TreatmentEffectiveness,
                PlanNote = n.PlanNote,
                Version = n.Version
            })
            .ToListAsync();
    }

    public async Task<Guid> UpsertPatientAsync(PatientSyncDto patient, Guid clientId)
    {
        var existing = await _context.Patients
            .FirstOrDefaultAsync(p => p.PatientId == patient.Id);

        if (existing == null)
        {
            // Create new
            var newPatient = new Patient
            {
                PatientId = patient.Id ?? Guid.NewGuid(),
                FullName = patient.FullName,
                BirthDt = patient.BirthDt,
                Gender = patient.Gender,
                CreatedDt = DateTime.UtcNow,
                UpdatedDt = DateTime.UtcNow,
                IsDeleted = false,
                Version = 1
            };
            _context.Patients.Add(newPatient);
            return newPatient.PatientId;
        }
        else
        {
            // Update if newer version
            if (patient.Version > existing.Version)
            {
                existing.FullName = patient.FullName;
                existing.BirthDt = patient.BirthDt;
                existing.Gender = patient.Gender;
                existing.UpdatedDt = DateTime.UtcNow;
                existing.Version++;
            }
            return existing.PatientId;
        }
    }

    public async Task<Guid> UpsertHospitalizationAsync(HospitalizationSyncDto hospitalization)
    {
        var existing = await _context.Hospitalizations
            .FirstOrDefaultAsync(h => h.HospitalizationId == hospitalization.Id);

        if (existing == null)
        {
            var newHospitalization = new Hospitalization
            {
                HospitalizationId = hospitalization.Id ?? Guid.NewGuid(),
                PatientId = hospitalization.PatientId,
                AdmissionDt = hospitalization.AdmissionDt,
                DischargeDt = hospitalization.DischargeDt,
                Room = hospitalization.Room,
                Bed = hospitalization.Bed,
                AttendingDoctorId = hospitalization.AttendingDoctorId,
                Status = hospitalization.Status,
                CreatedDt = DateTime.UtcNow,
                UpdatedDt = DateTime.UtcNow,
                IsDeleted = false,
                Version = 1
            };
            _context.Hospitalizations.Add(newHospitalization);
            return newHospitalization.HospitalizationId;
        }
        else
        {
            if (hospitalization.Version > existing.Version)
            {
                existing.DischargeDt = hospitalization.DischargeDt;
                existing.Room = hospitalization.Room;
                existing.Bed = hospitalization.Bed;
                existing.Status = hospitalization.Status;
                existing.UpdatedDt = DateTime.UtcNow;
                existing.Version++;
            }
            return existing.HospitalizationId;
        }
    }

    public async Task<Guid> UpsertVitalSignAsync(VitalSignSyncDto vitalSign)
    {
        var existing = await _context.VitalSigns
            .FirstOrDefaultAsync(v => v.VitalSignId == vitalSign.Id);

        if (existing == null)
        {
            var newVitalSign = new VitalSign
            {
                VitalSignId = vitalSign.Id ?? Guid.NewGuid(),
                HospitalizationId = vitalSign.HospitalizationId,
                MeasuredDt = vitalSign.MeasuredDt,
                Temperature = vitalSign.Temperature,
                Pulse = vitalSign.Pulse,
                SpO2 = vitalSign.SpO2,
                RespiratoryRate = vitalSign.RespiratoryRate,
                InsUserId = vitalSign.InsUserId,
                CreatedDt = DateTime.UtcNow,
                UpdatedDt = DateTime.UtcNow,
                IsDeleted = false,
                Version = 1
            };
            _context.VitalSigns.Add(newVitalSign);
            return newVitalSign.VitalSignId;
        }
        return existing.VitalSignId;
    }

    public async Task<Guid> UpsertAppointmentAsync(AppointmentSyncDto appointment)
    {
        var existing = await _context.Appointments
            .FirstOrDefaultAsync(a => a.AppointmentId == appointment.Id);

        if (existing == null)
        {
            var newAppointment = new Appointment
            {
                AppointmentId = appointment.Id ?? Guid.NewGuid(),
                HospitalizationId = appointment.HospitalizationId,
                TemplateId = appointment.TemplateId,
                InsUserId = appointment.InsUserId,
                Type = appointment.Type,
                Name = appointment.Name,
                Priority = appointment.Priority,
                DurationMin = appointment.DurationMin,
                Instructions = appointment.Instructions,
                Notes = appointment.Notes,
                Status = appointment.Status,
                CreatedDt = DateTime.UtcNow,
                UpdatedDt = DateTime.UtcNow,
                IsDeleted = false,
                Version = 1
            };
            _context.Appointments.Add(newAppointment);
            return newAppointment.AppointmentId;
        }
        else
        {
            if (appointment.Version > existing.Version)
            {
                existing.Status = appointment.Status;
                existing.UpdatedDt = DateTime.UtcNow;
                existing.Version++;
            }
            return existing.AppointmentId;
        }
    }

    public async Task<Guid> UpsertDoctorNoteAsync(DoctorNoteSyncDto note)
    {
        var existing = await _context.DoctorNotes
            .FirstOrDefaultAsync(n => n.DoctorNoteId == note.Id);

        if (existing == null)
        {
            var newNote = new DoctorNote
            {
                DoctorNoteId = note.Id ?? Guid.NewGuid(),
                HospitalizationId = note.HospitalizationId,
                DoctorId = note.DoctorId,
                Complaints = note.Complaints,
                GeneralCondition = note.GeneralCondition,
                MentalStatus = note.MentalStatus,
                Temperature = note.Temperature,
                Pulse = note.Pulse,
                RespiratoryRate = note.RespiratoryRate,
                ExaminationSummary = note.ExaminationSummary,
                TreatmentEffectiveness = note.TreatmentEffectiveness,
                PlanNote = note.PlanNote,
                CreatedDt = DateTime.UtcNow,
                UpdatedDt = DateTime.UtcNow,
                IsDeleted = false,
                Version = 1
            };
            _context.DoctorNotes.Add(newNote);
            return newNote.DoctorNoteId;
        }
        else
        {
            if (note.Version > existing.Version)
            {
                existing.Complaints = note.Complaints;
                existing.GeneralCondition = note.GeneralCondition;
                existing.ExaminationSummary = note.ExaminationSummary;
                existing.PlanNote = note.PlanNote;
                existing.UpdatedDt = DateTime.UtcNow;
                existing.Version++;
            }
            return existing.DoctorNoteId;
        }
    }

    public async Task<bool> DeleteEntityAsync(string entityName, Guid id)
    {
        switch (entityName.ToLower())
        {
            case "patient":
                var patient = await _context.Patients.FindAsync(id);
                if (patient != null)
                {
                    patient.IsDeleted = true;
                    patient.UpdatedDt = DateTime.UtcNow;
                    patient.Version++;
                    return true;
                }
                break;

            case "appointment":
                var appointment = await _context.Appointments.FindAsync(id);
                if (appointment != null)
                {
                    appointment.IsDeleted = true;
                    appointment.UpdatedDt = DateTime.UtcNow;
                    appointment.Version++;
                    return true;
                }
                break;
        }
        return false;
    }

    public async Task<List<UserSyncDto>> GetUsersForSyncAsync(DateTime? since)
    {
        var query = _context.Users.AsQueryable();

        if (since.HasValue)
        {
            query = query.Where(u => u.UpdatedDt > since.Value || u.CreatedDt > since.Value);
        }

        return await query
            .Select(u => new UserSyncDto
            {
                Id = u.UserId,
                Login = u.Login,
                FullName = u.FullName,
                Role = u.Role,
                Version = u.Version,
                UpdatedDt = u.UpdatedDt,
                IsDeleted = u.IsDeleted
            })
            .ToListAsync();
    }

    public async Task<List<VitalSignSyncDto>> GetChangedVitalSignsAsync(Guid? hospitalizationId, DateTime? since)
    {
        var query = _context.VitalSigns
            .Where(v => !v.IsDeleted)
            .AsQueryable();

        if (hospitalizationId.HasValue)
        {
            query = query.Where(v => v.HospitalizationId == hospitalizationId.Value);
        }

        if (since.HasValue)
        {
            query = query.Where(v => v.MeasuredDt > since.Value);
        }

        return await query
            .OrderByDescending(v => v.MeasuredDt)
            .Select(v => new VitalSignSyncDto
            {
                Id = v.VitalSignId,
                HospitalizationId = v.HospitalizationId,
                MeasuredDt = v.MeasuredDt,
                Temperature = v.Temperature,
                Pulse = v.Pulse,
                SystolicBP = v.SystolicBp,
                DiastolicBP = v.DiastolicBp,
                SpO2 = v.SpO2,
                RespiratoryRate = v.RespiratoryRate,
                NEWSScore = v.Newsscore,
                InsUserId = v.InsUserId,
                Version = v.Version
            })
            .ToListAsync();
    }

    public async Task<List<AppointmentSyncDto>> GetChangedAppointmentsAsync(Guid? hospitalizationId, DateTime? since)
    {
        var query = _context.Appointments
            .Where(a => !a.IsDeleted)
            .AsQueryable();

        if (hospitalizationId.HasValue)
        {
            query = query.Where(a => a.HospitalizationId == hospitalizationId.Value);
        }

        if (since.HasValue)
        {
            query = query.Where(a => a.UpdatedDt > since.Value || a.CreatedDt > since.Value);
        }

        return await query
            .Select(a => new AppointmentSyncDto
            {
                Id = a.AppointmentId,
                HospitalizationId = a.HospitalizationId,
                TemplateId = a.TemplateId,
                InsUserId = a.InsUserId,
                Type = a.Type,
                Name = a.Name,
                Priority = a.Priority,
                DurationMin = a.DurationMin,
                Instructions = a.Instructions,
                Notes = a.Notes,
                Status = a.Status,
                Version = a.Version,
                UpdatedDt = a.UpdatedDt
            })
            .ToListAsync();
    }

    public async Task<bool> UpdateAppointmentStatusAsync(Guid appointmentId, string status, Guid? completedBy)
    {
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && !a.IsDeleted);

        if (appointment == null) return false;

        appointment.Status = status;
        appointment.UpdatedDt = DateTime.UtcNow;
        appointment.Version++;

        if (status == "completed" && completedBy.HasValue)
        {
            // Добавляем запись о выполнении
            var execution = new AppointmentExecution
            {
                AppointmentExecutionId = Guid.NewGuid(),
                AppointmentId = appointmentId,
                ExecutedAt = DateTime.UtcNow,
                ExecutedUserId = completedBy.Value,
                Status = status,
                CreatedDt = DateTime.UtcNow,
                UpdatedDt = DateTime.UtcNow,
                IsDeleted = false,
                Version = 1
            };
            _context.AppointmentExecutions.Add(execution);
        }

        await _context.SaveChangesAsync();
        return true;
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

    public async Task<Guid> UpsertPatientDiagnosisAsync(PatientDiagnosisSyncDto dto)
    {
        var existing = await _context.PatientDiagnoses
            .FirstOrDefaultAsync(pd => pd.PatientDiagnoseId == dto.Id);

        if (existing == null)
        {
            var newPatientDiagnosis = new PatientDiagnosis
            {
                PatientDiagnoseId = dto.Id,
                HospitalizationId = dto.HospitalizationId,
                DiagnosisId = dto.DiagnosisId,
                IsPrimary = dto.IsPrimary,
                CreatedDt = DateTime.UtcNow,
                UpdatedDt = DateTime.UtcNow,
                IsDeleted = false,
                Version = 1
            };
            _context.PatientDiagnoses.Add(newPatientDiagnosis);
            return newPatientDiagnosis.PatientDiagnoseId;
        }
        else
        {
            if (dto.Version > existing.Version)
            {
                existing.IsPrimary = dto.IsPrimary;
                existing.UpdatedDt = DateTime.UtcNow;
                existing.Version++;
            }
            return existing.PatientDiagnoseId;
        }
    }
}