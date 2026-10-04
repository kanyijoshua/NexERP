using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABPmicroservice.Erp.Migrations
{
    /// <inheritdoc />
    public partial class Campus_DefinedBenefit_Payroll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AttendancePct",
                table: "ErpStudentUnits",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "SessionsAttended",
                table: "ErpStudentUnits",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SessionsHeld",
                table: "ErpStudentUnits",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "BenefitCalculationNos",
                table: "ErpPensionSetups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AccrualRatePct",
                table: "ErpPensionSchemes",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CommutationFactor",
                table: "ErpPensionSchemes",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EarlyRetirementReductionPct",
                table: "ErpPensionSchemes",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxCommutationPct",
                table: "ErpPensionSchemes",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "MaxPensionableServiceYears",
                table: "ErpPensionSchemes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AttendanceNos",
                table: "ErpAcademicSetups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClinicVisitNos",
                table: "ErpAcademicSetups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HostelAllocationNos",
                table: "ErpAcademicSetups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LaundryExpressChargePct",
                table: "ErpAcademicSetups",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "LaundryNos",
                table: "ErpAcademicSetups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MedicalFeeItemCode",
                table: "ErpAcademicSetups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinAttendancePct",
                table: "ErpAcademicSetups",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ShortCourseNos",
                table: "ErpAcademicSetups",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ErpAttendanceLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    StudentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StudentName = table.Column<string>(type: "text", nullable: true),
                    Mark = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_ErpAttendanceLines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpAttendanceRegisters",
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
                    LecturerNo = table.Column<string>(type: "text", nullable: true),
                    LessonDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartTime = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    Hours = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Remarks = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PostedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PostedBy = table.Column<string>(type: "text", nullable: true),
                    NoOfStudents = table.Column<int>(type: "integer", nullable: false),
                    NoPresent = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_ErpAttendanceRegisters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpClinicPrescriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    ItemNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Dosage = table.Column<string>(type: "text", nullable: true),
                    LocationCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Issued = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_ErpClinicPrescriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpClinicVisits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PatientType = table.Column<int>(type: "integer", nullable: false),
                    PatientNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PatientName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    VisitDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TreatmentType = table.Column<int>(type: "integer", nullable: false),
                    Complaint = table.Column<string>(type: "text", nullable: true),
                    Diagnosis = table.Column<string>(type: "text", nullable: true),
                    Treatment = table.Column<string>(type: "text", nullable: true),
                    AttendedBy = table.Column<string>(type: "text", nullable: true),
                    ReferredTo = table.Column<string>(type: "text", nullable: true),
                    OffDutyFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OffDutyTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OffDutyDays = table.Column<int>(type: "integer", nullable: false),
                    LightDutyDays = table.Column<int>(type: "integer", nullable: false),
                    OffDutyComments = table.Column<string>(type: "text", nullable: true),
                    Charge = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    BillNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedBy = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_ErpClinicVisits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpEmployeePayItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ItemType = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_ErpEmployeePayItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpHostelAllocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StudentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StudentName = table.Column<string>(type: "text", nullable: true),
                    HostelCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RoomNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SemesterCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AllocationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Remarks = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Charges = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    BillNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ClearanceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProcessedBy = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_ErpHostelAllocations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpHostelRooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HostelCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RoomNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BedSpaces = table.Column<int>(type: "integer", nullable: false),
                    RoomCost = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    OutOfOrder = table.Column<bool>(type: "boolean", nullable: false),
                    OccupiedSpaces = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_ErpHostelRooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpHostels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Gender = table.Column<int>(type: "integer", nullable: false),
                    CostPerOccupant = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    FeeItemCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Blocked = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_ErpHostels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpLaundryItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RatePerItem = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    GLAccountNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_ErpLaundryItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpLaundryOrderLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    LaundryItemCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
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
                    table.PrimaryKey("PK_ErpLaundryOrderLines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpLaundryOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CustomerNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ReceivedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PromisedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Express = table.Column<bool>(type: "boolean", nullable: false),
                    DiscountPct = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Remarks = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    InvoicedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CollectedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProcessedBy = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_ErpLaundryOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpLectureRooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomType = table.Column<int>(type: "integer", nullable: false),
                    BuildingCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    MaximumCapacity = table.Column<int>(type: "integer", nullable: false),
                    Blocked = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_ErpLectureRooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPayrollDeductions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CalculationMethod = table.Column<int>(type: "integer", nullable: false),
                    DefaultValue = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    MaximumAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    TaxDeductible = table.Column<bool>(type: "boolean", nullable: false),
                    EmployerContributionPct = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    GLAccountNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    EmployerExpenseAccountNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Blocked = table.Column<bool>(type: "boolean", nullable: false),
                    Statutory = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_ErpPayrollDeductions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPayrollEarnings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CalculationMethod = table.Column<int>(type: "integer", nullable: false),
                    DefaultValue = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    BasicPay = table.Column<bool>(type: "boolean", nullable: false),
                    Taxable = table.Column<bool>(type: "boolean", nullable: false),
                    GLAccountNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Blocked = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_ErpPayrollEarnings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPayrollRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PayPeriod = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PostedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PostedBy = table.Column<string>(type: "text", nullable: true),
                    NoOfEmployees = table.Column<int>(type: "integer", nullable: false),
                    TotalGross = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    TotalDeductions = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    TotalNet = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    TotalEmployerContributions = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    PaymentVoucherNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_ErpPayrollRuns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPayrollSetups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PayrollRunNos = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PersonalRelief = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
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
                    table.PrimaryKey("PK_ErpPayrollSetups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPayrollTaxBands",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LowerLimit = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    UpperLimit = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    RatePct = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
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
                    table.PrimaryKey("PK_ErpPayrollTaxBands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPayslipLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PayrollRunNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EmployeeNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LineType = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
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
                    table.PrimaryKey("PK_ErpPayslipLines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPayslips",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PayrollRunNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PayPeriod = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EmployeeNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EmployeeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    JobTitle = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BankAccountNo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    BasicPay = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    GrossPay = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    TaxablePay = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    IncomeTax = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    TotalDeductions = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    NetPay = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    EmployerContributions = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
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
                    table.PrimaryKey("PK_ErpPayslips", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPensionBenefitCalculations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MemberNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MemberName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SchemeCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CalculationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RetirementDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FinalPensionableSalary = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    CommutationPct = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    AgeAtRetirement = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    PensionableServiceYears = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    AccrualRatePct = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    CommutationFactor = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    EarlyReductionPct = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    FullAnnualPension = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    ReducedAnnualPension = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    CommutedAnnualPension = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    LumpSum = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    AnnualPension = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    MonthlyPension = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ApprovedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedBy = table.Column<string>(type: "text", nullable: true),
                    PensionerNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PaymentVoucherNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_ErpPensionBenefitCalculations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpShortCourseApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ApplicationType = table.Column<int>(type: "integer", nullable: false),
                    CourseCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CourseDescription = table.Column<string>(type: "text", nullable: true),
                    ApplicationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CustomerNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CustomerName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FeePerParticipant = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Remarks = table.Column<string>(type: "text", nullable: true),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    NoOfParticipants = table.Column<int>(type: "integer", nullable: false),
                    BilledAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    RegisteredDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProcessedBy = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_ErpShortCourseApplications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpShortCourseParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NationalId = table.Column<string>(type: "text", nullable: true),
                    PhoneNo = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    CertificateNo = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: true),
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
                    table.PrimaryKey("PK_ErpShortCourseParticipants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpShortCourses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DurationDays = table.Column<int>(type: "integer", nullable: false),
                    FeePerParticipant = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    GLAccountNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    MaxParticipants = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_ErpShortCourses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpTimetableEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SemesterCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AcademicYearCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    TimetableType = table.Column<int>(type: "integer", nullable: false),
                    ProgrammeCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StageCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    UnitCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    UnitDescription = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Day = table.Column<int>(type: "integer", nullable: false),
                    ExamDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StartTime = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    EndTime = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    RoomCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    LecturerNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Remarks = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_ErpTimetableEntries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ErpAttendanceLines_TenantId_CompanyId_DocumentNo_LineNo",
                table: "ErpAttendanceLines",
                columns: new[] { "TenantId", "CompanyId", "DocumentNo", "LineNo" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpAttendanceRegisters_TenantId_CompanyId_No",
                table: "ErpAttendanceRegisters",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpClinicPrescriptions_TenantId_CompanyId_DocumentNo_LineNo",
                table: "ErpClinicPrescriptions",
                columns: new[] { "TenantId", "CompanyId", "DocumentNo", "LineNo" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpClinicVisits_CompanyId_PatientNo",
                table: "ErpClinicVisits",
                columns: new[] { "CompanyId", "PatientNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpClinicVisits_TenantId_CompanyId_No",
                table: "ErpClinicVisits",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpEmployeePayItems_CompanyId_EmployeeNo",
                table: "ErpEmployeePayItems",
                columns: new[] { "CompanyId", "EmployeeNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpHostelAllocations_CompanyId_StudentNo_SemesterCode",
                table: "ErpHostelAllocations",
                columns: new[] { "CompanyId", "StudentNo", "SemesterCode" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpHostelAllocations_TenantId_CompanyId_No",
                table: "ErpHostelAllocations",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpHostelRooms_TenantId_CompanyId_HostelCode_RoomNo",
                table: "ErpHostelRooms",
                columns: new[] { "TenantId", "CompanyId", "HostelCode", "RoomNo" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpHostels_TenantId_CompanyId_Code",
                table: "ErpHostels",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpLaundryItems_TenantId_CompanyId_Code",
                table: "ErpLaundryItems",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpLaundryOrderLines_TenantId_CompanyId_DocumentNo_LineNo",
                table: "ErpLaundryOrderLines",
                columns: new[] { "TenantId", "CompanyId", "DocumentNo", "LineNo" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpLaundryOrders_CompanyId_CustomerNo",
                table: "ErpLaundryOrders",
                columns: new[] { "CompanyId", "CustomerNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpLaundryOrders_TenantId_CompanyId_No",
                table: "ErpLaundryOrders",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpLectureRooms_TenantId_CompanyId_Code",
                table: "ErpLectureRooms",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPayrollDeductions_TenantId_CompanyId_Code",
                table: "ErpPayrollDeductions",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPayrollEarnings_TenantId_CompanyId_Code",
                table: "ErpPayrollEarnings",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPayrollRuns_TenantId_CompanyId_No",
                table: "ErpPayrollRuns",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPayrollSetups_TenantId_CompanyId",
                table: "ErpPayrollSetups",
                columns: new[] { "TenantId", "CompanyId" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPayrollTaxBands_TenantId_CompanyId_LowerLimit",
                table: "ErpPayrollTaxBands",
                columns: new[] { "TenantId", "CompanyId", "LowerLimit" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPayslipLines_CompanyId_PayrollRunNo_EmployeeNo",
                table: "ErpPayslipLines",
                columns: new[] { "CompanyId", "PayrollRunNo", "EmployeeNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpPayslips_CompanyId_EmployeeNo",
                table: "ErpPayslips",
                columns: new[] { "CompanyId", "EmployeeNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpPayslips_TenantId_CompanyId_PayrollRunNo_EmployeeNo",
                table: "ErpPayslips",
                columns: new[] { "TenantId", "CompanyId", "PayrollRunNo", "EmployeeNo" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPensionBenefitCalculations_CompanyId_MemberNo",
                table: "ErpPensionBenefitCalculations",
                columns: new[] { "CompanyId", "MemberNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpPensionBenefitCalculations_TenantId_CompanyId_No",
                table: "ErpPensionBenefitCalculations",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpShortCourseApplications_TenantId_CompanyId_No",
                table: "ErpShortCourseApplications",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpShortCourseParticipants_TenantId_CompanyId_DocumentNo_Li~",
                table: "ErpShortCourseParticipants",
                columns: new[] { "TenantId", "CompanyId", "DocumentNo", "LineNo" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpShortCourses_TenantId_CompanyId_Code",
                table: "ErpShortCourses",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpTimetableEntries_CompanyId_SemesterCode_Day",
                table: "ErpTimetableEntries",
                columns: new[] { "CompanyId", "SemesterCode", "Day" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ErpAttendanceLines");

            migrationBuilder.DropTable(
                name: "ErpAttendanceRegisters");

            migrationBuilder.DropTable(
                name: "ErpClinicPrescriptions");

            migrationBuilder.DropTable(
                name: "ErpClinicVisits");

            migrationBuilder.DropTable(
                name: "ErpEmployeePayItems");

            migrationBuilder.DropTable(
                name: "ErpHostelAllocations");

            migrationBuilder.DropTable(
                name: "ErpHostelRooms");

            migrationBuilder.DropTable(
                name: "ErpHostels");

            migrationBuilder.DropTable(
                name: "ErpLaundryItems");

            migrationBuilder.DropTable(
                name: "ErpLaundryOrderLines");

            migrationBuilder.DropTable(
                name: "ErpLaundryOrders");

            migrationBuilder.DropTable(
                name: "ErpLectureRooms");

            migrationBuilder.DropTable(
                name: "ErpPayrollDeductions");

            migrationBuilder.DropTable(
                name: "ErpPayrollEarnings");

            migrationBuilder.DropTable(
                name: "ErpPayrollRuns");

            migrationBuilder.DropTable(
                name: "ErpPayrollSetups");

            migrationBuilder.DropTable(
                name: "ErpPayrollTaxBands");

            migrationBuilder.DropTable(
                name: "ErpPayslipLines");

            migrationBuilder.DropTable(
                name: "ErpPayslips");

            migrationBuilder.DropTable(
                name: "ErpPensionBenefitCalculations");

            migrationBuilder.DropTable(
                name: "ErpShortCourseApplications");

            migrationBuilder.DropTable(
                name: "ErpShortCourseParticipants");

            migrationBuilder.DropTable(
                name: "ErpShortCourses");

            migrationBuilder.DropTable(
                name: "ErpTimetableEntries");

            migrationBuilder.DropColumn(
                name: "AttendancePct",
                table: "ErpStudentUnits");

            migrationBuilder.DropColumn(
                name: "SessionsAttended",
                table: "ErpStudentUnits");

            migrationBuilder.DropColumn(
                name: "SessionsHeld",
                table: "ErpStudentUnits");

            migrationBuilder.DropColumn(
                name: "BenefitCalculationNos",
                table: "ErpPensionSetups");

            migrationBuilder.DropColumn(
                name: "AccrualRatePct",
                table: "ErpPensionSchemes");

            migrationBuilder.DropColumn(
                name: "CommutationFactor",
                table: "ErpPensionSchemes");

            migrationBuilder.DropColumn(
                name: "EarlyRetirementReductionPct",
                table: "ErpPensionSchemes");

            migrationBuilder.DropColumn(
                name: "MaxCommutationPct",
                table: "ErpPensionSchemes");

            migrationBuilder.DropColumn(
                name: "MaxPensionableServiceYears",
                table: "ErpPensionSchemes");

            migrationBuilder.DropColumn(
                name: "AttendanceNos",
                table: "ErpAcademicSetups");

            migrationBuilder.DropColumn(
                name: "ClinicVisitNos",
                table: "ErpAcademicSetups");

            migrationBuilder.DropColumn(
                name: "HostelAllocationNos",
                table: "ErpAcademicSetups");

            migrationBuilder.DropColumn(
                name: "LaundryExpressChargePct",
                table: "ErpAcademicSetups");

            migrationBuilder.DropColumn(
                name: "LaundryNos",
                table: "ErpAcademicSetups");

            migrationBuilder.DropColumn(
                name: "MedicalFeeItemCode",
                table: "ErpAcademicSetups");

            migrationBuilder.DropColumn(
                name: "MinAttendancePct",
                table: "ErpAcademicSetups");

            migrationBuilder.DropColumn(
                name: "ShortCourseNos",
                table: "ErpAcademicSetups");
        }
    }
}
