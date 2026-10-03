using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABPmicroservice.Erp.Migrations
{
    /// <inheritdoc />
    public partial class JobQueue_Schema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ErpJobQueueCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_ErpJobQueueCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpJobQueueEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    CategoryCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    JobType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ParameterString = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    EarliestStartDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpirationDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RecurringJob = table.Column<bool>(type: "boolean", nullable: false),
                    RunOnMondays = table.Column<bool>(type: "boolean", nullable: false),
                    RunOnTuesdays = table.Column<bool>(type: "boolean", nullable: false),
                    RunOnWednesdays = table.Column<bool>(type: "boolean", nullable: false),
                    RunOnThursdays = table.Column<bool>(type: "boolean", nullable: false),
                    RunOnFridays = table.Column<bool>(type: "boolean", nullable: false),
                    RunOnSaturdays = table.Column<bool>(type: "boolean", nullable: false),
                    RunOnSundays = table.Column<bool>(type: "boolean", nullable: false),
                    DailyStartingTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    DailyEndingTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    IntervalType = table.Column<int>(type: "integer", nullable: false),
                    IntervalMinutes = table.Column<int>(type: "integer", nullable: false),
                    NextRunTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastRunTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastHeartbeat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NoOfAttemptsToRun = table.Column<int>(type: "integer", nullable: false),
                    MaxNoOfAttemptsToRun = table.Column<int>(type: "integer", nullable: false),
                    RerunDelaySeconds = table.Column<int>(type: "integer", nullable: false),
                    TimeoutSeconds = table.Column<int>(type: "integer", nullable: false),
                    LastErrorMessage = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    LastErrorStackTrace = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
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
                    table.PrimaryKey("PK_ErpJobQueueEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpJobQueueLogEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobQueueEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    JobDescription = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    JobType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StartDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DurationMs = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ErrorStackTrace = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ProcessedRecords = table.Column<int>(type: "integer", nullable: false),
                    OutputDetails = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
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
                    table.PrimaryKey("PK_ErpJobQueueLogEntries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ErpJobQueueCategories_TenantId_CompanyId_Code",
                table: "ErpJobQueueCategories",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpJobQueueEntries_CompanyId_Status_NextRunTime",
                table: "ErpJobQueueEntries",
                columns: new[] { "CompanyId", "Status", "NextRunTime" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpJobQueueLogEntries_CompanyId_JobQueueEntryId_StartDateTi~",
                table: "ErpJobQueueLogEntries",
                columns: new[] { "CompanyId", "JobQueueEntryId", "StartDateTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ErpJobQueueCategories");

            migrationBuilder.DropTable(
                name: "ErpJobQueueEntries");

            migrationBuilder.DropTable(
                name: "ErpJobQueueLogEntries");
        }
    }
}
