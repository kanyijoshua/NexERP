using System.Collections.Generic;
using System.Linq;
using ABPmicroservice.Erp.Modules;
using ABPmicroservice.Erp.Permissions;

namespace ABPmicroservice.Erp.Profiles;

/// <summary>A link of a role center: where it goes, and what opening it takes.</summary>
public sealed record RoleCenterLink(string NameKey, string Route, string Module, params string[] Permissions);

/// <summary>A top-level entry or sub-menu of a role center's navigation bar: a menu of links (and optional sub-menus), or one link.</summary>
public sealed record RoleCenterMenu(
    string Key,
    string NameKey,
    RoleCenterLink Link,
    IReadOnlyList<RoleCenterLink> DirectLinks,
    IReadOnlyList<RoleCenterMenu> SubMenus = null
)
{
    public IReadOnlyList<RoleCenterMenu> SubMenus { get; init; } = SubMenus ?? [];

    /// <summary>Every link in this menu and its nested sub-menus.</summary>
    public IReadOnlyList<RoleCenterLink> Links =>
        SubMenus.Count == 0
            ? DirectLinks
            : DirectLinks.Concat(SubMenus.SelectMany(s => s.Links)).ToList();
}

/// <summary>A profile and the navigation bar of its role center.</summary>
public sealed record RoleCenterDefinition(string ProfileId, IReadOnlyList<RoleCenterMenu> Navigation)
{
    public string NameKey => $"Profile:{ProfileId}";

    public string DescriptionKey => $"Profile:{ProfileId}:Description";
}

/// <summary>
/// The profiles and their role centers (Business Manager, Accountant, Order Processor, Purchasing
/// Agent, HR Manager, Administrator): a few direct links to the lists used every day, then a menu
/// per area with direct sublinks and grouped sub-menus.
/// Routes and permissions match the Angular routes they open.
/// </summary>
public static class RoleCenterCatalog
{
    // Lists used every day.
    private static readonly RoleCenterLink Customers = new("Menu:Customers", "/erp/customers", ErpModuleRegistry.Sales, ErpPermissions.Customers.Default);
    private static readonly RoleCenterLink Vendors = new("Menu:Vendors", "/erp/vendors", ErpModuleRegistry.Purchasing, ErpPermissions.Vendors.Default);
    private static readonly RoleCenterLink Items = new("Menu:Items", "/erp/items", ErpModuleRegistry.Inventory, ErpPermissions.Items.Default);
    private static readonly RoleCenterLink BankAccounts = new("Menu:BankAccounts", "/erp/bank-accounts", ErpModuleRegistry.CashManagement, ErpPermissions.BankAccounts.Default);
    private static readonly RoleCenterLink ChartOfAccounts = new("Menu:ChartOfAccounts", "/erp/chart-of-accounts", ErpModuleRegistry.Finance, ErpPermissions.GLAccounts.Default);
    private static readonly RoleCenterLink Employees = new("Menu:Employees", "/erp/employees", ErpModuleRegistry.HumanResources, ErpPermissions.Employees.Default);

    // Sales & CRM
    private static readonly RoleCenterLink SalesQuotes = new("Menu:SalesQuotes", "/erp/sales-quotes", ErpModuleRegistry.Sales, ErpPermissions.SalesDocuments.Default);
    private static readonly RoleCenterLink SalesOrders = new("Menu:SalesOrders", "/erp/sales-orders", ErpModuleRegistry.Sales, ErpPermissions.SalesDocuments.Default);
    private static readonly RoleCenterLink SalesInvoices = new("Menu:SalesInvoices", "/erp/sales-invoices", ErpModuleRegistry.Sales, ErpPermissions.SalesDocuments.Default);
    private static readonly RoleCenterLink SalesCreditMemos = new("Menu:SalesCreditMemos", "/erp/sales-credit-memos", ErpModuleRegistry.Sales, ErpPermissions.SalesDocuments.Default);
    private static readonly RoleCenterLink SalesBlanketOrders = new("Menu:SalesBlanketOrders", "/erp/sales-blanket-orders", ErpModuleRegistry.Sales, ErpPermissions.SalesDocuments.Default);
    private static readonly RoleCenterLink SalesReturnOrders = new("Menu:SalesReturnOrders", "/erp/sales-return-orders", ErpModuleRegistry.Sales, ErpPermissions.SalesDocuments.Default);
    private static readonly RoleCenterLink PostedSalesShipments = new("Menu:PostedSalesShipments", "/erp/posted-sales-shipments", ErpModuleRegistry.Sales, ErpPermissions.SalesDocuments.Default);
    private static readonly RoleCenterLink PostedSalesInvoices = new("Menu:PostedSalesInvoices", "/erp/posted-sales-invoices", ErpModuleRegistry.Sales, ErpPermissions.SalesDocuments.Default);
    private static readonly RoleCenterLink CustomerLedgerEntries = new("Menu:CustomerLedgerEntries", "/erp/finance/customer-ledger-entries", ErpModuleRegistry.Sales, ErpPermissions.Customers.Default);
    private static readonly RoleCenterLink CustomerBankAccounts = new("Menu:CustomerBankAccounts", "/erp/customer-bank-accounts", ErpModuleRegistry.Sales, ErpPermissions.Customers.Default);
    private static readonly RoleCenterLink SalespeoplePurchasers = new("Menu:SalespeoplePurchasers", "/erp/salespeople-purchasers", ErpModuleRegistry.Sales, ErpPermissions.SalespeoplePurchasers.Default);
    private static readonly RoleCenterLink PriceLists = new("Menu:PriceLists", "/erp/price-lists", ErpModuleRegistry.Inventory, ErpPermissions.PriceLists.Default);
    private static readonly RoleCenterLink CustomerPriceGroups = new("Menu:CustomerPriceGroups", "/erp/customer-price-groups", ErpModuleRegistry.Inventory, ErpPermissions.PriceLists.Default);
    private static readonly RoleCenterLink CrmPipeline = new("Menu:Pipeline", "/erp/crm/pipeline", ErpModuleRegistry.Crm, ErpPermissions.Crm.Default);
    private static readonly RoleCenterLink Opportunities = new("Menu:Opportunities", "/erp/opportunities", ErpModuleRegistry.Crm, ErpPermissions.Crm.Default);
    private static readonly RoleCenterLink Contacts = new("Menu:Contacts", "/erp/contacts", ErpModuleRegistry.Crm, ErpPermissions.Crm.Default);
    private static readonly RoleCenterLink Campaigns = new("Menu:Campaigns", "/erp/campaigns", ErpModuleRegistry.Crm, ErpPermissions.Crm.Default);
    private static readonly RoleCenterLink InteractionLogEntries = new("Menu:InteractionLogEntries", "/erp/interaction-log-entries", ErpModuleRegistry.Crm, ErpPermissions.Crm.Default);
    private static readonly RoleCenterLink MyActivities = new("Menu:MyActivities", "/erp/crm/my-activities", ErpModuleRegistry.Crm, ErpPermissions.Chatter.Default);
    private static readonly RoleCenterLink PipelineStages = new("Menu:PipelineStages", "/erp/pipeline-stages", ErpModuleRegistry.Crm, ErpPermissions.CrmSetup.Default);
    private static readonly RoleCenterLink CloseOpportunityCodes = new("Menu:CloseOpportunityCodes", "/erp/close-opportunity-codes", ErpModuleRegistry.Crm, ErpPermissions.CrmSetup.Default);
    private static readonly RoleCenterLink ActivityTypes = new("Menu:ActivityTypes", "/erp/activity-types", ErpModuleRegistry.Crm, ErpPermissions.CrmSetup.Default);

