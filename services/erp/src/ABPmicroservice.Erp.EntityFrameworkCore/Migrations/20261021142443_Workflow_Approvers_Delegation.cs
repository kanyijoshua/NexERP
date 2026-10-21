using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABPmicroservice.Erp.Migrations
{
    /// <inheritdoc />
    public partial class Workflow_Approvers_Delegation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApproverType",
                table: "ErpWorkflows",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DelegateAfterDays",
                table: "ErpWorkflows",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "SpecificApproverUserId",
                table: "ErpWorkflows",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpecificApproverUserName",
                table: "ErpWorkflows",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkflowUserGroupCode",
                table: "ErpWorkflows",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApproverType",
                table: "ErpWorkflows");

            migrationBuilder.DropColumn(
                name: "DelegateAfterDays",
                table: "ErpWorkflows");

            migrationBuilder.DropColumn(
                name: "SpecificApproverUserId",
                table: "ErpWorkflows");

            migrationBuilder.DropColumn(
                name: "SpecificApproverUserName",
                table: "ErpWorkflows");

            migrationBuilder.DropColumn(
                name: "WorkflowUserGroupCode",
                table: "ErpWorkflows");
        }
    }
}
