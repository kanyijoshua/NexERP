using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABPmicroservice.Erp.Migrations
{
    /// <inheritdoc />
    public partial class Chatter_Record_Indexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ErpDocumentNotes_CompanyId_EntityType_EntityId",
                table: "ErpDocumentNotes",
                columns: new[] { "CompanyId", "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpDocumentActivityTasks_CompanyId_EntityType_EntityId",
                table: "ErpDocumentActivityTasks",
                columns: new[] { "CompanyId", "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpActivityStreamEntries_CompanyId_EntityType_EntityId",
                table: "ErpActivityStreamEntries",
                columns: new[] { "CompanyId", "EntityType", "EntityId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ErpDocumentNotes_CompanyId_EntityType_EntityId",
                table: "ErpDocumentNotes");

            migrationBuilder.DropIndex(
                name: "IX_ErpDocumentActivityTasks_CompanyId_EntityType_EntityId",
                table: "ErpDocumentActivityTasks");

            migrationBuilder.DropIndex(
                name: "IX_ErpActivityStreamEntries_CompanyId_EntityType_EntityId",
                table: "ErpActivityStreamEntries");
        }
    }
}
