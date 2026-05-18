using System;
using System.Collections.Generic;
using MedObhod.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace MedObhod.Backend.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<AppointmentExecution> AppointmentExecutions { get; set; }

    public virtual DbSet<AppointmentSchedule> AppointmentSchedules { get; set; }

    public virtual DbSet<AppointmentTemplate> AppointmentTemplates { get; set; }

    public virtual DbSet<AppointmentTime> AppointmentTimes { get; set; }

    public virtual DbSet<Diagnosis> Diagnoses { get; set; }

    public virtual DbSet<DoctorNote> DoctorNotes { get; set; }

    public virtual DbSet<DoctorRound> DoctorRounds { get; set; }

    public virtual DbSet<DoctorRoundItem> DoctorRoundItems { get; set; }

    public virtual DbSet<Hospitalization> Hospitalizations { get; set; }

    public virtual DbSet<Medication> Medications { get; set; }

    public virtual DbSet<Patient> Patients { get; set; }

    public virtual DbSet<PatientDiagnosis> PatientDiagnoses { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserSession> UserSessions { get; set; }

    public virtual DbSet<VitalSign> VitalSigns { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-EA92TFG\\FLORALSERVER;Database=MedObhodDB;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.AppointmentId).HasName("PK__Appointm__FD01B50351FF13E7");

            entity.Property(e => e.AppointmentId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedDt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(d => d.Hospitalization).WithMany(p => p.Appointments).HasConstraintName("FK__Appointme__Versi__778AC167");

            entity.HasOne(d => d.InsUser).WithMany(p => p.Appointments).HasConstraintName("FK__Appointme__InsUs__797309D9");

            entity.HasOne(d => d.Template).WithMany(p => p.Appointments).HasConstraintName("FK__Appointme__Templ__787EE5A0");
        });

        modelBuilder.Entity<AppointmentExecution>(entity =>
        {
            entity.HasKey(e => e.AppointmentExecutionId).HasName("PK__Appointm__5F05EC99BABD3EC1");

            entity.Property(e => e.AppointmentExecutionId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedDt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(d => d.Appointment).WithMany(p => p.AppointmentExecutions).HasConstraintName("FK__Appointme__Versi__114A936A");

            entity.HasOne(d => d.ExecutedUser).WithMany(p => p.AppointmentExecutions).HasConstraintName("FK__Appointme__Execu__123EB7A3");
        });

        modelBuilder.Entity<AppointmentSchedule>(entity =>
        {
            entity.HasKey(e => e.AppointmentScheduleId).HasName("PK__Appointm__F48AD1F41D708DDB");

            entity.Property(e => e.AppointmentScheduleId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedDt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(d => d.Appointment).WithMany(p => p.AppointmentSchedules).HasConstraintName("FK__Appointme__Versi__05D8E0BE");
        });

        modelBuilder.Entity<AppointmentTemplate>(entity =>
        {
            entity.HasKey(e => e.AppointmentTemplateId).HasName("PK__Appointm__F0820D3AB12A61D9");

            entity.Property(e => e.CreatedDt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });

        modelBuilder.Entity<AppointmentTime>(entity =>
        {
            entity.HasKey(e => e.AppointmentTimeId).HasName("PK__Appointm__C1678A6D29A63A92");

            entity.Property(e => e.AppointmentTimeId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedDt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(d => d.Schedule).WithMany(p => p.AppointmentTimes).HasConstraintName("FK__Appointme__Versi__0B91BA14");
        });

        modelBuilder.Entity<Diagnosis>(entity =>
        {
            entity.HasKey(e => e.DiagnoseId).HasName("PK__Diagnose__E7B4E19565D041F7");

            entity.Property(e => e.DiagnoseId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedDt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });

        modelBuilder.Entity<DoctorNote>(entity =>
        {
            entity.HasKey(e => e.DoctorNoteId).HasName("PK__DoctorNo__9E56EB65751EAC66");

            entity.Property(e => e.DoctorNoteId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedDt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(d => d.Doctor).WithMany(p => p.DoctorNotes).HasConstraintName("FK__DoctorNot__Docto__25518C17");

            entity.HasOne(d => d.Hospitalization).WithMany(p => p.DoctorNotes).HasConstraintName("FK__DoctorNot__Hospi__245D67DE");

            entity.HasOne(d => d.RoundItem).WithMany(p => p.DoctorNotes).HasConstraintName("FK__DoctorNot__Round__2645B050");
        });

        modelBuilder.Entity<DoctorRound>(entity =>
        {
            entity.HasKey(e => e.DoctorRoundId).HasName("PK__DoctorRo__C7376DB5BBDA492D");

            entity.Property(e => e.DoctorRoundId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedDt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(d => d.Doctor).WithMany(p => p.DoctorRounds).HasConstraintName("FK__DoctorRou__Versi__17F790F9");
        });

        modelBuilder.Entity<DoctorRoundItem>(entity =>
        {
            entity.HasKey(e => e.DoctorRoundItemId).HasName("PK__DoctorRo__978DD3AE12A86FCD");

            entity.Property(e => e.DoctorRoundItemId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedDt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(d => d.Hospitalization).WithMany(p => p.DoctorRoundItems).HasConstraintName("FK__DoctorRou__Hospi__1EA48E88");

            entity.HasOne(d => d.Round).WithMany(p => p.DoctorRoundItems).HasConstraintName("FK__DoctorRou__Versi__1DB06A4F");
        });

        modelBuilder.Entity<Hospitalization>(entity =>
        {
            entity.HasKey(e => e.HospitalizationId).HasName("PK__Hospital__58D1FB439ED200D8");

            entity.Property(e => e.HospitalizationId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedDt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(d => d.AttendingDoctor).WithMany(p => p.Hospitalizations).HasConstraintName("FK__Hospitali__Atten__571DF1D5");

            entity.HasOne(d => d.Patient).WithMany(p => p.Hospitalizations).HasConstraintName("FK__Hospitali__Versi__5629CD9C");
        });

        modelBuilder.Entity<Medication>(entity =>
        {
            entity.HasKey(e => e.MedicationId).HasName("PK__Medicati__8CBD3D897871A588");

            entity.Property(e => e.MedicationId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedDt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.PatientId).HasName("PK__Patients__C1A88B5990161BD0");

            entity.Property(e => e.PatientId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedDt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });

        modelBuilder.Entity<PatientDiagnosis>(entity =>
        {
            entity.HasKey(e => e.PatientDiagnoseId).HasName("PK__PatientD__C7122CB2FE93F4C2");

            entity.Property(e => e.PatientDiagnoseId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedDt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(d => d.Diagnosis).WithMany(p => p.PatientDiagnoses).HasConstraintName("FK__PatientDi__Diagn__628FA481");

            entity.HasOne(d => d.Hospitalization).WithMany(p => p.PatientDiagnoses).HasConstraintName("FK__PatientDi__Versi__619B8048");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__206D9190BB9A6AA3");

            entity.Property(e => e.UserId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedDt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });

        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.HasKey(e => e.UserSessionId).HasName("PK__UserSess__2884045AB8B8C8E9");

            entity.Property(e => e.UserSessionId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedDt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(d => d.User).WithMany(p => p.UserSessions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserSessi__User___40058253");
        });

        modelBuilder.Entity<VitalSign>(entity =>
        {
            entity.HasKey(e => e.VitalSignId).HasName("PK__VitalSig__9645F3132C188739");

            entity.Property(e => e.VitalSignId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedDt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(d => d.Hospitalization).WithMany(p => p.VitalSigns).HasConstraintName("FK__VitalSign__Versi__68487DD7");

            entity.HasOne(d => d.InsUser).WithMany(p => p.VitalSigns).HasConstraintName("FK__VitalSign__InsUs__693CA210");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