    // Purchasing & Procurement
    private static readonly RoleCenterLink PurchaseQuotes = new("Menu:PurchaseQuotes", "/erp/purchase-quotes", ErpModuleRegistry.Purchasing, ErpPermissions.PurchaseDocuments.Default);
    private static readonly RoleCenterLink PurchaseOrders = new("Menu:PurchaseOrders", "/erp/purchase-orders", ErpModuleRegistry.Purchasing, ErpPermissions.PurchaseDocuments.Default);
    private static readonly RoleCenterLink PurchaseInvoices = new("Menu:PurchaseInvoices", "/erp/purchase-invoices", ErpModuleRegistry.Purchasing, ErpPermissions.PurchaseDocuments.Default);
    private static readonly RoleCenterLink PurchaseCreditMemos = new("Menu:PurchaseCreditMemos", "/erp/purchase-credit-memos", ErpModuleRegistry.Purchasing, ErpPermissions.PurchaseDocuments.Default);
    private static readonly RoleCenterLink PurchaseBlanketOrders = new("Menu:PurchaseBlanketOrders", "/erp/purchase-blanket-orders", ErpModuleRegistry.Purchasing, ErpPermissions.PurchaseDocuments.Default);
    private static readonly RoleCenterLink PurchaseReturnOrders = new("Menu:PurchaseReturnOrders", "/erp/purchase-return-orders", ErpModuleRegistry.Purchasing, ErpPermissions.PurchaseDocuments.Default);
    private static readonly RoleCenterLink PostedPurchaseReceipts = new("Menu:PostedPurchaseReceipts", "/erp/posted-purchase-receipts", ErpModuleRegistry.Purchasing, ErpPermissions.PurchaseDocuments.Default);
    private static readonly RoleCenterLink PostedPurchaseInvoices = new("Menu:PostedPurchaseInvoices", "/erp/posted-purchase-invoices", ErpModuleRegistry.Purchasing, ErpPermissions.PurchaseDocuments.Default);
    private static readonly RoleCenterLink VendorLedgerEntries = new("Menu:VendorLedgerEntries", "/erp/finance/vendor-ledger-entries", ErpModuleRegistry.Purchasing, ErpPermissions.Vendors.Default);
    private static readonly RoleCenterLink VendorBankAccounts = new("Menu:VendorBankAccounts", "/erp/vendor-bank-accounts", ErpModuleRegistry.Purchasing, ErpPermissions.Vendors.Default);
    private static readonly RoleCenterLink OrderAddresses = new("Menu:OrderAddresses", "/erp/order-addresses", ErpModuleRegistry.Purchasing, ErpPermissions.Vendors.Default);
    private static readonly RoleCenterLink ItemVendors = new("Menu:ItemVendors", "/erp/item-vendors", ErpModuleRegistry.Purchasing, ErpPermissions.Vendors.Default);
    private static readonly RoleCenterLink PurchaseRequisitions = new("Menu:PurchaseRequisitions", "/erp/purchase-requisitions", ErpModuleRegistry.Procurement, ErpPermissions.PurchaseRequisitions.Default);
    private static readonly RoleCenterLink Tenders = new("Menu:Tenders", "/erp/tenders", ErpModuleRegistry.Procurement, ErpPermissions.Tenders.Default);
    private static readonly RoleCenterLink ProcurementPlans = new("Menu:ProcurementPlans", "/erp/procurement-plans", ErpModuleRegistry.Procurement, ErpPermissions.ProcurementPlans.Default);

    // Inventory, Warehouse & Manufacturing
    private static readonly RoleCenterLink ItemCategories = new("Menu:ItemCategories", "/erp/item-categories", ErpModuleRegistry.Inventory, ErpPermissions.ItemCategories.Default);
    private static readonly RoleCenterLink UnitsOfMeasure = new("Menu:UnitsOfMeasure", "/erp/units-of-measure", ErpModuleRegistry.Inventory, ErpPermissions.UnitsOfMeasure.Default);
    private static readonly RoleCenterLink Locations = new("Menu:Locations", "/erp/locations", ErpModuleRegistry.Inventory, ErpPermissions.Locations.Default);
    private static readonly RoleCenterLink ItemJournals = new("Menu:ItemJournals", "/erp/item-journals", ErpModuleRegistry.Inventory, ErpPermissions.ItemJournals.Default);
    private static readonly RoleCenterLink ItemJournalTemplates = new("Menu:ItemJournalTemplates", "/erp/item-journal-templates", ErpModuleRegistry.Inventory, ErpPermissions.ItemJournals.Default);
    private static readonly RoleCenterLink TransferOrders = new("Menu:TransferOrders", "/erp/transfer-orders", ErpModuleRegistry.Inventory, ErpPermissions.TransferOrders.Default);
    private static readonly RoleCenterLink PostedTransfers = new("Menu:PostedTransfers", "/erp/posted-transfers", ErpModuleRegistry.Inventory, ErpPermissions.TransferOrders.Default);
    private static readonly RoleCenterLink ItemLedgerEntries = new("Menu:ItemLedgerEntries", "/erp/item-ledger-entries", ErpModuleRegistry.Inventory, ErpPermissions.Items.Default);
    private static readonly RoleCenterLink ItemVariants = new("Menu:ItemVariants", "/erp/item-variants", ErpModuleRegistry.Inventory, ErpPermissions.Items.Default);
    private static readonly RoleCenterLink ItemTrackingCodes = new("Menu:ItemTrackingCodes", "/erp/item-tracking-codes", ErpModuleRegistry.Inventory, ErpPermissions.InventorySetup.Default);
    private static readonly RoleCenterLink LotNoInformation = new("Menu:LotNoInformation", "/erp/lot-no-information", ErpModuleRegistry.Inventory, ErpPermissions.Items.Default);
    private static readonly RoleCenterLink SerialNoInformation = new("Menu:SerialNoInformation", "/erp/serial-no-information", ErpModuleRegistry.Inventory, ErpPermissions.Items.Default);
    private static readonly RoleCenterLink InventorySetup = new("Menu:InventorySetup", "/erp/setup/inventory", ErpModuleRegistry.Inventory, ErpPermissions.InventorySetup.Default);
    private static readonly RoleCenterLink ProductionOrders = new("Menu:ProductionOrders", "/erp/production-orders", ErpModuleRegistry.Manufacturing, ErpPermissions.Manufacturing.Default);
    private static readonly RoleCenterLink PlanningWorksheet = new("Menu:PlanningWorksheet", "/erp/planning-worksheet", ErpModuleRegistry.Manufacturing, ErpPermissions.Manufacturing.Default);
    private static readonly RoleCenterLink ProductionBoms = new("Menu:ProductionBoms", "/erp/production-boms", ErpModuleRegistry.Manufacturing, ErpPermissions.Manufacturing.Default);
    private static readonly RoleCenterLink Routings = new("Menu:Routings", "/erp/routings", ErpModuleRegistry.Manufacturing, ErpPermissions.Manufacturing.Default);
    private static readonly RoleCenterLink CapacityLedgerEntries = new("Menu:CapacityLedgerEntries", "/erp/capacity-ledger-entries", ErpModuleRegistry.Manufacturing, ErpPermissions.Manufacturing.Default);
    private static readonly RoleCenterLink WorkCenters = new("Menu:WorkCenters", "/erp/work-centers", ErpModuleRegistry.Manufacturing, ErpPermissions.ManufacturingSetup.Default);
    private static readonly RoleCenterLink MachineCenters = new("Menu:MachineCenters", "/erp/machine-centers", ErpModuleRegistry.Manufacturing, ErpPermissions.ManufacturingSetup.Default);
    private static readonly RoleCenterLink ManufacturingSetup = new("Menu:ManufacturingSetup", "/erp/setup/manufacturing-setup", ErpModuleRegistry.Manufacturing, ErpPermissions.ManufacturingSetup.Default);

