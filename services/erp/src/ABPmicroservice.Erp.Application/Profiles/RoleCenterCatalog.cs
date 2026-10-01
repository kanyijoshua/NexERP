using System.Collections.Generic;
using System.Linq;
using ABPmicroservice.Erp.Modules;
using ABPmicroservice.Erp.Permissions;

namespace ABPmicroservice.Erp.Profiles;

/// <summary>A link of a role center: where it goes, and what opening it takes.</summary>
public sealed record RoleCenterLink(string NameKey, string Route, string Module, params string[] Permissions);

/// <summary>A top-level entry of a role center's navigation bar: a menu of links, or one link.</summary>
public sealed record RoleCenterMenu(string Key, string NameKey, RoleCenterLink Link, IReadOnlyList<RoleCenterLink> Links);

/// <summary>A profile and the navigation bar of its role center.</summary>
public sealed record RoleCenterDefinition(string ProfileId, IReadOnlyList<RoleCenterMenu> Navigation)
{
    public string NameKey => $"Profile:{ProfileId}";

    public string DescriptionKey => $"Profile:{ProfileId}:Description";
}

/// <summary>
/// The profiles and their role centers. Mirrors the navigation Business Central's role centers
/// ship with (Business Manager page 9022, Accountant 9027, Order Processor 9006, Purchasing Agent
/// 9007, Administrator 9018): a few direct links to the lists used every day, then a menu per area.
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

    // Documents
    private static readonly RoleCenterLink SalesInvoices = new("Menu:SalesInvoices", "/erp/sales-invoices", ErpModuleRegistry.Sales, ErpPermissions.SalesDocuments.Default);
    private static readonly RoleCenterLink PurchaseInvoices = new("Menu:PurchaseInvoices", "/erp/purchase-invoices", ErpModuleRegistry.Purchasing, ErpPermissions.PurchaseDocuments.Default);
    private static readonly RoleCenterLink RequestsToApprove = new("Menu:RequestsToApprove", "/erp/approvals", ErpModuleRegistry.Approvals, ErpPermissions.Workflows.Default);

    // Ledgers
    private static readonly RoleCenterLink CustomerLedgerEntries = new("Menu:CustomerLedgerEntries", "/erp/finance/customer-ledger-entries", ErpModuleRegistry.Sales, ErpPermissions.Customers.Default);
    private static readonly RoleCenterLink VendorLedgerEntries = new("Menu:VendorLedgerEntries", "/erp/finance/vendor-ledger-entries", ErpModuleRegistry.Purchasing, ErpPermissions.Vendors.Default);
    private static readonly RoleCenterLink BankLedgerEntries = new("Menu:BankAccountLedgerEntries", "/erp/finance/bank-ledger-entries", ErpModuleRegistry.CashManagement, ErpPermissions.BankAccounts.Default);
    private static readonly RoleCenterLink EmployeeLedgerEntries = new("Menu:EmployeeLedgerEntries", "/erp/finance/employee-ledger-entries", ErpModuleRegistry.HumanResources, ErpPermissions.Employees.Default);
    private static readonly RoleCenterLink VatEntries = new("Menu:VatEntries", "/erp/finance/vat-entries", ErpModuleRegistry.Finance, ErpPermissions.VatEntries.Default);
    private static readonly RoleCenterLink GLRegisters = new("Menu:GLRegisters", "/erp/finance/registers", ErpModuleRegistry.Finance, ErpPermissions.GLRegisters.Default);

    // Journals and period end
    private static readonly RoleCenterLink GeneralJournal = new("Menu:GeneralJournal", "/erp/finance/general-journal", ErpModuleRegistry.Finance, ErpPermissions.Journals.Default);
    private static readonly RoleCenterLink JournalTemplates = new("Menu:JournalTemplates", "/erp/finance/journal-templates", ErpModuleRegistry.Finance, ErpPermissions.Journals.Default);
    private static readonly RoleCenterLink VatStatement = new("Menu:VatStatement", "/erp/finance/vat-return", ErpModuleRegistry.Finance, ErpPermissions.VatEntries.Default);
    private static readonly RoleCenterLink VatSettlement = new("Menu:VatSettlement", "/erp/finance/vat-settlement", ErpModuleRegistry.Finance, ErpPermissions.PeriodicActivities.SettleVat);
    private static readonly RoleCenterLink AdjustExchangeRates = new("Menu:AdjustExchangeRates", "/erp/finance/exch-rate-adjustment", ErpModuleRegistry.Finance, ErpPermissions.PeriodicActivities.Default);
    private static readonly RoleCenterLink AccountingPeriods = new("Menu:AccountingPeriods", "/erp/finance/accounting-periods", ErpModuleRegistry.Finance, ErpPermissions.FinanceSetup.Default);

    // Finance setup
    private static readonly RoleCenterLink Currencies = new("Menu:Currencies", "/erp/currencies", ErpModuleRegistry.Finance, ErpPermissions.FinanceSetup.Default);
    private static readonly RoleCenterLink CurrencyExchangeRates = new("Menu:CurrencyExchangeRates", "/erp/currency-exchange-rates", ErpModuleRegistry.Finance, ErpPermissions.FinanceSetup.Default);
    private static readonly RoleCenterLink PaymentTerms = new("Menu:PaymentTerms", "/erp/payment-terms", ErpModuleRegistry.Finance, ErpPermissions.FinanceSetup.Default);
    private static readonly RoleCenterLink PaymentMethods = new("Menu:PaymentMethods", "/erp/payment-methods", ErpModuleRegistry.Finance, ErpPermissions.FinanceSetup.Default);
    private static readonly RoleCenterLink GeneralLedgerSetup = new("Menu:GeneralLedgerSetup", "/erp/setup/general-ledger", ErpModuleRegistry.Finance, ErpPermissions.GeneralLedgerSetup.Default);
    private static readonly RoleCenterLink GeneralPostingSetup = new("Menu:GeneralPostingSetup", "/erp/general-posting-setup", ErpModuleRegistry.Finance, ErpPermissions.PostingSetup.Default);
    private static readonly RoleCenterLink VatPostingSetup = new("Menu:VatPostingSetup", "/erp/vat-posting-setup", ErpModuleRegistry.Finance, ErpPermissions.PostingSetup.Default);
    private static readonly RoleCenterLink CustomerPostingGroups = new("Menu:CustomerPostingGroups", "/erp/customer-posting-groups", ErpModuleRegistry.Sales, ErpPermissions.PostingSetup.Default);
    private static readonly RoleCenterLink VendorPostingGroups = new("Menu:VendorPostingGroups", "/erp/vendor-posting-groups", ErpModuleRegistry.Purchasing, ErpPermissions.PostingSetup.Default);
    private static readonly RoleCenterLink InventoryPostingSetup = new("Menu:InventoryPostingSetup", "/erp/inventory-posting-setup", ErpModuleRegistry.Inventory, ErpPermissions.PostingSetup.Default);
    private static readonly RoleCenterLink BankAccountPostingGroups = new("Menu:BankAccountPostingGroups", "/erp/bank-account-posting-groups", ErpModuleRegistry.CashManagement, ErpPermissions.PostingSetup.Default);

    // Sales, purchasing and inventory lists
    private static readonly RoleCenterLink SalespeoplePurchasers = new("Menu:SalespeoplePurchasers", "/erp/salespeople-purchasers", ErpModuleRegistry.Sales, ErpPermissions.SalespeoplePurchasers.Default);
    private static readonly RoleCenterLink ItemCategories = new("Menu:ItemCategories", "/erp/item-categories", ErpModuleRegistry.Inventory, ErpPermissions.ItemCategories.Default);
    private static readonly RoleCenterLink UnitsOfMeasure = new("Menu:UnitsOfMeasure", "/erp/units-of-measure", ErpModuleRegistry.Inventory, ErpPermissions.UnitsOfMeasure.Default);
    private static readonly RoleCenterLink Locations = new("Menu:Locations", "/erp/locations", ErpModuleRegistry.Inventory, ErpPermissions.Locations.Default);
    private static readonly RoleCenterLink InventorySetup = new("Menu:InventorySetup", "/erp/setup/inventory", ErpModuleRegistry.Inventory, ErpPermissions.InventorySetup.Default);

    // Human resources
    private static readonly RoleCenterLink EmployeeAbsences = new("Menu:EmployeeAbsences", "/erp/employee-absences", ErpModuleRegistry.HumanResources, ErpPermissions.Employees.Default);
    private static readonly RoleCenterLink CausesOfAbsence = new("Menu:CausesOfAbsence", "/erp/causes-of-absence", ErpModuleRegistry.HumanResources, ErpPermissions.HumanResourcesSetup.Default);
    private static readonly RoleCenterLink Qualifications = new("Menu:Qualifications", "/erp/qualifications", ErpModuleRegistry.HumanResources, ErpPermissions.HumanResourcesSetup.Default);
    private static readonly RoleCenterLink EmploymentContracts = new("Menu:EmploymentContracts", "/erp/employment-contracts", ErpModuleRegistry.HumanResources, ErpPermissions.HumanResourcesSetup.Default);
    private static readonly RoleCenterLink Unions = new("Menu:Unions", "/erp/unions", ErpModuleRegistry.HumanResources, ErpPermissions.HumanResourcesSetup.Default);
    private static readonly RoleCenterLink GroundsForTermination = new("Menu:GroundsForTermination", "/erp/grounds-for-termination", ErpModuleRegistry.HumanResources, ErpPermissions.HumanResourcesSetup.Default);
    private static readonly RoleCenterLink EmployeePostingGroups = new("Menu:EmployeePostingGroups", "/erp/employee-posting-groups", ErpModuleRegistry.HumanResources, ErpPermissions.HumanResourcesSetup.Default);
    private static readonly RoleCenterLink HumanResourcesSetup = new("Menu:HumanResourcesSetup", "/erp/setup/human-resources", ErpModuleRegistry.HumanResources, ErpPermissions.HumanResourcesSetup.Default);

    // Reports
    private static readonly RoleCenterLink FinancialReports = new("Menu:FinancialReports", "/erp/reports/financial", ErpModuleRegistry.Reporting, ErpPermissions.Reports.Default);
    private static readonly RoleCenterLink AccountSchedules = new("Menu:AccountSchedules", "/erp/reports/account-schedules", ErpModuleRegistry.Reporting, ErpPermissions.AccountSchedules.Default);
    private static readonly RoleCenterLink ColumnLayouts = new("Menu:ColumnLayouts", "/erp/reports/column-layouts", ErpModuleRegistry.Reporting, ErpPermissions.AccountSchedules.Default);
    private static readonly RoleCenterLink ReportLayouts = new("Menu:ReportLayouts", "/erp/reports/report-layouts", ErpModuleRegistry.Reporting, ErpPermissions.ReportLayouts.Default);

    // Administration
    private static readonly RoleCenterLink Modules = new("Menu:Modules", "/erp/setup/modules", ErpModuleRegistry.Core, ErpPermissions.Modules.Default);
    private static readonly RoleCenterLink Theme = new("Menu:Theme", "/erp/setup/theme", ErpModuleRegistry.Core, ErpPermissions.Theme.Default);
    private static readonly RoleCenterLink NoSeries = new("Menu:NoSeries", "/erp/setup/no-series", ErpModuleRegistry.Core, ErpPermissions.NoSeries.Default);
    private static readonly RoleCenterLink Profiles = new("Menu:Profiles", "/erp/setup/profiles", ErpModuleRegistry.Core, ErpPermissions.Profiles.Default);
    private static readonly RoleCenterLink Workflows = new("Menu:Workflows", "/erp/setup/workflows", ErpModuleRegistry.Approvals, ErpPermissions.Workflows.Manage);
    private static readonly RoleCenterLink ApprovalUserSetup = new("Menu:ApprovalUserSetup", "/erp/setup/approval-users", ErpModuleRegistry.Approvals, ErpPermissions.ApprovalUserSetup.Default);
    private static readonly RoleCenterLink WebServices = new("Menu:WebServices", "/erp/setup/web-services", ErpModuleRegistry.Integration, ErpPermissions.WebServices.Default, ErpPermissions.DataExport.Default);
    private static readonly RoleCenterLink Webhooks = new("Menu:Webhooks", "/erp/setup/webhooks", ErpModuleRegistry.Integration, ErpPermissions.Webhooks.Default, ErpPermissions.DataExport.Default);
    private static readonly RoleCenterLink DataExport = new("Menu:DataExport", "/erp/setup/data-export", ErpModuleRegistry.DataExport, ErpPermissions.DataExport.Default);
    private static readonly RoleCenterLink ConfigPackages = new("Menu:ConfigPackages", "/erp/rapid-start/packages", ErpModuleRegistry.RapidStart, ErpPermissions.RapidStart.Default);
    private static readonly RoleCenterLink ConfigWorksheet = new("Menu:ConfigWorksheet", "/erp/rapid-start/worksheet", ErpModuleRegistry.RapidStart, ErpPermissions.RapidStart.Default);
    private static readonly RoleCenterLink ImportData = new("Menu:ImportData", "/erp/rapid-start/import", ErpModuleRegistry.RapidStart, ErpPermissions.RapidStart.Apply);

    // Menus shared by several role centers
    private static RoleCenterMenu Link(RoleCenterLink link) => new(link.Route, link.NameKey, link, []);

    private static RoleCenterMenu Menu(string key, string nameKey, params RoleCenterLink[] links) => new(key, nameKey, null, links);

    private static readonly RoleCenterMenu Finance = Menu(
        "finance",
        "Menu:Finance",
        ChartOfAccounts,
        GLRegisters,
        VatEntries,
        AccountingPeriods,
        Currencies,
        CurrencyExchangeRates,
        PaymentTerms,
        GeneralLedgerSetup
    );

    private static readonly RoleCenterMenu Journals = Menu("journals", "Menu:Journals", GeneralJournal, JournalTemplates, GLRegisters);

    private static readonly RoleCenterMenu CashManagement = Menu(
        "cash-management",
        "Menu:CashManagement",
        BankAccounts,
        BankLedgerEntries,
        PaymentMethods,
        BankAccountPostingGroups
    );

    private static readonly RoleCenterMenu PeriodicActivities = Menu(
        "periodic-activities",
        "Menu:PeriodicActivities",
        VatStatement,
        VatSettlement,
        AdjustExchangeRates,
        AccountingPeriods
    );

    private static readonly RoleCenterMenu Sales = Menu(
        "sales",
        "Menu:Sales",
        Customers,
        SalesInvoices,
        CustomerLedgerEntries,
        SalespeoplePurchasers
    );

    private static readonly RoleCenterMenu Purchasing = Menu(
        "purchasing",
        "Menu:Purchasing",
        Vendors,
        PurchaseInvoices,
        VendorLedgerEntries
    );

    private static readonly RoleCenterMenu Inventory = Menu(
        "inventory",
        "Menu:Inventory",
        Items,
        ItemCategories,
        UnitsOfMeasure,
        Locations,
        InventorySetup
    );

    private static readonly RoleCenterMenu HumanResources = Menu(
        "human-resources",
        "Menu:HumanResources",
        Employees,
        EmployeeAbsences,
        EmployeeLedgerEntries,
        CausesOfAbsence,
        Qualifications,
        EmploymentContracts,
        Unions,
        GroundsForTermination,
        EmployeePostingGroups,
        HumanResourcesSetup
    );

    private static readonly RoleCenterMenu PostingSetup = Menu(
        "posting-setup",
        "Menu:PostingSetup",
        GeneralPostingSetup,
        VatPostingSetup,
        CustomerPostingGroups,
        VendorPostingGroups,
        InventoryPostingSetup,
        BankAccountPostingGroups
    );

    private static readonly RoleCenterMenu Reports = Menu(
        "reports",
        "Menu:Reports",
        FinancialReports,
        AccountSchedules,
        ColumnLayouts,
        ReportLayouts
    );

    private static readonly RoleCenterMenu Approvals = Menu("approvals", "Module:Approvals", RequestsToApprove);

    private static readonly RoleCenterMenu Setup = Menu(
        "setup",
        "Menu:Setup",
        GeneralLedgerSetup,
        InventorySetup,
        HumanResourcesSetup,
        NoSeries,
        Workflows,
        ApprovalUserSetup,
        Profiles,
        Modules,
        Theme
    );

    private static readonly RoleCenterMenu Integration = Menu(
        "integration",
        "Menu:Integration",
        WebServices,
        Webhooks,
        DataExport,
        ConfigPackages,
        ConfigWorksheet,
        ImportData
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
                Approvals,
                Reports,
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
            [Link(Customers), Link(Items), Link(SalesInvoices), Sales, Inventory, Approvals, Reports]
        ),
        new(
            ErpProfileIds.PurchasingAgent,
            [Link(Vendors), Link(Items), Link(PurchaseInvoices), Purchasing, Inventory, Approvals, Reports]
        ),
        new(ErpProfileIds.HumanResourcesManager, [Link(Employees), HumanResources, Approvals]),
        new(
            ErpProfileIds.Administrator,
            [Link(Modules), Link(Profiles), Setup, PostingSetup, Integration, Finance, Reports]
        ),
    ];

    public static RoleCenterDefinition Find(string profileId) =>
        All.FirstOrDefault(d => d.ProfileId == profileId);

    public static bool Exists(string profileId) => Find(profileId) != null;
}
