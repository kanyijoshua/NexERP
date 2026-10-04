using ABPmicroservice.Erp.Academics;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

public partial class ErpDbContext
{
    public DbSet<AcademicSetup> AcademicSetups { get; set; }
    public DbSet<AcademicYear> AcademicYears { get; set; }
    public DbSet<Semester> Semesters { get; set; }
    public DbSet<Intake> Intakes { get; set; }
    public DbSet<ExamCategory> ExamCategories { get; set; }
    public DbSet<GradingBand> GradingBands { get; set; }
    public DbSet<ExamComponent> ExamComponents { get; set; }
    public DbSet<Programme> Programmes { get; set; }
    public DbSet<ProgrammeStage> ProgrammeStages { get; set; }
    public DbSet<CourseUnit> CourseUnits { get; set; }
    public DbSet<FeeItem> FeeItems { get; set; }
    public DbSet<FeeStructureLine> FeeStructureLines { get; set; }
    public DbSet<StudentApplication> StudentApplications { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<SemesterRegistration> SemesterRegistrations { get; set; }
    public DbSet<StudentUnit> StudentUnits { get; set; }
    public DbSet<StudentBillHeader> StudentBillHeaders { get; set; }
    public DbSet<StudentBillLine> StudentBillLines { get; set; }
    public DbSet<ExamResultHeader> ExamResultHeaders { get; set; }
    public DbSet<ExamResultLine> ExamResultLines { get; set; }
    public DbSet<StudentReceipt> StudentReceipts { get; set; }
    public DbSet<StudentRefund> StudentRefunds { get; set; }
    public DbSet<StudentStatusChange> StudentStatusChanges { get; set; }
    public DbSet<LectureRoom> LectureRooms { get; set; }
    public DbSet<TimetableEntry> TimetableEntries { get; set; }
    public DbSet<AttendanceRegister> AttendanceRegisters { get; set; }
    public DbSet<AttendanceLine> AttendanceLines { get; set; }
    public DbSet<Hostel> Hostels { get; set; }
    public DbSet<HostelRoom> HostelRooms { get; set; }
    public DbSet<HostelAllocation> HostelAllocations { get; set; }
    public DbSet<ClinicVisit> ClinicVisits { get; set; }
    public DbSet<ClinicPrescription> ClinicPrescriptions { get; set; }
    public DbSet<LaundryItem> LaundryItems { get; set; }
    public DbSet<LaundryOrder> LaundryOrders { get; set; }
    public DbSet<LaundryOrderLine> LaundryOrderLines { get; set; }
    public DbSet<ShortCourse> ShortCourses { get; set; }
    public DbSet<ShortCourseApplication> ShortCourseApplications { get; set; }
    public DbSet<ShortCourseParticipant> ShortCourseParticipants { get; set; }
}

public static class ErpAcademicsModelCreatingExtensions
{
    public static void ConfigureErpAcademics(this ModelBuilder builder)
    {
        builder.Entity<AcademicSetup>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "AcademicSetups", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex();
        });