    // Projects & Service
    private static readonly RoleCenterLink Projects = new("Menu:ProjectList", "/erp/projects", ErpModuleRegistry.Projects, ErpPermissions.Projects.Default);
    private static readonly RoleCenterLink TimeSheets = new("Menu:TimeSheets", "/erp/time-sheets", ErpModuleRegistry.Projects, ErpPermissions.TimeSheets.Default);
    private static readonly RoleCenterLink ProjectJournals = new("Menu:ProjectJournals", "/erp/project-journals", ErpModuleRegistry.Projects, ErpPermissions.Projects.Default);
    private static readonly RoleCenterLink ProjectLedgerEntries = new("Menu:ProjectLedgerEntries", "/erp/project-ledger-entries", ErpModuleRegistry.Projects, ErpPermissions.Projects.Default);
    private static readonly RoleCenterLink ResourceLedgerEntries = new("Menu:ResourceLedgerEntries", "/erp/resource-ledger-entries", ErpModuleRegistry.Projects, ErpPermissions.Projects.Default);
    private static readonly RoleCenterLink Resources = new("Menu:Resources", "/erp/resources", ErpModuleRegistry.Projects, ErpPermissions.ProjectsSetup.Default);
    private static readonly RoleCenterLink ProjectPostingGroups = new("Menu:ProjectPostingGroups", "/erp/project-posting-groups", ErpModuleRegistry.Projects, ErpPermissions.ProjectsSetup.Default);
    private static readonly RoleCenterLink ServiceBoard = new("Menu:ServiceBoard", "/erp/service/board", ErpModuleRegistry.Service, ErpPermissions.Service.Default);
    private static readonly RoleCenterLink ServiceOrders = new("Menu:ServiceOrders", "/erp/service-orders", ErpModuleRegistry.Service, ErpPermissions.Service.Default);
    private static readonly RoleCenterLink ServiceContracts = new("Menu:ServiceContracts", "/erp/service-contracts", ErpModuleRegistry.Service, ErpPermissions.Service.Default);
    private static readonly RoleCenterLink ServiceItems = new("Menu:ServiceItems", "/erp/service-items", ErpModuleRegistry.Service, ErpPermissions.Service.Default);
    private static readonly RoleCenterLink ServiceLedgerEntries = new("Menu:ServiceLedgerEntries", "/erp/service-ledger-entries", ErpModuleRegistry.Service, ErpPermissions.Service.Default);
    private static readonly RoleCenterLink WarrantyLedgerEntries = new("Menu:WarrantyLedgerEntries", "/erp/warranty-ledger-entries", ErpModuleRegistry.Service, ErpPermissions.Service.Default);
    private static readonly RoleCenterLink ServiceOrderTypes = new("Menu:ServiceOrderTypes", "/erp/service-order-types", ErpModuleRegistry.Service, ErpPermissions.ServiceSetup.Default);
    private static readonly RoleCenterLink FaultCodes = new("Menu:FaultCodes", "/erp/fault-codes", ErpModuleRegistry.Service, ErpPermissions.ServiceSetup.Default);
    private static readonly RoleCenterLink ResolutionCodes = new("Menu:ResolutionCodes", "/erp/resolution-codes", ErpModuleRegistry.Service, ErpPermissions.ServiceSetup.Default);
    private static readonly RoleCenterLink ServiceSetup = new("Menu:ServiceSetup", "/erp/setup/service-setup", ErpModuleRegistry.Service, ErpPermissions.ServiceSetup.Default);

    // Finance, Journals, Budgets, Cash Flow, Collections & Fixed Assets
    private static readonly RoleCenterLink GeneralJournal = new("Menu:GeneralJournal", "/erp/finance/general-journal", ErpModuleRegistry.Finance, ErpPermissions.Journals.Default);
    private static readonly RoleCenterLink JournalTemplates = new("Menu:JournalTemplates", "/erp/finance/journal-templates", ErpModuleRegistry.Finance, ErpPermissions.Journals.Default);
    private static readonly RoleCenterLink GLRegisters = new("Menu:GLRegisters", "/erp/finance/registers", ErpModuleRegistry.Finance, ErpPermissions.GLRegisters.Default);
    private static readonly RoleCenterLink VatEntries = new("Menu:VatEntries", "/erp/finance/vat-entries", ErpModuleRegistry.Finance, ErpPermissions.VatEntries.Default);
    private static readonly RoleCenterLink VatStatement = new("Menu:VatStatement", "/erp/finance/vat-return", ErpModuleRegistry.Finance, ErpPermissions.VatEntries.Default);
    private static readonly RoleCenterLink VatSettlement = new("Menu:VatSettlement", "/erp/finance/vat-settlement", ErpModuleRegistry.Finance, ErpPermissions.PeriodicActivities.SettleVat);
    private static readonly RoleCenterLink AdjustExchangeRates = new("Menu:AdjustExchangeRates", "/erp/finance/exch-rate-adjustment", ErpModuleRegistry.Finance, ErpPermissions.PeriodicActivities.Default);
    private static readonly RoleCenterLink AccountingPeriods = new("Menu:AccountingPeriods", "/erp/finance/accounting-periods", ErpModuleRegistry.Finance, ErpPermissions.FinanceSetup.Default);
    private static readonly RoleCenterLink GLBudgetNames = new("Menu:GLBudgetNames", "/erp/gl-budget-names", ErpModuleRegistry.Finance, ErpPermissions.Budgets.Default);
    private static readonly RoleCenterLink BudgetMatrix = new("Menu:BudgetMatrix", "/erp/finance/budget-matrix", ErpModuleRegistry.Finance, ErpPermissions.Budgets.Default);
    private static readonly RoleCenterLink GLBudgetEntries = new("Menu:GLBudgetEntries", "/erp/gl-budget-entries", ErpModuleRegistry.Finance, ErpPermissions.Budgets.Default);
    private static readonly RoleCenterLink CashFlowForecasts = new("Menu:CashFlowForecasts", "/erp/cash-flow-forecasts", ErpModuleRegistry.Finance, ErpPermissions.CashFlow.Default);
    private static readonly RoleCenterLink CashFlowWorksheet = new("Menu:CashFlowWorksheet", "/erp/cash-flow-worksheet", ErpModuleRegistry.Finance, ErpPermissions.CashFlow.Default);
    private static readonly RoleCenterLink CashFlowManualEntries = new("Menu:CashFlowManualEntries", "/erp/cash-flow-manual-entries", ErpModuleRegistry.Finance, ErpPermissions.CashFlow.Default);
    private static readonly RoleCenterLink CashFlowForecastEntries = new("Menu:CashFlowForecastEntries", "/erp/cash-flow-forecast-entries", ErpModuleRegistry.Finance, ErpPermissions.CashFlow.Default);
    private static readonly RoleCenterLink DeferralTemplates = new("Menu:DeferralTemplates", "/erp/deferral-templates", ErpModuleRegistry.Finance, ErpPermissions.Deferrals.Default);
    private static readonly RoleCenterLink PostedDeferrals = new("Menu:PostedDeferrals", "/erp/posted-deferrals", ErpModuleRegistry.Finance, ErpPermissions.Deferrals.Default);
    private static readonly RoleCenterLink Reminders = new("Menu:Reminders", "/erp/reminders", ErpModuleRegistry.Finance, ErpPermissions.Reminders.Default);
    private static readonly RoleCenterLink FinanceChargeMemos = new("Menu:FinanceChargeMemos", "/erp/finance-charge-memos", ErpModuleRegistry.Finance, ErpPermissions.FinanceChargeMemos.Default);
    private static readonly RoleCenterLink ReminderTerms = new("Menu:ReminderTerms", "/erp/reminder-terms", ErpModuleRegistry.Finance, ErpPermissions.CollectionSetup.Default);
    private static readonly RoleCenterLink FinanceChargeTerms = new("Menu:FinanceChargeTerms", "/erp/finance-charge-terms", ErpModuleRegistry.Finance, ErpPermissions.CollectionSetup.Default);
    private static readonly RoleCenterLink Currencies = new("Menu:Currencies", "/erp/currencies", ErpModuleRegistry.Finance, ErpPermissions.FinanceSetup.Default);
    private static readonly RoleCenterLink CurrencyExchangeRates = new("Menu:CurrencyExchangeRates", "/erp/currency-exchange-rates", ErpModuleRegistry.Finance, ErpPermissions.FinanceSetup.Default);
    private static readonly RoleCenterLink PaymentTerms = new("Menu:PaymentTerms", "/erp/payment-terms", ErpModuleRegistry.Finance, ErpPermissions.FinanceSetup.Default);
    private static readonly RoleCenterLink PaymentMethods = new("Menu:PaymentMethods", "/erp/payment-methods", ErpModuleRegistry.Finance, ErpPermissions.FinanceSetup.Default);
    private static readonly RoleCenterLink SourceCodes = new("Menu:SourceCodes", "/erp/source-codes", ErpModuleRegistry.Finance, ErpPermissions.FinanceSetup.Default);
    private static readonly RoleCenterLink ReasonCodes = new("Menu:ReasonCodes", "/erp/reason-codes", ErpModuleRegistry.Finance, ErpPermissions.FinanceSetup.Default);
    private static readonly RoleCenterLink CountriesRegions = new("Menu:CountriesRegions", "/erp/countries-regions", ErpModuleRegistry.Finance, ErpPermissions.FinanceSetup.Default);
    private static readonly RoleCenterLink PostCodes = new("Menu:PostCodes", "/erp/post-codes", ErpModuleRegistry.Finance, ErpPermissions.FinanceSetup.Default);
    private static readonly RoleCenterLink ShipmentMethods = new("Menu:ShipmentMethods", "/erp/shipment-methods", ErpModuleRegistry.Finance, ErpPermissions.FinanceSetup.Default);
    private static readonly RoleCenterLink ResponsibilityCenters = new("Menu:ResponsibilityCenters", "/erp/responsibility-centers", ErpModuleRegistry.Finance, ErpPermissions.FinanceSetup.Default);
    private static readonly RoleCenterLink GeneralLedgerSetup = new("Menu:GeneralLedgerSetup", "/erp/setup/general-ledger", ErpModuleRegistry.Finance, ErpPermissions.GeneralLedgerSetup.Default);

