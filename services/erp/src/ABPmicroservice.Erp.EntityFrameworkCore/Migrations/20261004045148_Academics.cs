using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABPmicroservice.Erp.Migrations
{
    /// <inheritdoc />
    public partial class Academics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ErpAcademicSetups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationNos = table.Column<string>(type: "text", nullable: true),
                    StudentNos = table.Column<string>(type: "text", nullable: true),
                    RegistrationNos = table.Column<string>(type: "text", nullable: true),
                    BillingNos = table.Column<string>(type: "text", nullable: true),
                    ExamResultNos = table.Column<string>(type: "text", nullable: true),
                    StudentPostingGroup = table.Column<string>(type: "text", nullable: true),
                    StudentGenBusPostingGroup = table.Column<string>(type: "text", nullable: true),
                    CheckStudentBalance = table.Column<bool>(type: "boolean", nullable: false),
                    MaxFeeBalanceToRegister = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    BillOnRegistration = table.Column<bool>(type: "boolean", nullable: false),
                    ExamRoundingDecimals = table.Column<int>(type: "integer", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpAcademicSetups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpAcademicYears",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Current = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpAcademicYears", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpCourseUnits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgrammeCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    StageCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SemesterCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    UnitType = table.Column<int>(type: "integer", nullable: false),
                    CreditHours = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    PrerequisiteUnitCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Blocked = table.Column<bool>(type: "boolean", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpCourseUnits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpExamCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BlockResultsEntry = table.Column<bool>(type: "boolean", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpExamCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpExamComponents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamCategoryCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ExamType = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    MaxScore = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    ContributionPct = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpExamComponents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpExamResultHeaders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ProgrammeCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StageCode = table.Column<string>(type: "text", nullable: true),
                    SemesterCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AcademicYearCode = table.Column<string>(type: "text", nullable: true),
                    UnitCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    UnitDescription = table.Column<string>(type: "text", nullable: true),
                    ExamType = table.Column<int>(type: "integer", nullable: false),
                    LecturerNo = table.Column<string>(type: "text", nullable: true),
                    DocumentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PostedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PostedBy = table.Column<string>(type: "text", nullable: true),
                    NoOfStudents = table.Column<int>(type: "integer", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpExamResultHeaders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpExamResultLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    StudentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StudentName = table.Column<string>(type: "text", nullable: true),
                    Mark = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    NotDone = table.Column<bool>(type: "boolean", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpExamResultLines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpFeeItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FeeType = table.Column<int>(type: "integer", nullable: false),
                    GLAccountNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DefaultAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    TuitionFee = table.Column<bool>(type: "boolean", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpFeeItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpFeeStructureLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgrammeCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StageCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SemesterCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    StudyMode = table.Column<int>(type: "integer", nullable: false),
                    FeeItemCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpFeeStructureLines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpGradingBands",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamCategoryCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Grade = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    FromMark = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    ToMark = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Points = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Remarks = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Passed = table.Column<bool>(type: "boolean", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpGradingBands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpIntakes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AcademicYearCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Current = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpIntakes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpProgrammes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    ExamCategoryCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DurationMonths = table.Column<int>(type: "integer", nullable: false),
                    MinimumCapacity = table.Column<int>(type: "integer", nullable: false),
                    MaximumCapacity = table.Column<int>(type: "integer", nullable: false),
                    StudentNos = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    StudentNoPrefix = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    StudentNoSuffix = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpProgrammes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpProgrammeStages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgrammeCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    FinalStage = table.Column<bool>(type: "boolean", nullable: false),
                    MinimumUnits = table.Column<int>(type: "integer", nullable: false),
                    MaximumUnits = table.Column<int>(type: "integer", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpProgrammeStages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpSemesterRegistrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StudentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StudentName = table.Column<string>(type: "text", nullable: true),
                    ProgrammeCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StageCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SemesterCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AcademicYearCode = table.Column<string>(type: "text", nullable: true),
                    RegistrationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RegisterFor = table.Column<int>(type: "integer", nullable: false),
                    Remarks = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    NoOfUnits = table.Column<int>(type: "integer", nullable: false),
                    BillNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    BilledAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpSemesterRegistrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpSemesters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AcademicYearCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    RegistrationFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RegistrationTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Current = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpSemesters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpStudentApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ApplicationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    OtherName = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Gender = table.Column<int>(type: "integer", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NationalId = table.Column<string>(type: "text", nullable: true),
                    PhoneNo = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    ProgrammeCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IntakeCode = table.Column<string>(type: "text", nullable: true),
                    AcademicYearCode = table.Column<string>(type: "text", nullable: true),
                    StudyMode = table.Column<int>(type: "integer", nullable: false),
                    FormerSchool = table.Column<string>(type: "text", nullable: true),
                    IndexNumber = table.Column<string>(type: "text", nullable: true),
                    MeanGrade = table.Column<string>(type: "text", nullable: true),
                    Sponsorship = table.Column<int>(type: "integer", nullable: false),
                    GuardianName = table.Column<string>(type: "text", nullable: true),
                    GuardianPhoneNo = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    StudentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AdmissionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpStudentApplications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpStudentBillHeaders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StudentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StudentName = table.Column<string>(type: "text", nullable: true),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProgrammeCode = table.Column<string>(type: "text", nullable: true),
                    StageCode = table.Column<string>(type: "text", nullable: true),
                    SemesterCode = table.Column<string>(type: "text", nullable: true),
                    AcademicYearCode = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    RegistrationNo = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PostedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PostedBy = table.Column<string>(type: "text", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpStudentBillHeaders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpStudentBillLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    FeeItemCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpStudentBillLines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpStudents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    OtherName = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Gender = table.Column<int>(type: "integer", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NationalId = table.Column<string>(type: "text", nullable: true),
                    PhoneNo = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    ProgrammeCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CurrentStageCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CurrentSemesterCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IntakeCode = table.Column<string>(type: "text", nullable: true),
                    AcademicYearCode = table.Column<string>(type: "text", nullable: true),
                    StudyMode = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AdmissionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CustomerNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ApplicationNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Sponsorship = table.Column<int>(type: "integer", nullable: false),
                    GuardianName = table.Column<string>(type: "text", nullable: true),
                    GuardianPhoneNo = table.Column<string>(type: "text", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpStudents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpStudentUnits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RegistrationNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StudentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StudentName = table.Column<string>(type: "text", nullable: true),
                    ProgrammeCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StageCode = table.Column<string>(type: "text", nullable: true),
                    SemesterCode = table.Column<string>(type: "text", nullable: true),
                    AcademicYearCode = table.Column<string>(type: "text", nullable: true),
                    UnitCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    UnitDescription = table.Column<string>(type: "text", nullable: true),
                    UnitType = table.Column<int>(type: "integer", nullable: false),
                    CreditHours = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    AssignmentMark = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: true),
                    CatMark = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: true),
                    Cat2Mark = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: true),
                    ExamMark = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: true),
                    FinalScore = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Grade = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Points = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    ResultRemarks = table.Column<string>(type: "text", nullable: true),
                    Passed = table.Column<bool>(type: "boolean", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpStudentUnits", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ErpAcademicSetups_TenantId_CompanyId",
                table: "ErpAcademicSetups",
                columns: new[] { "TenantId", "CompanyId" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpAcademicYears_TenantId_CompanyId_Code",
                table: "ErpAcademicYears",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpCourseUnits_CompanyId_ProgrammeCode_StageCode",
                table: "ErpCourseUnits",
                columns: new[] { "CompanyId", "ProgrammeCode", "StageCode" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpCourseUnits_TenantId_CompanyId_ProgrammeCode_Code",
                table: "ErpCourseUnits",
                columns: new[] { "TenantId", "CompanyId", "ProgrammeCode", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpExamCategories_TenantId_CompanyId_Code",
                table: "ErpExamCategories",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpExamComponents_TenantId_CompanyId_ExamCategoryCode_ExamT~",
                table: "ErpExamComponents",
                columns: new[] { "TenantId", "CompanyId", "ExamCategoryCode", "ExamType" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpExamResultHeaders_TenantId_CompanyId_No",
                table: "ErpExamResultHeaders",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpExamResultLines_TenantId_CompanyId_DocumentNo_LineNo",
                table: "ErpExamResultLines",
                columns: new[] { "TenantId", "CompanyId", "DocumentNo", "LineNo" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpFeeItems_TenantId_CompanyId_Code",
                table: "ErpFeeItems",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpFeeStructureLines_TenantId_CompanyId_ProgrammeCode_Stage~",
                table: "ErpFeeStructureLines",
                columns: new[] { "TenantId", "CompanyId", "ProgrammeCode", "StageCode", "SemesterCode", "StudyMode", "FeeItemCode" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpGradingBands_TenantId_CompanyId_ExamCategoryCode_Grade",
                table: "ErpGradingBands",
                columns: new[] { "TenantId", "CompanyId", "ExamCategoryCode", "Grade" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpIntakes_TenantId_CompanyId_Code",
                table: "ErpIntakes",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpProgrammes_TenantId_CompanyId_Code",
                table: "ErpProgrammes",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpProgrammeStages_TenantId_CompanyId_ProgrammeCode_Code",
                table: "ErpProgrammeStages",
                columns: new[] { "TenantId", "CompanyId", "ProgrammeCode", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpSemesterRegistrations_CompanyId_StudentNo",
                table: "ErpSemesterRegistrations",
                columns: new[] { "CompanyId", "StudentNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpSemesterRegistrations_TenantId_CompanyId_No",
                table: "ErpSemesterRegistrations",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpSemesters_TenantId_CompanyId_Code",
                table: "ErpSemesters",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpStudentApplications_TenantId_CompanyId_No",
                table: "ErpStudentApplications",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpStudentBillHeaders_CompanyId_StudentNo",
                table: "ErpStudentBillHeaders",
                columns: new[] { "CompanyId", "StudentNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpStudentBillHeaders_TenantId_CompanyId_No",
                table: "ErpStudentBillHeaders",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpStudentBillLines_TenantId_CompanyId_DocumentNo_LineNo",
                table: "ErpStudentBillLines",
                columns: new[] { "TenantId", "CompanyId", "DocumentNo", "LineNo" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpStudents_CompanyId_ProgrammeCode_CurrentStageCode",
                table: "ErpStudents",
                columns: new[] { "CompanyId", "ProgrammeCode", "CurrentStageCode" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpStudents_TenantId_CompanyId_No",
                table: "ErpStudents",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpStudentUnits_CompanyId_ProgrammeCode_UnitCode_SemesterCo~",
                table: "ErpStudentUnits",
                columns: new[] { "CompanyId", "ProgrammeCode", "UnitCode", "SemesterCode" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpStudentUnits_CompanyId_StudentNo",
                table: "ErpStudentUnits",
                columns: new[] { "CompanyId", "StudentNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpStudentUnits_TenantId_CompanyId_RegistrationNo_UnitCode",
                table: "ErpStudentUnits",
                columns: new[] { "TenantId", "CompanyId", "RegistrationNo", "UnitCode" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ErpAcademicSetups");

            migrationBuilder.DropTable(
                name: "ErpAcademicYears");

            migrationBuilder.DropTable(
                name: "ErpCourseUnits");

            migrationBuilder.DropTable(
                name: "ErpExamCategories");

            migrationBuilder.DropTable(
                name: "ErpExamComponents");

            migrationBuilder.DropTable(
                name: "ErpExamResultHeaders");

            migrationBuilder.DropTable(
                name: "ErpExamResultLines");

            migrationBuilder.DropTable(
                name: "ErpFeeItems");

            migrationBuilder.DropTable(
                name: "ErpFeeStructureLines");

            migrationBuilder.DropTable(
                name: "ErpGradingBands");

            migrationBuilder.DropTable(
                name: "ErpIntakes");

            migrationBuilder.DropTable(
                name: "ErpProgrammes");

            migrationBuilder.DropTable(
                name: "ErpProgrammeStages");

            migrationBuilder.DropTable(
                name: "ErpSemesterRegistrations");

            migrationBuilder.DropTable(
                name: "ErpSemesters");

            migrationBuilder.DropTable(
                name: "ErpStudentApplications");

            migrationBuilder.DropTable(
                name: "ErpStudentBillHeaders");

            migrationBuilder.DropTable(
                name: "ErpStudentBillLines");

            migrationBuilder.DropTable(
                name: "ErpStudents");

            migrationBuilder.DropTable(
                name: "ErpStudentUnits");
        }
    }
}