        builder.Entity<AcademicYear>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "AcademicYears", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<Semester>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "Semesters", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
            b.Property(x => x.AcademicYearCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
        });

        builder.Entity<Intake>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "Intakes", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
            b.Property(x => x.AcademicYearCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
        });

        builder.Entity<ExamCategory>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ExamCategories", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<GradingBand>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "GradingBands", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(GradingBand.ExamCategoryCode), nameof(GradingBand.Grade));
            b.Property(x => x.ExamCategoryCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.Grade).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.Remarks).HasMaxLength(ErpDomainConsts.MaxNameLength);
        });

        builder.Entity<ExamComponent>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ExamComponents", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(ExamComponent.ExamCategoryCode), nameof(ExamComponent.ExamType));
            b.Property(x => x.ExamCategoryCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
        });

        builder.Entity<Programme>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "Programmes", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
            b.Property(x => x.ExamCategoryCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.StudentNos).HasMaxLength(ErpDomainConsts.MaxNoSeriesCodeLength);
            b.Property(x => x.StudentNoPrefix).HasMaxLength(AcademicConsts.MaxStudentNoAffixLength);
            b.Property(x => x.StudentNoSuffix).HasMaxLength(AcademicConsts.MaxStudentNoAffixLength);
        });

        builder.Entity<ProgrammeStage>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ProgrammeStages", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(ProgrammeStage.ProgrammeCode), nameof(ProgrammeStage.Code));
            b.Property(x => x.ProgrammeCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.Code).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
        });

        builder.Entity<CourseUnit>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "CourseUnits", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(CourseUnit.ProgrammeCode), nameof(CourseUnit.Code));
            b.HasIndex(x => new { x.CompanyId, x.ProgrammeCode, x.StageCode });
            b.Property(x => x.ProgrammeCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.Code).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.StageCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.SemesterCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.PrerequisiteUnitCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
        });

        builder.Entity<FeeItem>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "FeeItems", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
            b.Property(x => x.GLAccountNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
        });

        builder.Entity<FeeStructureLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "FeeStructureLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(
                nameof(FeeStructureLine.ProgrammeCode),
                nameof(FeeStructureLine.StageCode),
                nameof(FeeStructureLine.SemesterCode),
                nameof(FeeStructureLine.StudyMode),
                nameof(FeeStructureLine.FeeItemCode)
            );
            b.Property(x => x.ProgrammeCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.StageCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.SemesterCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.FeeItemCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
        });

        builder.Entity<StudentApplication>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "StudentApplications", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.FullName).IsRequired().HasMaxLength(ErpDomainConsts.MaxNameLength * 2);
            b.Property(x => x.ProgrammeCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.StudentNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Ignore(x => x.IsOpen);
        });

        builder.Entity<Student>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "Students", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.HasIndex(x => new { x.CompanyId, x.ProgrammeCode, x.CurrentStageCode });
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.FullName).IsRequired().HasMaxLength(ErpDomainConsts.MaxNameLength * 2);
            b.Property(x => x.ProgrammeCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.CurrentStageCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.CurrentSemesterCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.CustomerNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.ApplicationNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Ignore(x => x.IsActive);
        });

        builder.Entity<SemesterRegistration>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "SemesterRegistrations", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.HasIndex(x => new { x.CompanyId, x.StudentNo });
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.StudentNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.ProgrammeCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.StageCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.SemesterCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.BillNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Ignore(x => x.IsOpen);
        });

        builder.Entity<StudentUnit>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "StudentUnits", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(StudentUnit.RegistrationNo), nameof(StudentUnit.UnitCode));
            b.HasIndex(x => new { x.CompanyId, x.StudentNo });
            b.HasIndex(x => new { x.CompanyId, x.ProgrammeCode, x.UnitCode, x.SemesterCode });
            b.Property(x => x.RegistrationNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.StudentNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.ProgrammeCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.UnitCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.Grade).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Ignore(x => x.HasMarks);
        });

        builder.Entity<StudentBillHeader>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "StudentBillHeaders", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.HasIndex(x => new { x.CompanyId, x.StudentNo });
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.StudentNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Ignore(x => x.IsOpen);
        });

        builder.Entity<StudentBillLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "StudentBillLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(StudentBillLine.DocumentNo), nameof(StudentBillLine.LineNo));
            b.Property(x => x.DocumentNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.FeeItemCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
        });

        builder.Entity<ExamResultHeader>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ExamResultHeaders", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.ProgrammeCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.UnitCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.SemesterCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Ignore(x => x.IsOpen);
        });

        builder.Entity<StudentReceipt>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "StudentReceipts", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.HasIndex(x => new { x.CompanyId, x.StudentNo });
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.StudentNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.BankAccountNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.PayMode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.ExternalDocumentNo).HasMaxLength(ErpDomainConsts.MaxExternalDocumentNoLength);
            b.Property(x => x.AppliesToBillNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Ignore(x => x.IsOpen);
        });

        builder.Entity<StudentRefund>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "StudentRefunds", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.HasIndex(x => new { x.CompanyId, x.StudentNo });
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.StudentNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.PaymentVoucherNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Ignore(x => x.IsOpen);
        });

        builder.Entity<StudentStatusChange>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "StudentStatusChanges", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.HasIndex(x => new { x.CompanyId, x.StudentNo });
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.StudentNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Ignore(x => x.IsOpen);
        });

        builder.Entity<ExamResultLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ExamResultLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(ExamResultLine.DocumentNo), nameof(ExamResultLine.LineNo));
            b.Property(x => x.DocumentNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.StudentNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
        });

        builder.ConfigureErpCampus();
    }

    /// <summary>Timetable, class attendance, hostels, infirmary, laundry and short courses.</summary>
    private static void ConfigureErpCampus(this ModelBuilder builder)
    {
        builder.Entity<LectureRoom>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "LectureRooms", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
            b.Property(x => x.BuildingCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
        });

        builder.Entity<TimetableEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "TimetableEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.SemesterCode, x.Day });
            b.Property(x => x.SemesterCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.AcademicYearCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.ProgrammeCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.StageCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.UnitCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.UnitDescription).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.StartTime).IsRequired().HasMaxLength(5);
            b.Property(x => x.EndTime).IsRequired().HasMaxLength(5);
            b.Property(x => x.RoomCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.LecturerNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Remarks).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
        });

        builder.Entity<AttendanceRegister>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "AttendanceRegisters", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.ProgrammeCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.UnitCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.SemesterCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.StartTime).HasMaxLength(5);
            b.Ignore(x => x.IsOpen);
        });

        builder.Entity<AttendanceLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "AttendanceLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(AttendanceLine.DocumentNo), nameof(AttendanceLine.LineNo));
            b.Property(x => x.DocumentNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.StudentNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
        });

        builder.Entity<Hostel>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "Hostels", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
            b.Property(x => x.FeeItemCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
        });

        builder.Entity<HostelRoom>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "HostelRooms", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(HostelRoom.HostelCode), nameof(HostelRoom.RoomNo));
            b.Property(x => x.HostelCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.RoomNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Ignore(x => x.VacantSpaces);
        });

        builder.Entity<HostelAllocation>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "HostelAllocations", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.HasIndex(x => new { x.CompanyId, x.StudentNo, x.SemesterCode });
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.StudentNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.HostelCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.RoomNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.SemesterCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.BillNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
        });

        builder.Entity<ClinicVisit>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ClinicVisits", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.HasIndex(x => new { x.CompanyId, x.PatientNo });
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.PatientNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.PatientName).IsRequired().HasMaxLength(ErpDomainConsts.MaxNameLength * 2);
            b.Property(x => x.BillNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Ignore(x => x.IsOpen);
        });

        builder.Entity<ClinicPrescription>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ClinicPrescriptions", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(ClinicPrescription.DocumentNo), nameof(ClinicPrescription.LineNo));
            b.Property(x => x.DocumentNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.ItemNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.LocationCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
        });

        builder.Entity<LaundryItem>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "LaundryItems", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
            b.Property(x => x.GLAccountNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
        });

        builder.Entity<LaundryOrder>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "LaundryOrders", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.HasIndex(x => new { x.CompanyId, x.CustomerNo });
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.CustomerNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.CustomerName).HasMaxLength(ErpDomainConsts.MaxNameLength);
        });

        builder.Entity<LaundryOrderLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "LaundryOrderLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(LaundryOrderLine.DocumentNo), nameof(LaundryOrderLine.LineNo));
            b.Property(x => x.DocumentNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.LaundryItemCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
        });

        builder.Entity<ShortCourse>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ShortCourses", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
            b.Property(x => x.GLAccountNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
        });

        builder.Entity<ShortCourseApplication>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ShortCourseApplications", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.CourseCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.CustomerNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.CustomerName).HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Ignore(x => x.IsOpen);
        });

        builder.Entity<ShortCourseParticipant>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ShortCourseParticipants", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(ShortCourseParticipant.DocumentNo), nameof(ShortCourseParticipant.LineNo));
            b.Property(x => x.DocumentNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.Name).IsRequired().HasMaxLength(ErpDomainConsts.MaxNameLength * 2);
            b.Property(x => x.CertificateNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength + 4);
        });
    }
}
