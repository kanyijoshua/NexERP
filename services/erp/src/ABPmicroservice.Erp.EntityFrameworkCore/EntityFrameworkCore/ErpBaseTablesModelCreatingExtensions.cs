using ABPmicroservice.Erp.CashManagement;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.FixedAssets;
using ABPmicroservice.Erp.HumanResources;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Reporting;
using ABPmicroservice.Erp.Sales;
using ABPmicroservice.Erp.Workflows;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

public static class ErpBaseTablesModelCreatingExtensions
{
    public static void ConfigureErpBaseTables(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<SourceCode>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "SourceCodes", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<ReasonCode>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ReasonCodes", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<CountryRegion>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "CountriesRegions", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
            b.Property(x => x.IsoCode).HasMaxLength(2);
            b.Property(x => x.IsoNumericCode).HasMaxLength(3);
            b.Property(x => x.EuCountryRegionCode).HasMaxLength(10);
            b.Property(x => x.IntrastatCode).HasMaxLength(10);
            b.Property(x => x.VatScheme).HasMaxLength(10);
            b.Property(x => x.CountyName).HasMaxLength(30);
        });

        builder.Entity<PostCode>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PostCodes", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(PostCode.Code), nameof(PostCode.City));
            b.Property(x => x.Code).IsRequired().HasMaxLength(20);
            b.Property(x => x.City).IsRequired().HasMaxLength(30);
            b.Property(x => x.SearchCity).HasMaxLength(30);
            b.Property(x => x.CountryRegionCode).HasMaxLength(10);
            b.Property(x => x.County).HasMaxLength(30);
        });

        builder.Entity<ShipmentMethod>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ShipmentMethods", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<ResponsibilityCenter>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ResponsibilityCenters", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
            b.Property(x => x.Address).HasMaxLength(100);
            b.Property(x => x.Address2).HasMaxLength(50);
            b.Property(x => x.City).HasMaxLength(30);
            b.Property(x => x.PostCode).HasMaxLength(20);
            b.Property(x => x.CountryRegionCode).HasMaxLength(10);
            b.Property(x => x.PhoneNo).HasMaxLength(30);
            b.Property(x => x.FaxNo).HasMaxLength(30);
            b.Property(x => x.Name2).HasMaxLength(50);
            b.Property(x => x.Contact).HasMaxLength(100);
            b.Property(x => x.GlobalDimension1Code).HasMaxLength(20);
            b.Property(x => x.GlobalDimension2Code).HasMaxLength(20);
            b.Property(x => x.LocationCode).HasMaxLength(10);
            b.Property(x => x.County).HasMaxLength(30);
            b.Property(x => x.Email).HasMaxLength(80);
        });

        builder.Entity<UserSetup>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "UserSetups", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(UserSetup.UserId));
            b.Property(x => x.UserId).IsRequired().HasMaxLength(50);
            b.Property(x => x.SalespersPurchCode).HasMaxLength(20);
            b.Property(x => x.ApproverId).HasMaxLength(50);
            b.Property(x => x.Substitute).HasMaxLength(50);
            b.Property(x => x.Email).HasMaxLength(100);
            b.Property(x => x.PhoneNo).HasMaxLength(30);
            b.Property(x => x.SalesRespCtrFilter).HasMaxLength(10);
            b.Property(x => x.PurchaseRespCtrFilter).HasMaxLength(10);
        });

        builder.Entity<GLBudgetName>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "GLBudgetNames", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(GLBudgetName.Name));
            b.Property(x => x.Name).IsRequired().HasMaxLength(10);
            b.Property(x => x.Description).HasMaxLength(80);
            b.Property(x => x.BudgetDimension1Code).HasMaxLength(20);
            b.Property(x => x.BudgetDimension2Code).HasMaxLength(20);
            b.Property(x => x.BudgetDimension3Code).HasMaxLength(20);
            b.Property(x => x.BudgetDimension4Code).HasMaxLength(20);
        });

        builder.Entity<GLBudgetEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "GLBudgetEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(GLBudgetEntry.EntryNo));
            b.Property(x => x.BudgetName).IsRequired().HasMaxLength(10);
            b.Property(x => x.GLAccountNo).IsRequired().HasMaxLength(20);
            b.Property(x => x.GlobalDimension1Code).HasMaxLength(20);
            b.Property(x => x.GlobalDimension2Code).HasMaxLength(20);
            b.Property(x => x.Description).HasMaxLength(100);
            b.Property(x => x.BusinessUnitCode).HasMaxLength(20);
            b.Property(x => x.BudgetDimension1Code).HasMaxLength(20);
            b.Property(x => x.BudgetDimension2Code).HasMaxLength(20);
            b.Property(x => x.BudgetDimension3Code).HasMaxLength(20);
            b.Property(x => x.BudgetDimension4Code).HasMaxLength(20);
        });

        builder.Entity<CommentLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "CommentLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(CommentLine.TableName), nameof(CommentLine.No), nameof(CommentLine.LineNo));
            b.Property(x => x.No).IsRequired().HasMaxLength(20);
            b.Property(x => x.Code).HasMaxLength(10);
            b.Property(x => x.Comment).HasMaxLength(80);
        });

        builder.Entity<CustomerBankAccount>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "CustomerBankAccounts", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(CustomerBankAccount.CustomerNo), nameof(CustomerBankAccount.Code));
            b.Property(x => x.CustomerNo).IsRequired().HasMaxLength(20);
            b.Property(x => x.Code).IsRequired().HasMaxLength(20);
            b.Property(x => x.Name).HasMaxLength(100);
            b.Property(x => x.Name2).HasMaxLength(50);
            b.Property(x => x.Address).HasMaxLength(100);
            b.Property(x => x.Address2).HasMaxLength(50);
            b.Property(x => x.City).HasMaxLength(30);
            b.Property(x => x.PostCode).HasMaxLength(20);
            b.Property(x => x.Contact).HasMaxLength(100);
            b.Property(x => x.PhoneNo).HasMaxLength(30);
            b.Property(x => x.BankBranchNo).HasMaxLength(20);
            b.Property(x => x.BankAccountNo).HasMaxLength(30);
            b.Property(x => x.TransitNo).HasMaxLength(20);
            b.Property(x => x.CurrencyCode).HasMaxLength(10);
            b.Property(x => x.CountryRegionCode).HasMaxLength(10);
            b.Property(x => x.County).HasMaxLength(30);
            b.Property(x => x.FaxNo).HasMaxLength(30);
            b.Property(x => x.Email).HasMaxLength(80);
            b.Property(x => x.Iban).HasMaxLength(50);
            b.Property(x => x.SwiftCode).HasMaxLength(20);
            b.Property(x => x.BankClearingCode).HasMaxLength(50);
            b.Property(x => x.BankClearingStandard).HasMaxLength(50);
        });

        builder.Entity<DetailedCustLedgEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "DetailedCustLedgEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => x.CustomerNo);
            b.Property(x => x.DocumentNo).HasMaxLength(20);
            b.Property(x => x.CustomerNo).HasMaxLength(20);
            b.Property(x => x.CurrencyCode).HasMaxLength(10);
            b.Property(x => x.UserId).HasMaxLength(50);
            b.Property(x => x.SourceCode).HasMaxLength(10);
            b.Property(x => x.JournalBatchName).HasMaxLength(10);
            b.Property(x => x.ReasonCode).HasMaxLength(10);
            b.Property(x => x.InitialEntryGlobalDim1).HasMaxLength(20);
            b.Property(x => x.InitialEntryGlobalDim2).HasMaxLength(20);
            b.Property(x => x.GenBusPostingGroup).HasMaxLength(20);
            b.Property(x => x.GenProdPostingGroup).HasMaxLength(20);
            b.Property(x => x.VatBusPostingGroup).HasMaxLength(20);
            b.Property(x => x.VatProdPostingGroup).HasMaxLength(20);
            b.Property(x => x.PostingGroup).HasMaxLength(20);
        });

        builder.Entity<VendorBankAccount>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "VendorBankAccounts", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(VendorBankAccount.VendorNo), nameof(VendorBankAccount.Code));
            b.Property(x => x.VendorNo).IsRequired().HasMaxLength(20);
            b.Property(x => x.Code).IsRequired().HasMaxLength(20);
            b.Property(x => x.Name).HasMaxLength(100);
            b.Property(x => x.Name2).HasMaxLength(50);
            b.Property(x => x.Address).HasMaxLength(100);
            b.Property(x => x.Address2).HasMaxLength(50);
            b.Property(x => x.City).HasMaxLength(30);
            b.Property(x => x.PostCode).HasMaxLength(20);
            b.Property(x => x.Contact).HasMaxLength(100);
            b.Property(x => x.PhoneNo).HasMaxLength(30);
            b.Property(x => x.BankBranchNo).HasMaxLength(20);
            b.Property(x => x.BankAccountNo).HasMaxLength(30);
            b.Property(x => x.TransitNo).HasMaxLength(20);
            b.Property(x => x.CurrencyCode).HasMaxLength(10);
            b.Property(x => x.CountryRegionCode).HasMaxLength(10);
            b.Property(x => x.County).HasMaxLength(30);
            b.Property(x => x.FaxNo).HasMaxLength(30);
            b.Property(x => x.Email).HasMaxLength(80);
            b.Property(x => x.Iban).HasMaxLength(50);
            b.Property(x => x.SwiftCode).HasMaxLength(20);
            b.Property(x => x.BankClearingCode).HasMaxLength(50);
            b.Property(x => x.BankClearingStandard).HasMaxLength(50);
        });

        builder.Entity<DetailedVendorLedgEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "DetailedVendorLedgEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => x.VendorNo);
            b.Property(x => x.DocumentNo).HasMaxLength(20);
            b.Property(x => x.VendorNo).HasMaxLength(20);
            b.Property(x => x.CurrencyCode).HasMaxLength(10);
            b.Property(x => x.UserId).HasMaxLength(50);
            b.Property(x => x.SourceCode).HasMaxLength(10);
            b.Property(x => x.JournalBatchName).HasMaxLength(10);
            b.Property(x => x.ReasonCode).HasMaxLength(10);
            b.Property(x => x.InitialEntryGlobalDim1).HasMaxLength(20);
            b.Property(x => x.InitialEntryGlobalDim2).HasMaxLength(20);
            b.Property(x => x.GenBusPostingGroup).HasMaxLength(20);
            b.Property(x => x.GenProdPostingGroup).HasMaxLength(20);
            b.Property(x => x.VatBusPostingGroup).HasMaxLength(20);
            b.Property(x => x.VatProdPostingGroup).HasMaxLength(20);
            b.Property(x => x.PostingGroup).HasMaxLength(20);
        });

        builder.Entity<OrderAddress>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "OrderAddresses", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(OrderAddress.VendorNo), nameof(OrderAddress.Code));
            b.Property(x => x.VendorNo).IsRequired().HasMaxLength(20);
            b.Property(x => x.Code).IsRequired().HasMaxLength(10);
            b.Property(x => x.Name).HasMaxLength(100);
            b.Property(x => x.Name2).HasMaxLength(50);
            b.Property(x => x.Address).HasMaxLength(100);
            b.Property(x => x.Address2).HasMaxLength(50);
            b.Property(x => x.City).HasMaxLength(30);
            b.Property(x => x.Contact).HasMaxLength(100);
            b.Property(x => x.PhoneNo).HasMaxLength(30);
            b.Property(x => x.CountryRegionCode).HasMaxLength(10);
            b.Property(x => x.FaxNo).HasMaxLength(30);
            b.Property(x => x.PostCode).HasMaxLength(20);
            b.Property(x => x.County).HasMaxLength(30);
            b.Property(x => x.Email).HasMaxLength(80);
        });

        builder.Entity<ItemVendor>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ItemVendors", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(ItemVendor.VendorNo), nameof(ItemVendor.ItemNo));
            b.Property(x => x.ItemNo).IsRequired().HasMaxLength(20);
            b.Property(x => x.VendorNo).IsRequired().HasMaxLength(20);
            b.Property(x => x.LeadTimeCalculation).HasMaxLength(32);
            b.Property(x => x.VendorItemNo).HasMaxLength(50);
        });

        builder.Entity<PurchCommentLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PurchCommentLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(PurchCommentLine.DocumentType), nameof(PurchCommentLine.No), nameof(PurchCommentLine.DocumentLineNo), nameof(PurchCommentLine.LineNo));
            b.Property(x => x.No).IsRequired().HasMaxLength(20);
            b.Property(x => x.Code).HasMaxLength(10);
            b.Property(x => x.Comment).HasMaxLength(80);
        });

        builder.Entity<BankAccReconciliation>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "BankAccReconciliations", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(BankAccReconciliation.StatementType), nameof(BankAccReconciliation.BankAccountNo), nameof(BankAccReconciliation.StatementNo));
            b.Property(x => x.BankAccountNo).IsRequired().HasMaxLength(20);
            b.Property(x => x.StatementNo).IsRequired().HasMaxLength(20);
            b.Property(x => x.ShortcutDimension1Code).HasMaxLength(20);
            b.Property(x => x.ShortcutDimension2Code).HasMaxLength(20);
        });

        builder.Entity<BankAccReconciliationLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "BankAccReconciliationLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(BankAccReconciliationLine.StatementType), nameof(BankAccReconciliationLine.BankAccountNo), nameof(BankAccReconciliationLine.StatementNo), nameof(BankAccReconciliationLine.StatementLineNo));
            b.Property(x => x.BankAccountNo).IsRequired().HasMaxLength(20);
            b.Property(x => x.StatementNo).IsRequired().HasMaxLength(20);
            b.Property(x => x.DocumentNo).HasMaxLength(20);
            b.Property(x => x.Description).HasMaxLength(100);
            b.Property(x => x.CheckNo).HasMaxLength(20);
            b.Property(x => x.RelatedPartyName).HasMaxLength(250);
            b.Property(x => x.AdditionalTransactionInfo).HasMaxLength(100);
            b.Property(x => x.AccountNo).HasMaxLength(20);
            b.Property(x => x.TransactionText).HasMaxLength(140);
            b.Property(x => x.RelatedPartyBankAccNo).HasMaxLength(100);
            b.Property(x => x.RelatedPartyAddress).HasMaxLength(100);
            b.Property(x => x.RelatedPartyCity).HasMaxLength(50);
            b.Property(x => x.PaymentReferenceNo).HasMaxLength(50);
            b.Property(x => x.ShortcutDimension1Code).HasMaxLength(20);
            b.Property(x => x.ShortcutDimension2Code).HasMaxLength(20);
            b.Property(x => x.TransactionId).HasMaxLength(50);
        });

        builder.Entity<BankAccountStatement>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "BankAccountStatements", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => x.BankAccountNo);
            b.Property(x => x.BankAccountNo).HasMaxLength(20);
            b.Property(x => x.StatementNo).HasMaxLength(20);
        });

        builder.Entity<BankAccountStatementLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "BankAccountStatementLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => x.BankAccountNo);
            b.Property(x => x.BankAccountNo).HasMaxLength(20);
            b.Property(x => x.StatementNo).HasMaxLength(20);
            b.Property(x => x.DocumentNo).HasMaxLength(20);
            b.Property(x => x.Description).HasMaxLength(100);
            b.Property(x => x.CheckNo).HasMaxLength(20);
            b.Property(x => x.TransactionId).HasMaxLength(50);
        });

        builder.Entity<CheckLedgerEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "CheckLedgerEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => x.BankAccountNo);
            b.Property(x => x.BankAccountNo).HasMaxLength(20);
            b.Property(x => x.DocumentNo).HasMaxLength(20);
            b.Property(x => x.Description).HasMaxLength(100);
            b.Property(x => x.CheckNo).HasMaxLength(20);
            b.Property(x => x.BalAccountNo).HasMaxLength(20);
            b.Property(x => x.StatementNo).HasMaxLength(20);
            b.Property(x => x.UserId).HasMaxLength(50);
            b.Property(x => x.ExternalDocumentNo).HasMaxLength(35);
        });

        builder.Entity<Relative>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "Relatives", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<MiscArticle>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "MiscArticles", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<Confidential>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "Confidentials", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<EmployeeStatisticsGroup>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "EmployeeStatisticsGroups", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<EmployeeRelative>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "EmployeeRelatives", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(EmployeeRelative.EmployeeNo), nameof(EmployeeRelative.LineNo));
            b.Property(x => x.EmployeeNo).IsRequired().HasMaxLength(20);
            b.Property(x => x.RelativeCode).HasMaxLength(10);
            b.Property(x => x.FirstName).HasMaxLength(30);
            b.Property(x => x.MiddleName).HasMaxLength(30);
            b.Property(x => x.LastName).HasMaxLength(30);
            b.Property(x => x.PhoneNo).HasMaxLength(30);
            b.Property(x => x.RelativesEmployeeNo).HasMaxLength(20);
        });

        builder.Entity<EmployeeQualification>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "EmployeeQualifications", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(EmployeeQualification.EmployeeNo), nameof(EmployeeQualification.LineNo));
            b.Property(x => x.EmployeeNo).IsRequired().HasMaxLength(20);
            b.Property(x => x.QualificationCode).HasMaxLength(10);
            b.Property(x => x.Description).HasMaxLength(100);
            b.Property(x => x.InstitutionCompany).HasMaxLength(100);
            b.Property(x => x.CourseGrade).HasMaxLength(50);
        });

        builder.Entity<MiscArticleInformation>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "MiscArticleInformations", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(MiscArticleInformation.EmployeeNo), nameof(MiscArticleInformation.MiscArticleCode), nameof(MiscArticleInformation.LineNo));
            b.Property(x => x.EmployeeNo).IsRequired().HasMaxLength(20);
            b.Property(x => x.MiscArticleCode).IsRequired().HasMaxLength(10);
            b.Property(x => x.Description).HasMaxLength(100);
            b.Property(x => x.SerialNo).HasMaxLength(50);
        });

        builder.Entity<ConfidentialInformation>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ConfidentialInformations", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(ConfidentialInformation.EmployeeNo), nameof(ConfidentialInformation.ConfidentialCode), nameof(ConfidentialInformation.LineNo));
            b.Property(x => x.EmployeeNo).IsRequired().HasMaxLength(20);
            b.Property(x => x.ConfidentialCode).IsRequired().HasMaxLength(10);
            b.Property(x => x.Description).HasMaxLength(100);
        });

        builder.Entity<AlternativeAddress>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "AlternativeAddresses", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(AlternativeAddress.EmployeeNo), nameof(AlternativeAddress.Code));
            b.Property(x => x.EmployeeNo).IsRequired().HasMaxLength(20);
            b.Property(x => x.Code).IsRequired().HasMaxLength(10);
            b.Property(x => x.Name).HasMaxLength(100);
            b.Property(x => x.Name2).HasMaxLength(50);
            b.Property(x => x.Address).HasMaxLength(100);
            b.Property(x => x.Address2).HasMaxLength(50);
            b.Property(x => x.City).HasMaxLength(30);
            b.Property(x => x.PostCode).HasMaxLength(20);
            b.Property(x => x.County).HasMaxLength(30);
            b.Property(x => x.PhoneNo).HasMaxLength(30);
            b.Property(x => x.FaxNo).HasMaxLength(30);
            b.Property(x => x.Email).HasMaxLength(80);
            b.Property(x => x.CountryRegionCode).HasMaxLength(10);
        });

        builder.Entity<HumanResourceCommentLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "HumanResourceCommentLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(HumanResourceCommentLine.TableName), nameof(HumanResourceCommentLine.No), nameof(HumanResourceCommentLine.TableLineNo), nameof(HumanResourceCommentLine.LineNo));
            b.Property(x => x.No).IsRequired().HasMaxLength(20);
            b.Property(x => x.AlternativeAddressCode).HasMaxLength(10);
            b.Property(x => x.Code).HasMaxLength(10);
            b.Property(x => x.Comment).HasMaxLength(80);
        });

        builder.Entity<FAClass>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "FAClasses", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<FASubclass>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "FASubclasses", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
            b.Property(x => x.FAClassCode).HasMaxLength(10);
            b.Property(x => x.DefaultFAPostingGroup).HasMaxLength(20);
        });

        builder.Entity<FALocation>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "FALocations", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<Maintenance>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "Maintenances", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<DepreciationBook>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "DepreciationBooks", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<FAPostingGroup>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "FAPostingGroups", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
            b.Property(x => x.AcquisitionCostAccount).HasMaxLength(20);
            b.Property(x => x.AccumDepreciationAccount).HasMaxLength(20);
            b.Property(x => x.WriteDownAccount).HasMaxLength(20);
            b.Property(x => x.AppreciationAccount).HasMaxLength(20);
            b.Property(x => x.AcqCostAccOnDisposal).HasMaxLength(20);
            b.Property(x => x.AccumDeprAccOnDisposal).HasMaxLength(20);
            b.Property(x => x.WriteDownAccOnDisposal).HasMaxLength(20);
            b.Property(x => x.AppreciationAccOnDisposal).HasMaxLength(20);
            b.Property(x => x.GainsAccOnDisposal).HasMaxLength(20);
            b.Property(x => x.LossesAccOnDisposal).HasMaxLength(20);
            b.Property(x => x.BookValAccOnDispGain).HasMaxLength(20);
            b.Property(x => x.SalesAccOnDispGain).HasMaxLength(20);
            b.Property(x => x.WriteDownBalAccOnDisp).HasMaxLength(20);
            b.Property(x => x.ApprecBalAccOnDisp).HasMaxLength(20);
            b.Property(x => x.MaintenanceExpenseAccount).HasMaxLength(20);
            b.Property(x => x.MaintenanceBalAcc).HasMaxLength(20);
            b.Property(x => x.AcquisitionCostBalAcc).HasMaxLength(20);
            b.Property(x => x.DepreciationExpenseAcc).HasMaxLength(20);
            b.Property(x => x.WriteDownExpenseAcc).HasMaxLength(20);
            b.Property(x => x.AppreciationBalAccount).HasMaxLength(20);
            b.Property(x => x.SalesBalAcc).HasMaxLength(20);
            b.Property(x => x.SalesAccOnDispLoss).HasMaxLength(20);
            b.Property(x => x.BookValAccOnDispLoss).HasMaxLength(20);
        });

        builder.Entity<FASetup>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "FASetups", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            // One setup row per company.
            b.HasCompanyUniqueIndex();
            b.Property(x => x.DefaultDeprBook).HasMaxLength(10);
            b.Property(x => x.FixedAssetNos).HasMaxLength(20);
        });

        builder.Entity<FixedAsset>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "FixedAssets", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(FixedAsset.No));
            b.Property(x => x.No).IsRequired().HasMaxLength(20);
            b.Property(x => x.Description).IsRequired().HasMaxLength(100);
            b.Property(x => x.SearchDescription).HasMaxLength(100);
            b.Property(x => x.Description2).HasMaxLength(50);
            b.Property(x => x.FAClassCode).HasMaxLength(10);
            b.Property(x => x.FASubclassCode).HasMaxLength(10);
            b.Property(x => x.GlobalDimension1Code).HasMaxLength(20);
            b.Property(x => x.GlobalDimension2Code).HasMaxLength(20);
            b.Property(x => x.LocationCode).HasMaxLength(10);
            b.Property(x => x.FALocationCode).HasMaxLength(10);
            b.Property(x => x.VendorNo).HasMaxLength(20);
            b.Property(x => x.ComponentOfMainAsset).HasMaxLength(20);
            b.Property(x => x.ResponsibleEmployee).HasMaxLength(20);
            b.Property(x => x.SerialNo).HasMaxLength(50);
            b.Property(x => x.MaintenanceVendorNo).HasMaxLength(20);
            b.Property(x => x.FAPostingGroup).HasMaxLength(20);
        });

        builder.Entity<FADepreciationBook>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "FADepreciationBooks", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(FADepreciationBook.FANo), nameof(FADepreciationBook.DepreciationBookCode));
            b.Property(x => x.FANo).IsRequired().HasMaxLength(20);
            b.Property(x => x.DepreciationBookCode).IsRequired().HasMaxLength(10);
            b.Property(x => x.FAPostingGroup).HasMaxLength(20);
        });

        builder.Entity<FALedgerEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "FALedgerEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => x.FANo);
            b.Property(x => x.FANo).HasMaxLength(20);
            b.Property(x => x.DocumentNo).HasMaxLength(20);
            b.Property(x => x.ExternalDocumentNo).HasMaxLength(35);
            b.Property(x => x.Description).HasMaxLength(100);
            b.Property(x => x.DepreciationBookCode).HasMaxLength(10);
            b.Property(x => x.FASubclassCode).HasMaxLength(10);
            b.Property(x => x.FALocationCode).HasMaxLength(10);
            b.Property(x => x.FAPostingGroup).HasMaxLength(20);
            b.Property(x => x.GlobalDimension1Code).HasMaxLength(20);
            b.Property(x => x.GlobalDimension2Code).HasMaxLength(20);
            b.Property(x => x.LocationCode).HasMaxLength(10);
            b.Property(x => x.UserId).HasMaxLength(50);
            b.Property(x => x.JournalBatchName).HasMaxLength(10);
            b.Property(x => x.SourceCode).HasMaxLength(10);
            b.Property(x => x.ReasonCode).HasMaxLength(10);
            b.Property(x => x.BalAccountNo).HasMaxLength(20);
            b.Property(x => x.FAClassCode).HasMaxLength(10);
        });

        builder.Entity<MaintenanceRegistration>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "MaintenanceRegistrations", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(MaintenanceRegistration.FANo), nameof(MaintenanceRegistration.LineNo));
            b.Property(x => x.FANo).IsRequired().HasMaxLength(20);
            b.Property(x => x.MaintenanceVendorNo).HasMaxLength(20);
            b.Property(x => x.Comment).HasMaxLength(50);
            b.Property(x => x.ServiceAgentName).HasMaxLength(30);
            b.Property(x => x.ServiceAgentPhoneNo).HasMaxLength(30);
            b.Property(x => x.ServiceAgentMobilePhone).HasMaxLength(30);
        });

        builder.Entity<MainAssetComponent>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "MainAssetComponents", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(MainAssetComponent.MainAssetNo), nameof(MainAssetComponent.FANo));
            b.Property(x => x.MainAssetNo).IsRequired().HasMaxLength(20);
            b.Property(x => x.FANo).IsRequired().HasMaxLength(20);
            b.Property(x => x.Description).HasMaxLength(100);
        });

        builder.Entity<ReportSelection>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ReportSelections", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(ReportSelection.Usage), nameof(ReportSelection.Sequence));
            b.Property(x => x.Sequence).IsRequired().HasMaxLength(10);
            b.Property(x => x.CustomReportLayoutCode).HasMaxLength(20);
            b.Property(x => x.EmailBodyLayoutCode).HasMaxLength(20);
            b.Property(x => x.ReportLayoutName).HasMaxLength(250);
        });

        builder.Entity<CustomReportSelection>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "CustomReportSelections", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(CustomReportSelection.SourceType), nameof(CustomReportSelection.SourceNo), nameof(CustomReportSelection.Usage), nameof(CustomReportSelection.Sequence));
            b.Property(x => x.SourceNo).IsRequired().HasMaxLength(20);
            b.Property(x => x.CustomReportLayoutCode).HasMaxLength(20);
            b.Property(x => x.SendToEmail).HasMaxLength(200);
            b.Property(x => x.EmailBodyLayoutCode).HasMaxLength(20);
        });

        builder.Entity<WorkflowUserGroup>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "WorkflowUserGroups", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<WorkflowUserGroupMember>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "WorkflowUserGroupMembers", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(WorkflowUserGroupMember.WorkflowUserGroupCode), nameof(WorkflowUserGroupMember.UserName));
            b.Property(x => x.WorkflowUserGroupCode).IsRequired().HasMaxLength(20);
            b.Property(x => x.UserName).IsRequired().HasMaxLength(50);
        });

        builder.Entity<ApprovalCommentLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ApprovalCommentLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(ApprovalCommentLine.EntryNo));
            b.Property(x => x.DocumentNo).HasMaxLength(20);
            b.Property(x => x.UserId).HasMaxLength(50);
            b.Property(x => x.Comment).HasMaxLength(80);
        });
    }
}
