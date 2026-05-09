namespace MedObhod.Backend.DTOs
{
    public class AppointmentScheduleDto
    {
        public string Frequency { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string? EndDate { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string? RelationToMeal { get; set; }
        public List<string> Times { get; set; } = new();
    }

    public class AppointmentMedicationDto
    {
        public Guid? Id { get; set; }
        public string? CustomName { get; set; }
        public string? Dosage { get; set; }
        public string? Form { get; set; }
    }
}