    // Posting Setup
    private static readonly RoleCenterLink GeneralPostingSetup = new("Menu:GeneralPostingSetup", "/erp/general-posting-setup", ErpModuleRegistry.Finance, ErpPermissions.PostingSetup.Default);
    private static readonly RoleCenterLink GenBusPostingGroups = new("Menu:GenBusPostingGroups", "/erp/gen-bus-posting-groups", ErpModuleRegistry.Finance, ErpPermissions.PostingSetup.Default);
    private static readonly RoleCenterLink GenProdPostingGroups = new("Menu:GenProdPostingGroups", "/erp/gen-prod-posting-groups", ErpModuleRegistry.Finance, ErpPermissions.PostingSetup.Default);
    private static readonly RoleCenterLink VatPostingSetup = new("Menu:VatPostingSetup", "/erp/vat-posting-setup", ErpModuleRegistry.Finance, ErpPermissions.PostingSetup.Default);
    private static readonly RoleCenterLink VatBusPostingGroups = new("Menu:VatBusPostingGroups", "/erp/vat-bus-posting-groups", ErpModuleRegistry.Finance, ErpPermissions.PostingSetup.Default);
    private static readonly RoleCenterLink VatProdPostingGroups = new("Menu:VatProdPostingGroups", "/erp/vat-prod-posting-groups", ErpModuleRegistry.Finance, ErpPermissions.PostingSetup.Default);
    private static readonly RoleCenterLink CustomerPostingGroups = new("Menu:CustomerPostingGroups", "/erp/customer-posting-groups", ErpModuleRegistry.Sales, ErpPermissions.PostingSetup.Default);
    private static readonly RoleCenterLink VendorPostingGroups = new("Menu:VendorPostingGroups", "/erp/vendor-posting-groups", ErpModuleRegistry.Purchasing, ErpPermissions.PostingSetup.Default);
    private static readonly RoleCenterLink InventoryPostingGroups = new("Menu:InventoryPostingGroups", "/erp/inventory-posting-groups", ErpModuleRegistry.Inventory, ErpPermissions.PostingSetup.Default);
    private static readonly RoleCenterLink InventoryPostingSetup = new("Menu:InventoryPostingSetup", "/erp/inventory-posting-setup", ErpModuleRegistry.Inventory, ErpPermissions.PostingSetup.Default);
    private static readonly RoleCenterLink BankAccountPostingGroups = new("Menu:BankAccountPostingGroups", "/erp/bank-account-posting-groups", ErpModuleRegistry.CashManagement, ErpPermissions.PostingSetup.Default);

    // Fixed Assets
    private static readonly RoleCenterLink FixedAssets = new("Menu:FixedAssets", "/erp/fixed-assets", ErpModuleRegistry.FixedAssets, ErpPermissions.FixedAssets.Default);
    private static readonly RoleCenterLink FAJournals = new("Menu:FAJournals", "/erp/fa-journals", ErpModuleRegistry.FixedAssets, ErpPermissions.FAJournals.Default);
    private static readonly RoleCenterLink FALedgerEntries = new("Menu:FALedgerEntries", "/erp/fa-ledger-entries", ErpModuleRegistry.FixedAssets, ErpPermissions.FixedAssets.Default);
    private static readonly RoleCenterLink FADepreciationBooks = new("Menu:FADepreciationBooks", "/erp/fa-depreciation-books", ErpModuleRegistry.FixedAssets, ErpPermissions.FixedAssets.Default);
    private static readonly RoleCenterLink DepreciationBooks = new("Menu:DepreciationBooks", "/erp/depreciation-books", ErpModuleRegistry.FixedAssets, ErpPermissions.FixedAssetSetup.Default);
    private static readonly RoleCenterLink FAClasses = new("Menu:FAClasses", "/erp/fa-classes", ErpModuleRegistry.FixedAssets, ErpPermissions.FixedAssetSetup.Default);
    private static readonly RoleCenterLink FASubclasses = new("Menu:FASubclasses", "/erp/fa-subclasses", ErpModuleRegistry.FixedAssets, ErpPermissions.FixedAssetSetup.Default);
    private static readonly RoleCenterLink FALocations = new("Menu:FALocations", "/erp/fa-locations", ErpModuleRegistry.FixedAssets, ErpPermissions.FixedAssetSetup.Default);
    private static readonly RoleCenterLink FAPostingGroups = new("Menu:FAPostingGroups", "/erp/fa-posting-groups", ErpModuleRegistry.FixedAssets, ErpPermissions.FixedAssetSetup.Default);
    private static readonly RoleCenterLink Maintenances = new("Menu:Maintenances", "/erp/maintenances", ErpModuleRegistry.FixedAssets, ErpPermissions.FixedAssetSetup.Default);
    private static readonly RoleCenterLink MaintenanceRegistrations = new("Menu:MaintenanceRegistrations", "/erp/maintenance-registrations", ErpModuleRegistry.FixedAssets, ErpPermissions.FixedAssets.Default);
    private static readonly RoleCenterLink MainAssetComponents = new("Menu:MainAssetComponents", "/erp/main-asset-components", ErpModuleRegistry.FixedAssets, ErpPermissions.FixedAssets.Default);
    private static readonly RoleCenterLink FASetups = new("Menu:FASetups", "/erp/setup/fa-setups", ErpModuleRegistry.FixedAssets, ErpPermissions.FixedAssetSetup.Default);

