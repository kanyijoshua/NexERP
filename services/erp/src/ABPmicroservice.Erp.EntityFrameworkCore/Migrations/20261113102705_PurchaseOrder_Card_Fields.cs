using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABPmicroservice.Erp.Migrations
{
    /// <inheritdoc />
    public partial class PurchaseOrder_Card_Fields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Area",
                table: "ErpPurchaseHeaders",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CampaignNo",
                table: "ErpPurchaseHeaders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CompressPrepayment",
                table: "ErpPurchaseHeaders",
                type: "boolean",
                nullable: false,
                // Existing documents compress their prepayment lines, as new ones do by default.
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "EntryPoint",
                table: "ErpPurchaseHeaders",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InboundWhseHandlingTime",
                table: "ErpPurchaseHeaders",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LeadTimeCalculation",
                table: "ErpPurchaseHeaders",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PrepaymentDueDate",
                table: "ErpPurchaseHeaders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrepmtPaymentDiscountPct",
                table: "ErpPurchaseHeaders",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PrepmtPaymentTermsCode",
                table: "ErpPurchaseHeaders",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PrepmtPmtDiscountDate",
                table: "ErpPurchaseHeaders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransactionSpecification",
                table: "ErpPurchaseHeaders",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransactionType",
                table: "ErpPurchaseHeaders",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransportMethod",
                table: "ErpPurchaseHeaders",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VatReportingDate",
                table: "ErpPurchaseHeaders",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Area",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "CampaignNo",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "CompressPrepayment",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "EntryPoint",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "InboundWhseHandlingTime",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "LeadTimeCalculation",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "PrepaymentDueDate",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "PrepmtPaymentDiscountPct",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "PrepmtPaymentTermsCode",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "PrepmtPmtDiscountDate",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "TransactionSpecification",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "TransactionType",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "TransportMethod",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "VatReportingDate",
                table: "ErpPurchaseHeaders");
        }
    }
}
