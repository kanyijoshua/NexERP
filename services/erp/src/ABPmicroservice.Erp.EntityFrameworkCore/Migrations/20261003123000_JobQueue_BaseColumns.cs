using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABPmicroservice.Erp.Migrations
{
    /// <inheritdoc />
    public partial class JobQueue_BaseColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "ErpJobQueueEntries",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecordIdToProcess",
                table: "ErpJobQueueEntries",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NextRunDateFormula",
                table: "ErpJobQueueEntries",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReferenceStartingTime",
                table: "ErpJobQueueEntries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastReadyState",
                table: "ErpJobQueueEntries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyOnSuccess",
                table: "ErpJobQueueEntries",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Scheduled",
                table: "ErpJobQueueEntries",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ManualRecurrence",
                table: "ErpJobQueueEntries",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "InactivityTimeoutPeriod",
                table: "ErpJobQueueEntries",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SystemTaskId",
                table: "ErpJobQueueEntries",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "ErpJobQueueLogEntries",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CategoryCode",
                table: "ErpJobQueueLogEntries",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParameterString",
                table: "ErpJobQueueLogEntries",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SystemTaskId",
                table: "ErpJobQueueLogEntries",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ErpJobQueueEntries");

            migrationBuilder.DropColumn(
                name: "RecordIdToProcess",
                table: "ErpJobQueueEntries");

            migrationBuilder.DropColumn(
                name: "NextRunDateFormula",
                table: "ErpJobQueueEntries");

            migrationBuilder.DropColumn(
                name: "ReferenceStartingTime",
                table: "ErpJobQueueEntries");

            migrationBuilder.DropColumn(
                name: "LastReadyState",
                table: "ErpJobQueueEntries");

            migrationBuilder.DropColumn(
                name: "NotifyOnSuccess",
                table: "ErpJobQueueEntries");

            migrationBuilder.DropColumn(
                name: "Scheduled",
                table: "ErpJobQueueEntries");

            migrationBuilder.DropColumn(
                name: "ManualRecurrence",
                table: "ErpJobQueueEntries");

            migrationBuilder.DropColumn(
                name: "InactivityTimeoutPeriod",
                table: "ErpJobQueueEntries");

            migrationBuilder.DropColumn(
                name: "SystemTaskId",
                table: "ErpJobQueueEntries");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ErpJobQueueLogEntries");

            migrationBuilder.DropColumn(
                name: "CategoryCode",
                table: "ErpJobQueueLogEntries");

            migrationBuilder.DropColumn(
                name: "ParameterString",
                table: "ErpJobQueueLogEntries");

            migrationBuilder.DropColumn(
                name: "SystemTaskId",
                table: "ErpJobQueueLogEntries");
        }
    }
}