    // Cash Management
    private static readonly RoleCenterLink PaymentVouchers = new("Menu:PaymentVouchers", "/erp/payment-vouchers", ErpModuleRegistry.CashManagement, ErpPermissions.PaymentVouchers.Default);
    private static readonly RoleCenterLink BankReconciliations = new("Menu:BankAccReconciliations", "/erp/bank-acc-reconciliations", ErpModuleRegistry.CashManagement, ErpPermissions.BankReconciliations.Default);
    private static readonly RoleCenterLink BankAccountStatements = new("Menu:BankAccountStatements", "/erp/bank-account-statements", ErpModuleRegistry.CashManagement, ErpPermissions.BankReconciliations.Default);
    private static readonly RoleCenterLink BankLedgerEntries = new("Menu:BankAccountLedgerEntries", "/erp/finance/bank-ledger-entries", ErpModuleRegistry.CashManagement, ErpPermissions.BankAccounts.Default);
    private static readonly RoleCenterLink CheckLedgerEntries = new("Menu:CheckLedgerEntries", "/erp/check-ledger-entries", ErpModuleRegistry.CashManagement, ErpPermissions.BankAccounts.Default);
    private static readonly RoleCenterLink PaymentTypes = new("Menu:PaymentTypes", "/erp/payment-types", ErpModuleRegistry.CashManagement, ErpPermissions.PaymentVoucherSetup.Default);
    private static readonly RoleCenterLink PaymentDeductionCodes = new("Menu:PaymentDeductionCodes", "/erp/payment-deduction-codes", ErpModuleRegistry.CashManagement, ErpPermissions.PaymentVoucherSetup.Default);
    private static readonly RoleCenterLink CashManagementSetup = new("Menu:CashManagementSetup", "/erp/setup/cash-management", ErpModuleRegistry.CashManagement, ErpPermissions.PaymentVoucherSetup.Default);

    // Human Resources, Payroll & Pensions
    private static readonly RoleCenterLink EmployeeAbsences = new("Menu:EmployeeAbsences", "/erp/employee-absences", ErpModuleRegistry.HumanResources, ErpPermissions.Employees.Default);
    private static readonly RoleCenterLink EmployeeLedgerEntries = new("Menu:EmployeeLedgerEntries", "/erp/finance/employee-ledger-entries", ErpModuleRegistry.HumanResources, ErpPermissions.Employees.Default);
    private static readonly RoleCenterLink CausesOfAbsence = new("Menu:CausesOfAbsence", "/erp/causes-of-absence", ErpModuleRegistry.HumanResources, ErpPermissions.HumanResourcesSetup.Default);
    private static readonly RoleCenterLink Qualifications = new("Menu:Qualifications", "/erp/qualifications", ErpModuleRegistry.HumanResources, ErpPermissions.HumanResourcesSetup.Default);
    private static readonly RoleCenterLink EmploymentContracts = new("Menu:EmploymentContracts", "/erp/employment-contracts", ErpModuleRegistry.HumanResources, ErpPermissions.HumanResourcesSetup.Default);
    private static readonly RoleCenterLink Unions = new("Menu:Unions", "/erp/unions", ErpModuleRegistry.HumanResources, ErpPermissions.HumanResourcesSetup.Default);
    private static readonly RoleCenterLink GroundsForTermination = new("Menu:GroundsForTermination", "/erp/grounds-for-termination", ErpModuleRegistry.HumanResources, ErpPermissions.HumanResourcesSetup.Default);
    private static readonly RoleCenterLink HrUnitsOfMeasure = new("Menu:HumanResourceUnitsOfMeasure", "/erp/hr-units-of-measure", ErpModuleRegistry.HumanResources, ErpPermissions.HumanResourcesSetup.Default);
    private static readonly RoleCenterLink EmployeePostingGroups = new("Menu:EmployeePostingGroups", "/erp/employee-posting-groups", ErpModuleRegistry.HumanResources, ErpPermissions.HumanResourcesSetup.Default);
    private static readonly RoleCenterLink EmployeeStatisticsGroups = new("Menu:EmployeeStatisticsGroups", "/erp/employee-statistics-groups", ErpModuleRegistry.HumanResources, ErpPermissions.HumanResourcesSetup.Default);
    private static readonly RoleCenterLink Relatives = new("Menu:Relatives", "/erp/relatives", ErpModuleRegistry.HumanResources, ErpPermissions.HumanResourcesSetup.Default);
    private static readonly RoleCenterLink EmployeeRelatives = new("Menu:EmployeeRelatives", "/erp/employee-relatives", ErpModuleRegistry.HumanResources, ErpPermissions.Employees.Default);
    private static readonly RoleCenterLink EmployeeQualifications = new("Menu:EmployeeQualifications", "/erp/employee-qualifications", ErpModuleRegistry.HumanResources, ErpPermissions.Employees.Default);
    private static readonly RoleCenterLink MiscArticles = new("Menu:MiscArticles", "/erp/misc-articles", ErpModuleRegistry.HumanResources, ErpPermissions.HumanResourcesSetup.Default);
    private static readonly RoleCenterLink MiscArticleInformations = new("Menu:MiscArticleInformations", "/erp/misc-article-informations", ErpModuleRegistry.HumanResources, ErpPermissions.Employees.Default);
    private static readonly RoleCenterLink Confidentials = new("Menu:Confidentials", "/erp/confidentials", ErpModuleRegistry.HumanResources, ErpPermissions.HumanResourcesSetup.Default);
    private static readonly RoleCenterLink ConfidentialInformations = new("Menu:ConfidentialInformations", "/erp/confidential-informations", ErpModuleRegistry.HumanResources, ErpPermissions.Employees.Default);
    private static readonly RoleCenterLink AlternativeAddresses = new("Menu:AlternativeAddresses", "/erp/alternative-addresses", ErpModuleRegistry.HumanResources, ErpPermissions.Employees.Default);
    private static readonly RoleCenterLink HumanResourcesSetup = new("Menu:HumanResourcesSetup", "/erp/setup/human-resources", ErpModuleRegistry.HumanResources, ErpPermissions.HumanResourcesSetup.Default);

    private static readonly RoleCenterLink PayrollRuns = new("Menu:PayrollRuns", "/erp/payroll-runs", ErpModuleRegistry.Payroll, ErpPermissions.Payroll.Default);
    private static readonly RoleCenterLink Payslips = new("Menu:Payslips", "/erp/payslips", ErpModuleRegistry.Payroll, ErpPermissions.Payroll.Default);
    private static readonly RoleCenterLink EmployeePayItems = new("Menu:EmployeePayItems", "/erp/employee-pay-items", ErpModuleRegistry.Payroll, ErpPermissions.Payroll.Default);
    private static readonly RoleCenterLink PayrollEarnings = new("Menu:PayrollEarnings", "/erp/payroll-earnings", ErpModuleRegistry.Payroll, ErpPermissions.PayrollSetup.Default);
    private static readonly RoleCenterLink PayrollDeductions = new("Menu:PayrollDeductions", "/erp/payroll-deductions", ErpModuleRegistry.Payroll, ErpPermissions.PayrollSetup.Default);
    private static readonly RoleCenterLink PayrollTaxBands = new("Menu:PayrollTaxBands", "/erp/payroll-tax-bands", ErpModuleRegistry.Payroll, ErpPermissions.PayrollSetup.Default);
    private static readonly RoleCenterLink PayrollSetup = new("Menu:PayrollSetup", "/erp/setup/payroll", ErpModuleRegistry.Payroll, ErpPermissions.PayrollSetup.Default);

