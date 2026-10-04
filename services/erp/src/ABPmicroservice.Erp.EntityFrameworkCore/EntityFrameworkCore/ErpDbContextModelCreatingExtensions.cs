using ABPmicroservice.Erp.Automations;
using ABPmicroservice.Erp.Chatter;
using ABPmicroservice.Erp.CashManagement;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Dimensions;
using ABPmicroservice.Erp.Exporting;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Integration;
using ABPmicroservice.Erp.HumanResources;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.JobQueue;
using ABPmicroservice.Erp.Kanban;
using ABPmicroservice.Erp.Modules;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Profiles;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.RapidStart;
using ABPmicroservice.Erp.Reporting;
using ABPmicroservice.Erp.Sales;
using ABPmicroservice.Erp.Sequences;
using ABPmicroservice.Erp.Workflows;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

public static class ErpDbContextModelCreatingExtensions
{
    public static void ConfigureErp(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<GLAccount>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "GLAccounts", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Name).IsRequired().HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.SearchName).HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.Subcategory).HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.Totaling).HasMaxLength(ErpDomainConsts.MaxTotalingLength);
            b.Property(x => x.GenBusPostingGroup).HasMaxLength(ErpDomainConsts.MaxPostingGroupLength);
            b.Property(x => x.GenProdPostingGroup).HasMaxLength(ErpDomainConsts.MaxPostingGroupLength);
            b.Property(x => x.VatBusPostingGroup).HasMaxLength(ErpDomainConsts.MaxPostingGroupLength);
            b.Property(x => x.VatProdPostingGroup).HasMaxLength(ErpDomainConsts.MaxPostingGroupLength);
            b.Property(x => x.TaxAreaCode).HasMaxLength(ErpDomainConsts.MaxTaxAreaCodeLength);
            b.Property(x => x.TaxGroupCode).HasMaxLength(ErpDomainConsts.MaxTaxGroupCodeLength);
            b.Property(x => x.ConsolDebitAcc).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.ConsolCreditAcc).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.CostTypeNo).HasMaxLength(ErpDomainConsts.MaxCostTypeLength);
            b.Property(x => x.DefaultDeferralTemplateCode).HasMaxLength(ErpDomainConsts.MaxDeferralTemplateCodeLength);
        });

        builder.Entity<GLEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "GLEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.GLAccountNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.GLAccountName).HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.DocumentNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.SourceNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.GenBusPostingGroup).HasMaxLength(ErpDomainConsts.MaxGeneralBusPostingGroupLength);
            b.Property(x => x.GenProdPostingGroup).HasMaxLength(ErpDomainConsts.MaxPostingGroupLength);
            b.Property(x => x.BalAccountNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.VATBusPostingGroup).HasMaxLength(ErpDomainConsts.MaxPostingGroupLength);
            b.Property(x => x.VATProdPostingGroup).HasMaxLength(ErpDomainConsts.MaxPostingGroupLength);
            b.Property(x => x.ExternalDocumentNo).HasMaxLength(ErpDomainConsts.MaxExternalDocumentNoLength);
            b.Property(x => x.UserId).HasMaxLength(ErpDomainConsts.MaxUserNameLength);
            b.Property(x => x.JournalBatchName).HasMaxLength(ErpDomainConsts.MaxJournalTemplateNameLength);
            b.Property(x => x.JobNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.BusinessUnitCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.SourceCode).HasMaxLength(ErpDomainConsts.MaxSourceCodeLength);
            b.Property(x => x.ReasonCode).HasMaxLength(ErpDomainConsts.MaxReasonCodeLength);
        });

        builder.Entity<GeneralPostingSetup>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "GeneralPostingSetups", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(GeneralPostingSetup.GenBusPostingGroup), nameof(GeneralPostingSetup.GenProdPostingGroup));
        });

        builder.Entity<GeneralLedgerSetup>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "GeneralLedgerSetups", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            // One setup row per company.
            b.HasCompanyUniqueIndex();
            b.Property(x => x.LcyCode).HasMaxLength(ErpDomainConsts.MaxCurrencyCodeLength);
            b.Property(x => x.LocalCurrencySymbol).HasMaxLength(ErpDomainConsts.MaxCurrencySymbolLength);
            b.Property(x => x.LocalCurrencyDescription).HasMaxLength(ErpDomainConsts.MaxCurrencyDescriptionLength);
            b.Property(x => x.AdditionalReportingCurrency).HasMaxLength(ErpDomainConsts.MaxCurrencyCodeLength);
            b.Property(x => x.JobQueueCategoryCode).HasMaxLength(ErpDomainConsts.MaxJobCategoryCodeLength);
            b.Property(x => x.GlobalDimension1Code).HasMaxLength(ErpDomainConsts.MaxDimensionCodeLength);
            b.Property(x => x.GlobalDimension2Code).HasMaxLength(ErpDomainConsts.MaxDimensionCodeLength);
            b.Property(x => x.BankAccountNos).HasMaxLength(ErpDomainConsts.MaxNoSeriesCodeLength);
        });

        builder.Entity<GenBusinessPostingGroup>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "GenBusinessPostingGroups", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<GenProductPostingGroup>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "GenProductPostingGroups", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<GenJournalBatch>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "GenJournalBatches", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<GenJournalLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "GenJournalLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.CurrencyFactor).HasPrecision(28, 15);
        });

        builder.Entity<Customer>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "Customers", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
        });

        builder.Entity<CustomerPostingGroup>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "CustomerPostingGroups", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<CustomerLedgerEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "CustomerLedgerEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<SalesHeader>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "SalesHeaders", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.SalesHeaderId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<SalesLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "SalesLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<PostedSalesHeader>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PostedSalesHeaders", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.PostedSalesHeaderId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<PostedSalesLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PostedSalesLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<Vendor>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "Vendors", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Name).IsRequired().HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.SearchName).HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.Name2).HasMaxLength(ErpDomainConsts.MaxCityLength);
            b.Property(x => x.Address).HasMaxLength(ErpDomainConsts.MaxAddressLength);
            b.Property(x => x.Address2).HasMaxLength(ErpDomainConsts.MaxCityLength);
            b.Property(x => x.City).HasMaxLength(ErpDomainConsts.MaxCityLength);
            b.Property(x => x.PostCode).HasMaxLength(ErpDomainConsts.MaxPostCodeLength);
            b.Property(x => x.CountryRegionCode).HasMaxLength(ErpDomainConsts.MaxCountryRegionCodeLength);
            b.Property(x => x.PhoneNo).HasMaxLength(ErpDomainConsts.MaxPhoneLength);
            b.Property(x => x.MobilePhoneNo).HasMaxLength(ErpDomainConsts.MaxPhoneLength);
            b.Property(x => x.Email).HasMaxLength(ErpDomainConsts.MaxEmailLength);
            b.Property(x => x.HomePage).HasMaxLength(ErpDomainConsts.MaxHomePageLength);
            b.Property(x => x.Contact).HasMaxLength(ErpDomainConsts.MaxContactLength);
            b.Property(x => x.OurAccountNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.VendorPostingGroup).HasMaxLength(ErpDomainConsts.MaxPostingGroupLength);
            b.Property(x => x.GenBusPostingGroup).HasMaxLength(ErpDomainConsts.MaxGeneralBusPostingGroupLength);
            b.Property(x => x.VatBusPostingGroup).HasMaxLength(ErpDomainConsts.MaxVatBusPostingGroupLength);
            b.Property(x => x.PaymentTermsCode).HasMaxLength(ErpDomainConsts.MaxPaymentTermsCodeLength);
            b.Property(x => x.CurrencyCode).HasMaxLength(ErpDomainConsts.MaxCurrencyCodeLength);
            b.Property(x => x.ShipmentMethodCode).HasMaxLength(ErpDomainConsts.MaxShipmentMethodCodeLength);
            b.Property(x => x.ShippingAgentCode).HasMaxLength(ErpDomainConsts.MaxShippingAgentCodeLength);
            b.Property(x => x.InvoiceDiscCode).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.VATRegistrationNo).HasMaxLength(ErpDomainConsts.MaxVatRegistrationNoLength);
            b.Property(x => x.TaxAreaCode).HasMaxLength(ErpDomainConsts.MaxTaxAreaCodeLength);
            b.Property(x => x.LocationCode).HasMaxLength(ErpDomainConsts.MaxLocationCodeLength);
            b.Property(x => x.LeadTimeCalculation).HasMaxLength(ErpDomainConsts.MaxDateFormulaLength);
        });

        builder.Entity<VendorPostingGroup>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "VendorPostingGroups", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<VendorLedgerEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "VendorLedgerEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.VendorNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.VendorName).HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.DocumentType).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.DocumentNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.CurrencyCode).HasMaxLength(ErpDomainConsts.MaxCurrencyCodeLength);
            b.Property(x => x.VendorPostingGroup).HasMaxLength(ErpDomainConsts.MaxPostingGroupLength);
            b.Property(x => x.GlobalDimension1Code).HasMaxLength(ErpDomainConsts.MaxDimensionCodeLength);
            b.Property(x => x.GlobalDimension2Code).HasMaxLength(ErpDomainConsts.MaxDimensionCodeLength);
            b.Property(x => x.PurchaserCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.UserId).HasMaxLength(ErpDomainConsts.MaxUserNameLength);
            b.Property(x => x.SourceCode).HasMaxLength(ErpDomainConsts.MaxSourceCodeLength);
            b.Property(x => x.OnHold).HasMaxLength(ErpDomainConsts.MaxOnHoldLength);
            b.Property(x => x.AppliesToDocType).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.AppliesToDocNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.AppliesToId).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.JournalBatchName).HasMaxLength(ErpDomainConsts.MaxJournalTemplateNameLength);
            b.Property(x => x.ExternalDocumentNo).HasMaxLength(ErpDomainConsts.MaxExternalDocumentNoLength);
            b.Property(x => x.PaymentMethodCode).HasMaxLength(ErpDomainConsts.MaxPaymentMethodCodeLength);
        });

        builder.Entity<PurchaseHeader>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PurchaseHeaders", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.PurchaseHeaderId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.BuyFromVendorNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.BuyFromVendorName).HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.PayToVendorNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.PayToName).HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.PayToAddress).HasMaxLength(ErpDomainConsts.MaxAddressLength);
            b.Property(x => x.PayToCity).HasMaxLength(ErpDomainConsts.MaxCityLength);
            b.Property(x => x.PayToPostCode).HasMaxLength(ErpDomainConsts.MaxPostCodeLength);
            b.Property(x => x.PayToCountryRegionCode).HasMaxLength(ErpDomainConsts.MaxCountryRegionCodeLength);
            b.Property(x => x.PayToContact).HasMaxLength(ErpDomainConsts.MaxContactLength);
            b.Property(x => x.ShipToCode).HasMaxLength(ErpDomainConsts.MaxLocationCodeLength);
            b.Property(x => x.ShipToName).HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.ShipToAddress).HasMaxLength(ErpDomainConsts.MaxAddressLength);
            b.Property(x => x.ShipToCity).HasMaxLength(ErpDomainConsts.MaxCityLength);
            b.Property(x => x.ShipToPostCode).HasMaxLength(ErpDomainConsts.MaxPostCodeLength);
            b.Property(x => x.ShipToCountryRegionCode).HasMaxLength(ErpDomainConsts.MaxCountryRegionCodeLength);
            b.Property(x => x.ShipToContact).HasMaxLength(ErpDomainConsts.MaxContactLength);
            b.Property(x => x.CurrencyCode).HasMaxLength(ErpDomainConsts.MaxCurrencyCodeLength);
            b.Property(x => x.PaymentTermsCode).HasMaxLength(ErpDomainConsts.MaxPaymentTermsCodeLength);
            b.Property(x => x.LocationCode).HasMaxLength(ErpDomainConsts.MaxLocationCodeLength);
            b.Property(x => x.VendorInvoiceNo).HasMaxLength(ErpDomainConsts.MaxExternalDocumentNoLength);
            b.Property(x => x.PostedDocumentNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.YourReference).HasMaxLength(ErpDomainConsts.MaxYourReferenceLength);
            b.Property(x => x.ShipmentMethodCode).HasMaxLength(ErpDomainConsts.MaxShipmentMethodCodeLength);
            b.Property(x => x.PaymentMethodCode).HasMaxLength(ErpDomainConsts.MaxPaymentMethodCodeLength);
            b.Property(x => x.ShortcutDimension1Code).HasMaxLength(ErpDomainConsts.MaxDimensionCodeLength);
            b.Property(x => x.ShortcutDimension2Code).HasMaxLength(ErpDomainConsts.MaxDimensionCodeLength);
            b.Property(x => x.VendorPostingGroup).HasMaxLength(ErpDomainConsts.MaxPostingGroupLength);
            b.Property(x => x.GenBusPostingGroup).HasMaxLength(ErpDomainConsts.MaxGeneralBusPostingGroupLength);
            b.Property(x => x.VatBusPostingGroup).HasMaxLength(ErpDomainConsts.MaxVatBusPostingGroupLength);
            b.Property(x => x.OnHold).HasMaxLength(ErpDomainConsts.MaxOnHoldLength);
            b.Property(x => x.AppliesToDocNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.AppliesToId).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.TaxAreaCode).HasMaxLength(ErpDomainConsts.MaxTaxAreaCodeLength);
        });

        builder.Entity<PurchaseLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PurchaseLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.No).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.UnitOfMeasureCode).HasMaxLength(ErpDomainConsts.MaxUnitOfMeasureCodeLength);
            b.Property(x => x.LocationCode).HasMaxLength(ErpDomainConsts.MaxLocationCodeLength);
            b.Property(x => x.ItemCategoryCode).HasMaxLength(ErpDomainConsts.MaxPostingGroupLength);
            b.Property(x => x.ShortcutDimension1Code).HasMaxLength(ErpDomainConsts.MaxDimensionCodeLength);
            b.Property(x => x.ShortcutDimension2Code).HasMaxLength(ErpDomainConsts.MaxDimensionCodeLength);
            b.Property(x => x.DeferralCode).HasMaxLength(ErpDomainConsts.MaxDeferralTemplateCodeLength);
            b.Property(x => x.TaxAreaCode).HasMaxLength(ErpDomainConsts.MaxTaxAreaCodeLength);
            b.Property(x => x.TaxGroupCode).HasMaxLength(ErpDomainConsts.MaxTaxGroupCodeLength);
            b.Property(x => x.GenBusPostingGroup).HasMaxLength(ErpDomainConsts.MaxGeneralBusPostingGroupLength);
            b.Property(x => x.GenProdPostingGroup).HasMaxLength(ErpDomainConsts.MaxPostingGroupLength);
            b.Property(x => x.VatBusPostingGroup).HasMaxLength(ErpDomainConsts.MaxVatBusPostingGroupLength);
            b.Property(x => x.VatProdPostingGroup).HasMaxLength(ErpDomainConsts.MaxPostingGroupLength);
            b.Property(x => x.VatIdentifier).HasMaxLength(ErpDomainConsts.MaxVatIdentifierLength);
        });

        builder.Entity<PostedPurchaseHeader>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PostedPurchaseHeaders", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.PostedPurchaseHeaderId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<PostedPurchaseLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PostedPurchaseLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<Item>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "Items", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
        });

        builder.Entity<ItemCategory>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ItemCategories", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<UnitOfMeasure>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "UnitsOfMeasure", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<InventoryPostingGroup>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "InventoryPostingGroups", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<InventoryPostingSetup>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "InventoryPostingSetups", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(InventoryPostingSetup.LocationCode), nameof(InventoryPostingSetup.InventoryPostingGroup));
        });

        builder.Entity<PaymentTerms>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PaymentTerms", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<Currency>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "Currencies", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<CurrencyExchangeRate>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "CurrencyExchangeRates", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(CurrencyExchangeRate.CurrencyCode), nameof(CurrencyExchangeRate.StartingDate));
        });

        builder.Entity<AccountingPeriod>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "AccountingPeriods", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(AccountingPeriod.StartingDate));
        });

        builder.Entity<VatBusinessPostingGroup>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "VatBusinessPostingGroups", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<VatProductPostingGroup>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "VatProductPostingGroups", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<VatPostingSetup>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "VatPostingSetups", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(VatPostingSetup.VatBusPostingGroup), nameof(VatPostingSetup.VatProdPostingGroup));
        });

        builder.Entity<VatEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "VatEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.EntryNo });
            b.HasIndex(x => x.RegisterNo);
        });

        builder.Entity<Location>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "Locations", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<InventorySetup>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "InventorySetups", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            // One setup row per company.
            b.HasCompanyUniqueIndex();
        });

        builder.Entity<SalespersonPurchaser>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "SalespeoplePurchasers", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<BankAccountPostingGroup>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "BankAccountPostingGroups", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<BankAccount>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "BankAccounts", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
        });

        builder.Entity<BankAccountLedgerEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "BankAccountLedgerEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.EntryNo });
            b.HasIndex(x => x.BankAccountId);
            b.HasIndex(x => x.RegisterNo);
        });

        builder.Entity<PaymentMethod>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PaymentMethods", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<HumanResourcesSetup>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "HumanResourcesSetups", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            // One setup row per company.
            b.HasCompanyUniqueIndex();
        });

        builder.Entity<HumanResourceUnitOfMeasure>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "HumanResourceUnitsOfMeasure", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<EmployeePostingGroup>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "EmployeePostingGroups", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<CauseOfAbsence>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "CausesOfAbsence", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<Qualification>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "Qualifications", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<Union>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "Unions", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<EmploymentContract>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "EmploymentContracts", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<GroundsForTermination>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "GroundsForTermination", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<Employee>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "Employees", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
        });

        builder.Entity<EmployeeAbsence>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "EmployeeAbsences", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => x.EmployeeId);
        });

        builder.Entity<EmployeeLedgerEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "EmployeeLedgerEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.EntryNo });
            b.HasIndex(x => x.EmployeeId);
            b.HasIndex(x => x.RegisterNo);
        });

        builder.Entity<ExchRateAdjmtRegister>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ExchRateAdjmtRegisters", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            // A currency factor such as 1/130 needs far more places than an amount.
            b.Property(x => x.CurrencyFactor).HasPrecision(28, 15);
            b.HasIndex(x => x.GLRegisterNo);
        });

        builder.Entity<ItemLedgerEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ItemLedgerEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<ValueEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ValueEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<ItemJournalBatch>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ItemJournalBatches", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<ItemJournalLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ItemJournalLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<Dimension>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "Dimensions", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<DimensionValue>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "DimensionValues", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<DefaultDimension>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "DefaultDimensions", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<DimensionSetEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "DimensionSetEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<ConfigPackage>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ConfigPackages", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Code).IsRequired().HasMaxLength(ErpDomainConsts.MaxPackageCodeLength);
            b.Property(x => x.PackageName).IsRequired().HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.ProductVersion).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.HasCompanyUniqueIndex("Code");
            b.HasMany(x => x.Tables).WithOne().HasForeignKey(x => x.ConfigPackageId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ConfigPackageTable>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ConfigPackageTables", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.EntityName).IsRequired().HasMaxLength(ErpDomainConsts.MaxEntityNameLength);
            b.Property(x => x.DataTemplateCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.Filters).HasMaxLength(ErpDomainConsts.MaxConfigSettingsLength);
            b.HasMany(x => x.Fields).WithOne().HasForeignKey(x => x.ConfigPackageTableId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ConfigPackageField>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ConfigPackageFields", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.FieldName).IsRequired().HasMaxLength(ErpDomainConsts.MaxEntityNameLength);
            b.Property(x => x.Mappings).HasMaxLength(ErpDomainConsts.MaxConfigSettingsLength);
        });

        builder.Entity<ConfigPackageRecord>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ConfigPackageRecords", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Values).IsRequired();
            // Staged records are always read one package table at a time, in row order.
            b.HasIndex(x => new { x.ConfigPackageId, x.ConfigPackageTableId, x.RecordNo });
        });

        builder.Entity<ConfigPackageError>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ConfigPackageErrors", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.FieldName).HasMaxLength(ErpDomainConsts.MaxEntityNameLength);
            b.Property(x => x.ErrorText).IsRequired().HasMaxLength(ErpDomainConsts.MaxConfigErrorLength);
            b.HasIndex(x => new { x.ConfigPackageId, x.ConfigPackageTableId });
            b.HasIndex(x => x.ConfigPackageRecordId);
        });

        builder.Entity<ConfigTemplate>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ConfigTemplates", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Code).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.EntityName).IsRequired().HasMaxLength(ErpDomainConsts.MaxEntityNameLength);
            b.HasCompanyUniqueIndex("Code");
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.ConfigTemplateId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ConfigTemplateLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ConfigTemplateLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.FieldName).IsRequired().HasMaxLength(ErpDomainConsts.MaxEntityNameLength);
            b.Property(x => x.DefaultValue).HasMaxLength(ErpDomainConsts.MaxConfigValueLength);
        });

        builder.Entity<ConfigLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ConfigLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.EntityName).HasMaxLength(ErpDomainConsts.MaxEntityNameLength);
            b.Property(x => x.PackageCode).HasMaxLength(ErpDomainConsts.MaxPackageCodeLength);
            b.Property(x => x.ResponsibleUserName).HasMaxLength(ErpDomainConsts.MaxUserNameLength);
            b.Property(x => x.Comments).HasMaxLength(ErpDomainConsts.MaxCommentLength);
        });

        builder.Entity<Workflow>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "Workflows", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
            b.HasMany(x => x.Steps).WithOne().HasForeignKey(x => x.WorkflowId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<WorkflowStep>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "WorkflowSteps", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<ApprovalEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ApprovalEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.SenderUserName).HasMaxLength(ErpDomainConsts.MaxUserNameLength);
            b.Property(x => x.ApproverUserName).HasMaxLength(ErpDomainConsts.MaxUserNameLength);
            b.Property(x => x.Comment).HasMaxLength(ErpDomainConsts.MaxCommentLength);
            b.Ignore(x => x.IsPending);
            // "Requests to approve" and "pending entries of a document" are the two hot lookups.
            b.HasIndex(x => new { x.ApproverId, x.Status });
            b.HasIndex(x => new { x.DocumentKind, x.DocumentId });
        });

        builder.Entity<ApprovalUserSetup>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ApprovalUserSetups", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.UserName).IsRequired().HasMaxLength(ErpDomainConsts.MaxUserNameLength);
            b.HasCompanyUniqueIndex("UserId");
        });

        builder.Entity<NoSeries>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "NoSeries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Code).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoSeriesCodeLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.NoSeriesId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<NoSeriesLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "NoSeriesLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.StartingNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.EndingNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.WarningNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            // Two transactions that read the same last number cannot both take the next one.
            b.Property(x => x.LastNoUsed).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength).IsConcurrencyToken();
        });

        builder.Entity<SalesReceivablesSetup>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "SalesReceivablesSetups", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            // One setup row per company.
            b.HasCompanyUniqueIndex();
        });

        builder.Entity<PurchasesPayablesSetup>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PurchasesPayablesSetups", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex();
            b.Property(x => x.VendorNos).HasMaxLength(ErpDomainConsts.MaxNoSeriesCodeLength);
            b.Property(x => x.OrderNos).HasMaxLength(ErpDomainConsts.MaxNoSeriesCodeLength);
            b.Property(x => x.InvoiceNos).HasMaxLength(ErpDomainConsts.MaxNoSeriesCodeLength);
            b.Property(x => x.CreditMemoNos).HasMaxLength(ErpDomainConsts.MaxNoSeriesCodeLength);
            b.Property(x => x.QuoteNos).HasMaxLength(ErpDomainConsts.MaxNoSeriesCodeLength);
            b.Property(x => x.BlanketOrderNos).HasMaxLength(ErpDomainConsts.MaxNoSeriesCodeLength);
            b.Property(x => x.PostedReceiptNos).HasMaxLength(ErpDomainConsts.MaxNoSeriesCodeLength);
            b.Property(x => x.PostedReturnShptNos).HasMaxLength(ErpDomainConsts.MaxNoSeriesCodeLength);
            b.Property(x => x.ReturnOrderNos).HasMaxLength(ErpDomainConsts.MaxNoSeriesCodeLength);
            b.Property(x => x.JobQueueCategoryCode).HasMaxLength(ErpDomainConsts.MaxJobCategoryCodeLength);
        });

        builder.Entity<UserProfile>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "UserProfiles", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ProfileId).IsRequired().HasMaxLength(ErpDomainConsts.MaxProfileIdLength);
            b.HasIndex(x => new { x.TenantId, x.UserId });
        });

        builder.Entity<ProfileRoleAssignment>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ProfileRoleAssignments", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.RoleName).IsRequired().HasMaxLength(ErpDomainConsts.MaxUserNameLength);
            b.Property(x => x.ProfileId).IsRequired().HasMaxLength(ErpDomainConsts.MaxProfileIdLength);
            b.HasIndex(x => new { x.TenantId, x.RoleName }).IsUnique();
        });

        builder.Entity<UserRoleCenter>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "UserRoleCenters", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<AccountSchedule>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "AccountSchedules", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.AccountScheduleId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AccountScheduleLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "AccountScheduleLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<ColumnLayout>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ColumnLayouts", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.ColumnLayoutId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            b.HasCompanyUniqueIndex("Name");
        });

        builder.Entity<ColumnLayoutLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ColumnLayoutLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ColumnNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxRowNoLength);
            b.Property(x => x.ColumnHeader).HasMaxLength(ErpDomainConsts.MaxColumnHeaderLength);
            b.Property(x => x.ComparisonDateFormula).HasMaxLength(ErpDomainConsts.MaxDateFormulaLength);
        });

        builder.Entity<GenJournalTemplate>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "GenJournalTemplates", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(ErpDomainConsts.MaxJournalTemplateNameLength);
            b.Property(x => x.SourceCode).HasMaxLength(ErpDomainConsts.MaxSourceCodeLength);
            b.Property(x => x.NoSeriesCode).HasMaxLength(ErpDomainConsts.MaxNoSeriesCodeLength);
            b.HasCompanyUniqueIndex("Name");
        });

        builder.Entity<GLRegister>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "GLRegisters", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.SourceCode).HasMaxLength(ErpDomainConsts.MaxSourceCodeLength);
            b.Property(x => x.JournalBatchName).HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.UserName).HasMaxLength(ErpDomainConsts.MaxUserNameLength);
            b.Ignore(x => x.IsEmpty);
            b.Ignore(x => x.IsReversible);
            b.HasCompanyUniqueIndex("No");
        });

        builder.Entity<StandardGeneralJournal>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "StandardGeneralJournals", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.JournalTemplateName).IsRequired().HasMaxLength(ErpDomainConsts.MaxJournalTemplateNameLength);
            b.Property(x => x.Code).IsRequired().HasMaxLength(ErpDomainConsts.MaxJournalTemplateNameLength);
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.StandardGeneralJournalId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            b.HasCompanyUniqueIndex("JournalTemplateName", "Code");
        });

        builder.Entity<StandardGeneralJournalLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "StandardGeneralJournalLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.AccountNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
        });

        builder.Entity<ErpNumberSequence>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "NumberSequences", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.HasCompanyUniqueIndex("Name");
        });

        builder.Entity<PublishedWebService>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PublishedWebServices", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ServiceName).IsRequired().HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.EntityName).IsRequired().HasMaxLength(ErpDomainConsts.MaxEntityNameLength);
            b.Property(x => x.ExcludedFields).HasMaxLength(ErpDomainConsts.MaxTotalingLength);
            b.HasCompanyUniqueIndex("ServiceName");
        });

        builder.Entity<WebhookSubscription>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "WebhookSubscriptions", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.EntityName).IsRequired().HasMaxLength(ErpDomainConsts.MaxEntityNameLength);
            b.Property(x => x.EndpointUrl).IsRequired().HasMaxLength(ErpDomainConsts.MaxUrlLength);
            b.Property(x => x.Secret).HasMaxLength(ErpDomainConsts.MaxWebhookSecretLength);
            b.Property(x => x.LastError).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.HasIndex(x => new { x.EntityName, x.Active });
        });

        builder.Entity<WebhookDelivery>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "WebhookDeliveries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.EntityName).IsRequired().HasMaxLength(ErpDomainConsts.MaxEntityNameLength);
            b.Property(x => x.Error).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            // The worker asks for exactly this: what is due, oldest first.
            b.HasIndex(x => new { x.Status, x.NextAttemptTime });
            b.HasIndex(x => x.SubscriptionId);
        });

        builder.Entity<ErpModuleState>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ModuleStates", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ModuleCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);

            // A module stands at one state in a company, which is what the lookup assumes.
            b.HasCompanyUniqueIndex("ModuleCode");
        });

        builder.Entity<ExportTemplate>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ExportTemplates", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.EntityName).IsRequired().HasMaxLength(ErpDomainConsts.MaxEntityNameLength);
            b.Property(x => x.Fields).IsRequired().HasMaxLength(ErpDomainConsts.MaxTotalingLength);
            b.HasCompanyUniqueIndex("EntityName", "Name");
        });

        builder.Entity<Company>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "Companies", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<CompanyInformation>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "CompanyInformations", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<DocumentNote>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "DocumentNotes", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<ActivityStreamEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ActivityStreamEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<DocumentActivityTask>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "DocumentActivityTasks", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<KanbanStage>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "KanbanStages", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<AutomatedActionRule>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "AutomatedActionRules", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<ReportLayoutSelection>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ReportLayoutSelections", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ReportName).IsRequired().HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.CustomReportLayoutCode).HasMaxLength(ErpDomainConsts.MaxCustomLayoutCodeLength);
            b.Property(x => x.ReportLayoutDescription).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.ReportCaption).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);

            // One report prints through one layout in a company, which is what the lookup assumes.
            b.HasCompanyUniqueIndex("ReportName");
        });

        builder.Entity<CustomReportLayout>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "CustomReportLayouts", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Code).HasMaxLength(ErpDomainConsts.MaxCustomLayoutCodeLength);
            b.Property(x => x.ReportName).IsRequired().HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.LayoutName).IsRequired().HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.FileExtension).HasMaxLength(ErpDomainConsts.MaxFileExtensionLength);
            b.Property(x => x.LastModifiedByUser).HasMaxLength(ErpDomainConsts.MaxUserNameLength);
            b.Property(x => x.TemplateContent).HasMaxLength(ErpDomainConsts.MaxLayoutTemplateLength);
            b.HasIndex(x => new { x.CompanyId, x.ReportName });
        });

        builder.Entity<JobQueueCategory>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "JobQueueCategories", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Code).IsRequired().HasMaxLength(ErpDomainConsts.MaxJobCategoryCodeLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.HasCompanyUniqueIndex(nameof(JobQueueCategory.Code));
        });

        builder.Entity<JobQueueEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "JobQueueEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Description).IsRequired().HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.CategoryCode).HasMaxLength(ErpDomainConsts.MaxJobCategoryCodeLength);
            b.Property(x => x.JobType).IsRequired().HasMaxLength(ErpDomainConsts.MaxJobTypeLength);
            b.Property(x => x.ParameterString).HasMaxLength(ErpDomainConsts.MaxJobParameterLength);
            b.Property(x => x.LastErrorMessage).HasMaxLength(ErpDomainConsts.MaxJobErrorLength);
            b.Property(x => x.LastErrorStackTrace).HasMaxLength(ErpDomainConsts.MaxJobErrorLength);
            b.Property(x => x.UserId).HasMaxLength(ErpDomainConsts.MaxUserNameLength);
            b.Property(x => x.RecordIdToProcess).HasMaxLength(ErpDomainConsts.MaxRecordIdLength);
            b.Property(x => x.NextRunDateFormula).HasMaxLength(ErpDomainConsts.MaxDateFormulaLength);
            b.HasIndex(x => new { x.CompanyId, x.Status, x.NextRunTime });
        });

        builder.Entity<JobQueueLogEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "JobQueueLogEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.JobDescription).IsRequired().HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.JobType).IsRequired().HasMaxLength(ErpDomainConsts.MaxJobTypeLength);
            b.Property(x => x.ErrorMessage).HasMaxLength(ErpDomainConsts.MaxJobErrorLength);
            b.Property(x => x.ErrorStackTrace).HasMaxLength(ErpDomainConsts.MaxJobErrorLength);
            b.Property(x => x.OutputDetails).HasMaxLength(ErpDomainConsts.MaxJobParameterLength);
            b.Property(x => x.UserId).HasMaxLength(ErpDomainConsts.MaxUserNameLength);
            b.Property(x => x.CategoryCode).HasMaxLength(ErpDomainConsts.MaxJobCategoryCodeLength);
            b.Property(x => x.ParameterString).HasMaxLength(ErpDomainConsts.MaxJobParameterLength);
            b.HasIndex(x => new { x.CompanyId, x.JobQueueEntryId, x.StartDateTime });
        });

        builder.ConfigureErpBaseTables();
        builder.ConfigureErpAttachments();
        builder.ConfigureErpPensions();
        builder.ConfigureErpAcademics();
        builder.ConfigureErpPayroll();
        builder.ConfigureErpPaymentVouchers();
    }
}
