// DTOs/RoundDtos.cs
using System;

namespace MedObhod.Backend.DTOs;

public class StartRoundRequest
{
    public DateTime StartTime { get; set; }
    public string RoundType { get; set; } = "morning";
    public List<RoundPatientDto> Patients { get; set; } = new();
}

public class RoundPatientDto
{
    public Guid PatientId { get; set; }
    public Guid HospitalizationId { get; set; }
    public int OrderIndex { get; set; }
    public DateTime PlannedTime { get; set; }
}

public class CompleteRoundItemRequest
{
    public DateTime ActualTime { get; set; }
    public int? NewsScoreBefore { get; set; }
    public int? NewsScoreAfter { get; set; }
    public string? Complaints { get; set; }
    public string? Examination { get; set; }
    public string? TreatmentChanges { get; set; }
    public string? Notes { get; set; }
}

public class RoundResponseDto
{
    public Guid Id { get; set; }
    public DateTime? StartTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public int TotalPatients { get; set; }
    public int CompletedPatients { get; set; }
    public List<RoundItemResponseDto> Patients { get; set; } = new();
}

public class RoundItemResponseDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Room { get; set; } = string.Empty;
    public int? OrderIndex { get; set; }
    public DateTime? PlannedTime { get; set; }
    public DateTime? ActualTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? NewsScore { get; set; }
}

public class RoundHistoryDto
{
    public Guid Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public int TotalPatients { get; set; }
    public int CompletedPatients { get; set; }
    public TimeSpan? Duration { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RoundStatisticsDto
{
    public int TotalRounds { get; set; }
    public int CompletedRounds { get; set; }
    public int TotalPatientsExamined { get; set; }
    public double AveragePatientsPerRound { get; set; }
    public double AverageRoundDuration { get; set; } // в минутах
    public Dictionary<string, int> RoundsByType { get; set; } = new();
    public List<DailyRoundStats> DailyStats { get; set; } = new();
}

public class DailyRoundStats
{
    public DateTime Date { get; set; }
    public int RoundsCount { get; set; }
    public int PatientsExamined { get; set; }
}

public class DoctorEfficiencyDto
{
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public int TotalRounds { get; set; }
    public double AveragePatientsPerRound { get; set; }
    public double AverageRoundDuration { get; set; }
    public int CriticalPatientsVisited { get; set; }
    public int TotalPatients { get; set; }
    public double CompletionRate { get; set; } // процент выполнения
}

public class RoundReportDto
{
    public Guid RoundId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public List<RoundReportItemDto> Patients { get; set; } = new();
    public RoundReportSummaryDto Summary { get; set; } = new();
}

public class RoundReportItemDto
{
    public string PatientName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Room { get; set; } = string.Empty;
    public DateTime? PlannedTime { get; set; }
    public DateTime? ActualTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? NewsScoreBefore { get; set; }
    public int? NewsScoreAfter { get; set; }
    public string? Complaints { get; set; }
    public string? Examination { get; set; }
    public string? TreatmentChanges { get; set; }
    public string? Notes { get; set; }
}

public class RoundReportSummaryDto
{
    public int TotalPatients { get; set; }
    public int CompletedPatients { get; set; }
    public int PendingPatients { get; set; }
    public TimeSpan? TotalDuration { get; set; }
    public double AverageTimePerPatient { get; set; }
}

public class DepartmentRoundDto
{
    public Guid Id { get; set; }
    public Guid? DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public int TotalPatients { get; set; }
    public int CompletedPatients { get; set; }
}

public class DepartmentSummaryDto
{
    public int TotalRounds { get; set; }
    public int ActiveRounds { get; set; }
    public int CompletedRounds { get; set; }
    public int TotalPatientsExamined { get; set; }
    public List<DoctorRoundSummaryDto> DoctorsSummary { get; set; } = new();
}

public class DoctorRoundSummaryDto
{
    public Guid? DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public int RoundsCount { get; set; }
    public int PatientsExamined { get; set; }
    public double AveragePatientsPerRound { get; set; }
}