    private static readonly RoleCenterLink PensionSponsors = new("Menu:PensionSponsors", "/erp/pension-sponsors", ErpModuleRegistry.Pensions, ErpPermissions.Pensions.Default);
    private static readonly RoleCenterLink PensionMembers = new("Menu:PensionMembers", "/erp/pension-members", ErpModuleRegistry.Pensions, ErpPermissions.Pensions.Default);
    private static readonly RoleCenterLink PensionContributions = new("Menu:PensionContributions", "/erp/pension-contributions", ErpModuleRegistry.Pensions, ErpPermissions.Pensions.Default);
    private static readonly RoleCenterLink MemberExits = new("Menu:MemberExits", "/erp/member-exits", ErpModuleRegistry.Pensions, ErpPermissions.Pensions.Default);
    private static readonly RoleCenterLink MemberLedgerEntries = new("Menu:MemberLedgerEntries", "/erp/member-ledger-entries", ErpModuleRegistry.Pensions, ErpPermissions.Pensions.Default);
    private static readonly RoleCenterLink Pensioners = new("Menu:Pensioners", "/erp/pensioners", ErpModuleRegistry.Pensions, ErpPermissions.Pensions.Default);
    private static readonly RoleCenterLink PensionPayrolls = new("Menu:PensionPayrolls", "/erp/pension-payrolls", ErpModuleRegistry.Pensions, ErpPermissions.Pensions.Default);
    private static readonly RoleCenterLink PensionBenefitCalculations = new("Menu:PensionBenefitCalculations", "/erp/pension-benefit-calculations", ErpModuleRegistry.Pensions, ErpPermissions.Pensions.Default);
    private static readonly RoleCenterLink PensionSchemes = new("Menu:PensionSchemes", "/erp/pension-schemes", ErpModuleRegistry.Pensions, ErpPermissions.PensionSetup.Default);
    private static readonly RoleCenterLink PensionInterestRates = new("Menu:PensionInterestRates", "/erp/pension-interest-rates", ErpModuleRegistry.Pensions, ErpPermissions.PensionSetup.Default);
    private static readonly RoleCenterLink ExitReasons = new("Menu:ExitReasons", "/erp/exit-reasons", ErpModuleRegistry.Pensions, ErpPermissions.PensionSetup.Default);
    private static readonly RoleCenterLink PensionSetup = new("Menu:PensionSetup", "/erp/setup/pension-setup", ErpModuleRegistry.Pensions, ErpPermissions.PensionSetup.Default);

    // Academics
    private static readonly RoleCenterLink StudentApplications = new("Menu:StudentApplications", "/erp/student-applications", ErpModuleRegistry.Academics, ErpPermissions.Academics.Default);
    private static readonly RoleCenterLink Students = new("Menu:Students", "/erp/students", ErpModuleRegistry.Academics, ErpPermissions.Academics.Default);
    private static readonly RoleCenterLink SemesterRegistrations = new("Menu:SemesterRegistrations", "/erp/semester-registrations", ErpModuleRegistry.Academics, ErpPermissions.Academics.Default);
    private static readonly RoleCenterLink StudentUnits = new("Menu:StudentUnits", "/erp/student-units", ErpModuleRegistry.Academics, ErpPermissions.Academics.Default);
    private static readonly RoleCenterLink StudentBills = new("Menu:StudentBills", "/erp/student-bills", ErpModuleRegistry.Academics, ErpPermissions.Academics.Default);
    private static readonly RoleCenterLink StudentReceipts = new("Menu:StudentReceipts", "/erp/student-receipts", ErpModuleRegistry.Academics, ErpPermissions.Academics.Default);
    private static readonly RoleCenterLink StudentRefunds = new("Menu:StudentRefunds", "/erp/student-refunds", ErpModuleRegistry.Academics, ErpPermissions.Academics.Default);
    private static readonly RoleCenterLink ExamResults = new("Menu:ExamResults", "/erp/exam-results", ErpModuleRegistry.Academics, ErpPermissions.Academics.Default);
    private static readonly RoleCenterLink Timetable = new("Menu:Timetable", "/erp/timetable", ErpModuleRegistry.Academics, ErpPermissions.Academics.Default);
    private static readonly RoleCenterLink AttendanceRegisters = new("Menu:AttendanceRegisters", "/erp/attendance-registers", ErpModuleRegistry.Academics, ErpPermissions.Academics.Default);
    private static readonly RoleCenterLink HostelAllocations = new("Menu:HostelAllocations", "/erp/hostel-allocations", ErpModuleRegistry.Academics, ErpPermissions.Academics.Default);
    private static readonly RoleCenterLink Programmes = new("Menu:Programmes", "/erp/programmes", ErpModuleRegistry.Academics, ErpPermissions.AcademicSetup.Default);
    private static readonly RoleCenterLink CourseUnits = new("Menu:CourseUnits", "/erp/course-units", ErpModuleRegistry.Academics, ErpPermissions.AcademicSetup.Default);
    private static readonly RoleCenterLink AcademicYears = new("Menu:AcademicYears", "/erp/academic-years", ErpModuleRegistry.Academics, ErpPermissions.AcademicSetup.Default);
    private static readonly RoleCenterLink Semesters = new("Menu:Semesters", "/erp/semesters", ErpModuleRegistry.Academics, ErpPermissions.AcademicSetup.Default);
    private static readonly RoleCenterLink FeeStructure = new("Menu:FeeStructure", "/erp/fee-structure", ErpModuleRegistry.Academics, ErpPermissions.AcademicSetup.Default);
    private static readonly RoleCenterLink AcademicSetup = new("Menu:AcademicSetup", "/erp/setup/academic-setup", ErpModuleRegistry.Academics, ErpPermissions.AcademicSetup.Default);

    // Approvals
    private static readonly RoleCenterLink RequestsToApprove = new("Menu:RequestsToApprove", "/erp/approvals", ErpModuleRegistry.Approvals, ErpPermissions.Workflows.Default);
    private static readonly RoleCenterLink Workflows = new("Menu:Workflows", "/erp/setup/workflows", ErpModuleRegistry.Approvals, ErpPermissions.Workflows.Manage);
    private static readonly RoleCenterLink ApprovalUserSetup = new("Menu:ApprovalUserSetup", "/erp/setup/approval-users", ErpModuleRegistry.Approvals, ErpPermissions.ApprovalUserSetup.Default);
    private static readonly RoleCenterLink WorkflowUserGroups = new("Menu:WorkflowUserGroups", "/erp/workflow-user-groups", ErpModuleRegistry.Approvals, ErpPermissions.WorkflowUserGroups.Default);
    private static readonly RoleCenterLink WorkflowUserGroupMembers = new("Menu:WorkflowUserGroupMembers", "/erp/workflow-user-group-members", ErpModuleRegistry.Approvals, ErpPermissions.WorkflowUserGroups.Default);

    // Reports
    private static readonly RoleCenterLink FinancialReports = new("Menu:FinancialReports", "/erp/reports/financial", ErpModuleRegistry.Reporting, ErpPermissions.Reports.Default);
    private static readonly RoleCenterLink StandardReports = new("Menu:StandardReports", "/erp/reports/standard", ErpModuleRegistry.Reporting, ErpPermissions.Reports.Default);
    private static readonly RoleCenterLink AccountSchedules = new("Menu:AccountSchedules", "/erp/reports/account-schedules", ErpModuleRegistry.Reporting, ErpPermissions.AccountSchedules.Default);
    private static readonly RoleCenterLink ColumnLayouts = new("Menu:ColumnLayouts", "/erp/reports/column-layouts", ErpModuleRegistry.Reporting, ErpPermissions.AccountSchedules.Default);
    private static readonly RoleCenterLink ReportLayouts = new("Menu:ReportLayouts", "/erp/reports/report-layouts", ErpModuleRegistry.Reporting, ErpPermissions.ReportLayouts.Default);
    private static readonly RoleCenterLink ReportSelections = new("Menu:ReportSelections", "/erp/report-selections", ErpModuleRegistry.Reporting, ErpPermissions.ReportSelections.Default);
    private static readonly RoleCenterLink CustomReportSelections = new("Menu:CustomReportSelections", "/erp/custom-report-selections", ErpModuleRegistry.Reporting, ErpPermissions.ReportSelections.Default);

