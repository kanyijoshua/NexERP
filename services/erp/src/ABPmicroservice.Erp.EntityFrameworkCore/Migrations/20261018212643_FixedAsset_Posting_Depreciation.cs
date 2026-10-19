using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABPmicroservice.Erp.Migrations
{
    /// <inheritdoc />
    public partial class FixedAsset_Posting_Depreciation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ErpFAJournalBatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    DepreciationBookCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
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
                    table.PrimaryKey("PK_ErpFAJournalBatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpFAJournalLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FAJournalBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DocumentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FANo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DepreciationBookCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    FAPostingType = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    BalAccountType = table.Column<int>(type: "integer", nullable: true),
                    BalAccountNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    NoOfDepreciationDays = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_ErpFAJournalLines", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ErpFALedgerEntries_CompanyId_FANo_DepreciationBookCode",
                table: "ErpFALedgerEntries",
                columns: new[] { "CompanyId", "FANo", "DepreciationBookCode" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpFAJournalBatches_TenantId_CompanyId_Name",
                table: "ErpFAJournalBatches",
                columns: new[] { "TenantId", "CompanyId", "Name" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpFAJournalLines_FAJournalBatchId_LineNo",
                table: "ErpFAJournalLines",
                columns: new[] { "FAJournalBatchId", "LineNo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ErpFAJournalBatches");

            migrationBuilder.DropTable(
                name: "ErpFAJournalLines");

            migrationBuilder.DropIndex(
                name: "IX_ErpFALedgerEntries_CompanyId_FANo_DepreciationBookCode",
                table: "ErpFALedgerEntries");
        }
    }
}