    // Administration & Integration
    private static readonly RoleCenterLink Dimensions = new("Menu:Dimensions", "/erp/setup/dimensions", ErpModuleRegistry.Core, ErpPermissions.Dimensions.Default);
    private static readonly RoleCenterLink Modules = new("Menu:Modules", "/erp/setup/modules", ErpModuleRegistry.Core, ErpPermissions.Modules.Default);
    private static readonly RoleCenterLink Theme = new("Menu:Theme", "/erp/setup/theme", ErpModuleRegistry.Core, ErpPermissions.Theme.Default);
    private static readonly RoleCenterLink NoSeries = new("Menu:NoSeries", "/erp/setup/no-series", ErpModuleRegistry.Core, ErpPermissions.NoSeries.Default);
    private static readonly RoleCenterLink Profiles = new("Menu:Profiles", "/erp/setup/profiles", ErpModuleRegistry.Core, ErpPermissions.Profiles.Default);
    private static readonly RoleCenterLink UserSetups = new("Menu:UserSetups", "/erp/user-setups", ErpModuleRegistry.Finance, ErpPermissions.UserSetup.Default);
    private static readonly RoleCenterLink JobQueue = new("Menu:JobQueue", "/erp/setup/job-queue", ErpModuleRegistry.Core, ErpPermissions.JobQueue.Default);
    private static readonly RoleCenterLink WebServices = new("Menu:WebServices", "/erp/setup/web-services", ErpModuleRegistry.Integration, ErpPermissions.WebServices.Default, ErpPermissions.DataExport.Default);
    private static readonly RoleCenterLink Webhooks = new("Menu:Webhooks", "/erp/setup/webhooks", ErpModuleRegistry.Integration, ErpPermissions.Webhooks.Default, ErpPermissions.DataExport.Default);
    private static readonly RoleCenterLink DataExport = new("Menu:DataExport", "/erp/setup/data-export", ErpModuleRegistry.DataExport, ErpPermissions.DataExport.Default);
    private static readonly RoleCenterLink ConfigPackages = new("Menu:ConfigPackages", "/erp/rapid-start/packages", ErpModuleRegistry.RapidStart, ErpPermissions.RapidStart.Default);
    private static readonly RoleCenterLink ConfigWorksheet = new("Menu:ConfigWorksheet", "/erp/rapid-start/worksheet", ErpModuleRegistry.RapidStart, ErpPermissions.RapidStart.Default);
    private static readonly RoleCenterLink ConfigTemplates = new("Menu:ConfigTemplates", "/erp/rapid-start/templates", ErpModuleRegistry.RapidStart, ErpPermissions.RapidStart.Default);
    private static readonly RoleCenterLink ImportData = new("Menu:ImportData", "/erp/rapid-start/import", ErpModuleRegistry.RapidStart, ErpPermissions.RapidStart.Apply);

    // Helpers
    private static RoleCenterMenu Link(RoleCenterLink link) => new(link.Route, link.NameKey, link, []);

    private static RoleCenterMenu Menu(string key, string nameKey, params RoleCenterLink[] links) =>
        new(key, nameKey, null, links);

    private static RoleCenterMenu MenuWithSubMenus(
        string key,
        string nameKey,
        IReadOnlyList<RoleCenterLink> directLinks,
        params RoleCenterMenu[] subMenus
    ) => new(key, nameKey, null, directLinks, subMenus);

    // Sub-menus reusable across areas
    private static readonly RoleCenterMenu BudgetsSubMenu = Menu(
        "finance-budgets",
        "Menu:Budgets",
        GLBudgetNames,
        BudgetMatrix,
        GLBudgetEntries
    );

    private static readonly RoleCenterMenu CashFlowSubMenu = Menu(
        "finance-cash-flow",
        "Menu:CashFlow",
        CashFlowForecasts,
        CashFlowWorksheet,
        CashFlowManualEntries,
        CashFlowForecastEntries
    );

    private static readonly RoleCenterMenu PeriodicSubMenu = Menu(
        "finance-periodic",
        "Menu:PeriodicActivities",
        VatStatement,
        VatSettlement,
        AdjustExchangeRates,
        DeferralTemplates,
        PostedDeferrals
    );

    private static readonly RoleCenterMenu CollectionsSubMenu = Menu(
        "finance-collections",
        "Menu:Collections",
        Reminders,
        FinanceChargeMemos,
        ReminderTerms,
        FinanceChargeTerms
    );

    private static readonly RoleCenterMenu FixedAssetsSubMenu = Menu(
        "finance-fixed-assets",
        "Menu:FixedAssets",
        FixedAssets,
        FAJournals,
        FALedgerEntries,
        FADepreciationBooks,
        DepreciationBooks,
        FAClasses,
        FASubclasses,
        FALocations,
        FAPostingGroups,
        Maintenances,
        MaintenanceRegistrations,
        MainAssetComponents,
        FASetups
    );

    private static readonly RoleCenterMenu FinanceSetupSubMenu = Menu(
        "finance-setup",
        "Menu:FinanceSetup",
        GeneralLedgerSetup,
        Dimensions,
        Currencies,
        CurrencyExchangeRates,
        PaymentTerms,
        PaymentMethods,
        SourceCodes,
        ReasonCodes,
        CountriesRegions,
        PostCodes,
        ShipmentMethods,
        ResponsibilityCenters
    );

    private static readonly RoleCenterMenu PostingSetup = Menu(
        "posting-setup",
        "Menu:PostingSetup",
        GeneralPostingSetup,
        GenBusPostingGroups,
        GenProdPostingGroups,
        VatPostingSetup,
        VatBusPostingGroups,
        VatProdPostingGroups,
        CustomerPostingGroups,
        VendorPostingGroups,
        InventoryPostingGroups,
        InventoryPostingSetup,
        BankAccountPostingGroups
    );

    private static readonly RoleCenterMenu Finance = MenuWithSubMenus(
        "finance",
        "Menu:Finance",
        [
            ChartOfAccounts,
            GeneralJournal,
            GLRegisters,
            Dimensions,
            VatEntries,
            AccountingPeriods,
            Currencies,
            PaymentTerms,
            GeneralLedgerSetup,
        ],
        BudgetsSubMenu,
        CashFlowSubMenu,
        PeriodicSubMenu,
        CollectionsSubMenu,
        FixedAssetsSubMenu,
        FinanceSetupSubMenu,
        PostingSetup
    );

    private static readonly RoleCenterMenu Journals = Menu(
        "journals",
        "Menu:Journals",
        GeneralJournal,
        ItemJournals,
        FAJournals,
        ProjectJournals,
        JournalTemplates,
        ItemJournalTemplates,
        GLRegisters
    );

    private static readonly RoleCenterMenu CashManagement = MenuWithSubMenus(
        "cash-management",
        "Menu:CashManagement",
        [
            BankAccounts,
            PaymentVouchers,
            BankReconciliations,
            BankAccountStatements,
            BankLedgerEntries,
            CheckLedgerEntries,
        ],
        CashFlowSubMenu,
        Menu(
            "cash-management-setup",
            "Menu:Setup",
            PaymentMethods,
            PaymentTypes,
            PaymentDeductionCodes,
            BankAccountPostingGroups,
            CustomerBankAccounts,
            VendorBankAccounts,
            CashManagementSetup
        )
    );

    private static readonly RoleCenterMenu PeriodicActivities = Menu(
        "periodic-activities",
        "Menu:PeriodicActivities",
        VatStatement,
        VatSettlement,
        AdjustExchangeRates,
        AccountingPeriods,
        Reminders,
        FinanceChargeMemos,
        CashFlowForecasts,
        CashFlowWorksheet,
        DeferralTemplates,
        PostedDeferrals,
        BudgetMatrix,
        FAJournals
    );

    private static readonly RoleCenterMenu Sales = MenuWithSubMenus(
        "sales",
        "Menu:Sales",
        [
            Customers,
            SalesQuotes,
            SalesOrders,
            SalesInvoices,
            SalesCreditMemos,
            SalesBlanketOrders,
            SalesReturnOrders,
        ],
        Menu(
            "sales-posted",
            "Menu:SalesDocuments",
            PostedSalesShipments,
            PostedSalesInvoices,
            CustomerLedgerEntries
        ),
        Menu(
            "sales-pricing",
            "Menu:PriceLists",
            PriceLists,
            CustomerPriceGroups,
            SalespeoplePurchasers,
            CustomerBankAccounts,
            CustomerPostingGroups
        ),
        Menu(
            "sales-crm",
            "Menu:Crm",
            CrmPipeline,
            Opportunities,
            Contacts,
            Campaigns,
            InteractionLogEntries,
            MyActivities,
            PipelineStages,
            CloseOpportunityCodes,
            ActivityTypes
        ),
        CollectionsSubMenu
    );

    private static readonly RoleCenterMenu Purchasing = MenuWithSubMenus(
        "purchasing",
        "Menu:Purchasing",
        [
            Vendors,
            PurchaseQuotes,
            PurchaseOrders,
            PurchaseInvoices,
            PurchaseCreditMemos,
            PurchaseBlanketOrders,
            PurchaseReturnOrders,
        ],
        Menu(
            "purchasing-procurement",
            "Menu:Procurement",
            PurchaseRequisitions,
            ProcurementPlans,
            Tenders
        ),
        Menu(
            "purchasing-posted",
            "Menu:PurchaseDocuments",
            PostedPurchaseReceipts,
            PostedPurchaseInvoices,
            VendorLedgerEntries
        ),
        Menu(
            "purchasing-setup",
            "Menu:PurchasingSetup",
            ItemVendors,
            OrderAddresses,
            VendorBankAccounts,
            VendorPostingGroups,
            SalespeoplePurchasers
        )
    );

    private static readonly RoleCenterMenu Inventory = MenuWithSubMenus(
        "inventory",
        "Menu:Inventory",
        [
            Items,
            ItemCategories,
            UnitsOfMeasure,
            Locations,
            ItemJournals,
            TransferOrders,
            ItemLedgerEntries,
        ],
        Menu(
            "inventory-warehouse",
            "Menu:Warehouse",
            PostedTransfers,
            ItemVariants,
            ItemTrackingCodes,
            LotNoInformation,
            SerialNoInformation,
            ItemJournalTemplates
        ),
        Menu(
            "inventory-manufacturing",
            "Menu:Manufacturing",
            ProductionOrders,
            PlanningWorksheet,
            ProductionBoms,
            Routings,
            WorkCenters,
            MachineCenters,
            CapacityLedgerEntries,
            ManufacturingSetup
        ),
        Menu(
            "inventory-setup",
            "Menu:InventorySetup",
            InventorySetup,
            InventoryPostingGroups,
            InventoryPostingSetup,
            PriceLists,
            CustomerPriceGroups
        )
    );

    private static readonly RoleCenterMenu ProjectsAndService = MenuWithSubMenus(
        "projects",
        "Menu:Projects",
        [
            Projects,
            TimeSheets,
            ProjectJournals,
            Resources,
            ProjectLedgerEntries,
            ResourceLedgerEntries,
            ProjectPostingGroups,
        ],
        Menu(
            "projects-service",
            "Menu:Service",
            ServiceBoard,
            ServiceOrders,
            ServiceContracts,
            ServiceItems,
            ServiceLedgerEntries,
            WarrantyLedgerEntries,
            ServiceOrderTypes,
            FaultCodes,
            ResolutionCodes,
            ServiceSetup
        )
    );

    private static readonly RoleCenterMenu HumanResources = MenuWithSubMenus(
        "human-resources",
        "Menu:HumanResources",
        [
            Employees,
            EmployeeAbsences,
            EmployeeLedgerEntries,
            EmploymentContracts,
            Qualifications,
        ],
        Menu(
            "hr-payroll",
            "Menu:Payroll",
            PayrollRuns,
            Payslips,
            EmployeePayItems,
            PayrollEarnings,
            PayrollDeductions,
            PayrollTaxBands,
            PayrollSetup
        ),
        Menu(
            "hr-setup",
            "Menu:HumanResourcesSetup",
            CausesOfAbsence,
            Unions,
            GroundsForTermination,
            HrUnitsOfMeasure,
            EmployeePostingGroups,
            EmployeeStatisticsGroups,
            Relatives,
            EmployeeRelatives,
            EmployeeQualifications,
            MiscArticles,
            MiscArticleInformations,
            Confidentials,
            ConfidentialInformations,
            AlternativeAddresses,
            HumanResourcesSetup
        )
    );

    private static readonly RoleCenterMenu Pensions = Menu(
        "pensions",
        "Menu:Pensions",
        PensionMembers,
        PensionSponsors,
        PensionContributions,
        MemberExits,
        MemberLedgerEntries,
        Pensioners,
        PensionPayrolls,
        PensionBenefitCalculations,
        PensionSchemes,
        PensionInterestRates,
        ExitReasons,
        PensionSetup
    );

    private static readonly RoleCenterMenu Academics = MenuWithSubMenus(
        "academics",
        "Menu:Academics",
        [
            Students,
            StudentApplications,
            SemesterRegistrations,
            StudentUnits,
            StudentBills,
            StudentReceipts,
            ExamResults,
        ],
        Menu(
            "academics-campus",
            "Menu:CampusServices",
            Timetable,
            AttendanceRegisters,
            HostelAllocations,
            StudentRefunds
        ),
        Menu(
            "academics-config",
            "Menu:AcademicConfiguration",
            Programmes,
            CourseUnits,
            AcademicYears,
            Semesters,
            FeeStructure,
            AcademicSetup
        )
    );

    private static readonly RoleCenterMenu Reports = Menu(
        "reports",
        "Menu:Reports",
        FinancialReports,
        StandardReports,
        AccountSchedules,
        ColumnLayouts,
        ReportLayouts,
        ReportSelections,
        CustomReportSelections
    );

    private static readonly RoleCenterMenu Approvals = Menu(
        "approvals",
        "Module:Approvals",
        RequestsToApprove,
        Workflows,
        ApprovalUserSetup,
        WorkflowUserGroups,
        WorkflowUserGroupMembers
    );

    private static readonly RoleCenterMenu Integration = Menu(
        "integration",
        "Menu:Integration",
        WebServices,
        Webhooks,
        DataExport,
        ConfigPackages,
        ConfigWorksheet,
        ConfigTemplates,
        ImportData
    );

    private static readonly RoleCenterMenu Setup = MenuWithSubMenus(
        "setup",
        "Menu:Setup",
        [
            GeneralLedgerSetup,
            Dimensions,
            InventorySetup,
            HumanResourcesSetup,
            NoSeries,
            UserSetups,
            JobQueue,
            Profiles,
            Modules,
            Theme,
        ],
        Approvals,
        PostingSetup,
        Integration
    );

    public static readonly IReadOnlyList<RoleCenterDefinition> All =
    [
        new(
            ErpProfileIds.BusinessManager,
            [
                Link(Customers),
                Link(Vendors),
                Link(Items),
                Link(BankAccounts),
                Link(ChartOfAccounts),
                Finance,
                CashManagement,
                Sales,
                Purchasing,
                Inventory,
                ProjectsAndService,
                HumanResources,
                Pensions,
                Academics,
                Approvals,
                Reports,
                Setup,
            ]
        ),
        new(
            ErpProfileIds.Accountant,
            [
                Link(ChartOfAccounts),
                Link(BankAccounts),
                Link(Customers),
                Link(Vendors),
                Finance,
                Journals,
                CashManagement,
                PeriodicActivities,
                PostingSetup,
                Reports,
            ]
        ),
        new(
            ErpProfileIds.SalesOrderProcessor,
            [Link(Customers), Link(Items), Link(SalesInvoices), Sales, Inventory, ProjectsAndService, Approvals, Reports]
        ),
        new(
            ErpProfileIds.PurchasingAgent,
            [Link(Vendors), Link(Items), Link(PurchaseInvoices), Purchasing, Inventory, Approvals, Reports]
        ),
        new(ErpProfileIds.HumanResourcesManager, [Link(Employees), HumanResources, Pensions, Approvals]),
        new(
            ErpProfileIds.Administrator,
            [Link(Modules), Link(Profiles), Setup, PostingSetup, Integration, Finance, Reports]
        ),
    ];

    public static RoleCenterDefinition Find(string profileId) =>
        All.FirstOrDefault(d => d.ProfileId == profileId);

    public static bool Exists(string profileId) => Find(profileId) != null;
}
