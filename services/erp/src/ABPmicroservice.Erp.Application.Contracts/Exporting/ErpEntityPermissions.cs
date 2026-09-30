using System;
using System.Collections.Generic;
using ABPmicroservice.Erp.Permissions;

namespace ABPmicroservice.Erp.Exporting;

/// <summary>
/// Which permission a caller needs before rows of a table may be exported, queried through the
/// integration API, or sent to a webhook subscriber.
/// <para>
/// The registry in the domain layer says what a table is; this says who may see it. Keeping them
/// apart is what lets the domain stay free of the permission constants, and it means a generic
/// export can never widen access: exporting customers needs the same permission as reading them
/// on screen.
/// </para>
/// <para>
/// A table with no entry here is not exportable at all. <c>ErpEntityPermissions_Tests</c> fails
/// if the registry gains one that was never given a permission, so the gap is caught in the build
/// rather than in production.
/// </para>
/// </summary>
public static class ErpEntityPermissions
{
    private static readonly Dictionary<string, string> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        // Finance
        ["GLAccount"] = ErpPermissions.GLAccounts.Default,
        ["GLEntry"] = ErpPermissions.GLEntries.Default,
        ["GLRegister"] = ErpPermissions.GLRegisters.Default,
        ["GeneralLedgerSetup"] = ErpPermissions.GeneralLedgerSetup.Default,
        ["GenBusinessPostingGroup"] = ErpPermissions.PostingSetup.Default,
        ["GenProductPostingGroup"] = ErpPermissions.PostingSetup.Default,
        ["GeneralPostingSetup"] = ErpPermissions.PostingSetup.Default,
        ["GenJournalTemplate"] = ErpPermissions.Journals.Default,
        ["GenJournalBatch"] = ErpPermissions.Journals.Default,
        ["GenJournalLine"] = ErpPermissions.Journals.Default,
        ["StandardGeneralJournal"] = ErpPermissions.Journals.Default,
        ["StandardGeneralJournalLine"] = ErpPermissions.Journals.Default,

        // Sales
        ["Customer"] = ErpPermissions.Customers.Default,
        ["CustomerPostingGroup"] = ErpPermissions.PostingSetup.Default,
        ["CustomerLedgerEntry"] = ErpPermissions.Customers.Default,
        ["SalesHeader"] = ErpPermissions.SalesDocuments.Default,
        ["SalesLine"] = ErpPermissions.SalesDocuments.Default,
        ["PostedSalesHeader"] = ErpPermissions.SalesDocuments.Default,
        ["PostedSalesLine"] = ErpPermissions.SalesDocuments.Default,
        ["SalesReceivablesSetup"] = ErpPermissions.SalesSetup.Default,

        // Purchasing
        ["Vendor"] = ErpPermissions.Vendors.Default,
        ["VendorPostingGroup"] = ErpPermissions.PostingSetup.Default,
        ["VendorLedgerEntry"] = ErpPermissions.Vendors.Default,
        ["PurchaseHeader"] = ErpPermissions.PurchaseDocuments.Default,
        ["PurchaseLine"] = ErpPermissions.PurchaseDocuments.Default,
        ["PostedPurchaseHeader"] = ErpPermissions.PurchaseDocuments.Default,
        ["PostedPurchaseLine"] = ErpPermissions.PurchaseDocuments.Default,
        ["PurchasesPayablesSetup"] = ErpPermissions.PurchaseSetup.Default,

        // Inventory
        ["Item"] = ErpPermissions.Items.Default,
        ["ItemCategory"] = ErpPermissions.ItemCategories.Default,
        ["UnitOfMeasure"] = ErpPermissions.UnitsOfMeasure.Default,
        ["InventoryPostingGroup"] = ErpPermissions.PostingSetup.Default,
        ["InventoryPostingSetup"] = ErpPermissions.PostingSetup.Default,
        ["ItemLedgerEntry"] = ErpPermissions.Items.Default,
        ["ValueEntry"] = ErpPermissions.Items.Default,

        // Dimensions
        ["Dimension"] = ErpPermissions.Dimensions.Default,
        ["DimensionValue"] = ErpPermissions.Dimensions.Default,
        ["DefaultDimension"] = ErpPermissions.Dimensions.Default,

        // Numbering and approvals
        ["NoSeries"] = ErpPermissions.NoSeries.Default,
        ["NoSeriesLine"] = ErpPermissions.NoSeries.Default,
        ["Workflow"] = ErpPermissions.Workflows.Default,
        ["ApprovalEntry"] = ErpPermissions.Workflows.Default,
        ["ApprovalUserSetup"] = ErpPermissions.ApprovalUserSetup.Default,

        // Reporting setup
        ["AccountSchedule"] = ErpPermissions.AccountSchedules.Default,
        ["AccountScheduleLine"] = ErpPermissions.AccountSchedules.Default,
        ["ColumnLayout"] = ErpPermissions.AccountSchedules.Default,
        ["ColumnLayoutLine"] = ErpPermissions.AccountSchedules.Default,

        // Tax, cash management, finance and human resources setup
        ["PaymentTerms"] = ErpPermissions.FinanceSetup.Default,
        ["Currency"] = ErpPermissions.FinanceSetup.Default,
        ["CurrencyExchangeRate"] = ErpPermissions.FinanceSetup.Default,
        ["AccountingPeriod"] = ErpPermissions.FinanceSetup.Default,
        ["VatBusinessPostingGroup"] = ErpPermissions.PostingSetup.Default,
        ["VatProductPostingGroup"] = ErpPermissions.PostingSetup.Default,
        ["VatPostingSetup"] = ErpPermissions.PostingSetup.Default,
        ["VatEntry"] = ErpPermissions.VatEntries.Default,
        ["Location"] = ErpPermissions.Locations.Default,
        ["InventorySetup"] = ErpPermissions.InventorySetup.Default,
        ["SalespersonPurchaser"] = ErpPermissions.SalespeoplePurchasers.Default,
        ["BankAccountPostingGroup"] = ErpPermissions.PostingSetup.Default,
        ["BankAccount"] = ErpPermissions.BankAccounts.Default,
        ["BankAccountLedgerEntry"] = ErpPermissions.BankAccounts.Default,
        ["EmployeeLedgerEntry"] = ErpPermissions.Employees.Default,
        ["ExchRateAdjmtRegister"] = ErpPermissions.PeriodicActivities.Default,
        ["PaymentMethod"] = ErpPermissions.FinanceSetup.Default,
        ["HumanResourcesSetup"] = ErpPermissions.HumanResourcesSetup.Default,
        ["HumanResourceUnitOfMeasure"] = ErpPermissions.HumanResourcesSetup.Default,
        ["EmployeePostingGroup"] = ErpPermissions.HumanResourcesSetup.Default,
        ["CauseOfAbsence"] = ErpPermissions.HumanResourcesSetup.Default,
        ["Qualification"] = ErpPermissions.HumanResourcesSetup.Default,
        ["Union"] = ErpPermissions.HumanResourcesSetup.Default,
        ["EmploymentContract"] = ErpPermissions.HumanResourcesSetup.Default,
        ["GroundsForTermination"] = ErpPermissions.HumanResourcesSetup.Default,
        ["Employee"] = ErpPermissions.Employees.Default,
        ["EmployeeAbsence"] = ErpPermissions.Employees.Default,

        // Collaboration
        ["KanbanStage"] = ErpPermissions.Kanban.Default,
        ["DocumentNote"] = ErpPermissions.Chatter.Default,
        ["ActivityStreamEntry"] = ErpPermissions.Chatter.Default,
        ["DocumentActivityTask"] = ErpPermissions.Chatter.Default,

        ["Company"] = ErpPermissions.Companies.Default,
    };

    /// <summary>
    /// What a caller needs, beyond reading a table, before a configuration package or a data
    /// import may write to it: the same permissions that guard creating and changing its records
    /// on screen, so an import can never do what the user could not do by hand.
    /// <c>ErpEntityPermissions_Tests</c> fails if a table that may be imported has no entry here.
    /// </summary>
    private static readonly Dictionary<string, string[]> WriteMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["GLAccount"] = [ErpPermissions.GLAccounts.Create, ErpPermissions.GLAccounts.Update],
        ["GeneralLedgerSetup"] = [ErpPermissions.GeneralLedgerSetup.Update],
        ["GenBusinessPostingGroup"] = [ErpPermissions.PostingSetup.Create, ErpPermissions.PostingSetup.Update],
        ["GenProductPostingGroup"] = [ErpPermissions.PostingSetup.Create, ErpPermissions.PostingSetup.Update],
        ["GeneralPostingSetup"] = [ErpPermissions.PostingSetup.Create, ErpPermissions.PostingSetup.Update],
        ["GenJournalTemplate"] = [ErpPermissions.Journals.Manage],
        ["GenJournalBatch"] = [ErpPermissions.Journals.Manage],

        ["Customer"] = [ErpPermissions.Customers.Create, ErpPermissions.Customers.Update],
        ["CustomerPostingGroup"] = [ErpPermissions.PostingSetup.Create, ErpPermissions.PostingSetup.Update],
        ["SalesReceivablesSetup"] = [ErpPermissions.SalesSetup.Update],

        ["Vendor"] = [ErpPermissions.Vendors.Create, ErpPermissions.Vendors.Update],
        ["VendorPostingGroup"] = [ErpPermissions.PostingSetup.Create, ErpPermissions.PostingSetup.Update],
        ["PurchasesPayablesSetup"] = [ErpPermissions.PurchaseSetup.Update],

        ["Item"] = [ErpPermissions.Items.Create, ErpPermissions.Items.Update],
        ["ItemCategory"] = [ErpPermissions.ItemCategories.Create, ErpPermissions.ItemCategories.Update],
        ["UnitOfMeasure"] = [ErpPermissions.UnitsOfMeasure.Create, ErpPermissions.UnitsOfMeasure.Update],
        ["InventoryPostingGroup"] = [ErpPermissions.PostingSetup.Create, ErpPermissions.PostingSetup.Update],
        ["InventoryPostingSetup"] = [ErpPermissions.PostingSetup.Create, ErpPermissions.PostingSetup.Update],

        ["Dimension"] = [ErpPermissions.Dimensions.Create, ErpPermissions.Dimensions.Update],
        ["DimensionValue"] = [ErpPermissions.Dimensions.Create, ErpPermissions.Dimensions.Update],

        ["NoSeries"] = [ErpPermissions.NoSeries.Create, ErpPermissions.NoSeries.Update],
        ["NoSeriesLine"] = [ErpPermissions.NoSeries.Create, ErpPermissions.NoSeries.Update],

        ["AccountSchedule"] = [ErpPermissions.AccountSchedules.Manage],
        ["AccountScheduleLine"] = [ErpPermissions.AccountSchedules.Manage],
        ["ColumnLayout"] = [ErpPermissions.AccountSchedules.Manage],
        ["ColumnLayoutLine"] = [ErpPermissions.AccountSchedules.Manage],

        ["KanbanStage"] = [ErpPermissions.Kanban.Manage],

        ["PaymentTerms"] = [ErpPermissions.FinanceSetup.Create, ErpPermissions.FinanceSetup.Update],
        ["Currency"] = [ErpPermissions.FinanceSetup.Create, ErpPermissions.FinanceSetup.Update],
        ["CurrencyExchangeRate"] = [ErpPermissions.FinanceSetup.Create, ErpPermissions.FinanceSetup.Update],
        ["AccountingPeriod"] = [ErpPermissions.FinanceSetup.Create, ErpPermissions.FinanceSetup.Update],
        ["VatBusinessPostingGroup"] = [ErpPermissions.PostingSetup.Create, ErpPermissions.PostingSetup.Update],
        ["VatProductPostingGroup"] = [ErpPermissions.PostingSetup.Create, ErpPermissions.PostingSetup.Update],
        ["VatPostingSetup"] = [ErpPermissions.PostingSetup.Create, ErpPermissions.PostingSetup.Update],
        ["Location"] = [ErpPermissions.Locations.Create, ErpPermissions.Locations.Update],
        ["InventorySetup"] = [ErpPermissions.InventorySetup.Update],
        ["SalespersonPurchaser"] = [ErpPermissions.SalespeoplePurchasers.Create, ErpPermissions.SalespeoplePurchasers.Update],
        ["BankAccountPostingGroup"] = [ErpPermissions.PostingSetup.Create, ErpPermissions.PostingSetup.Update],
        ["BankAccount"] = [ErpPermissions.BankAccounts.Create, ErpPermissions.BankAccounts.Update],
        ["PaymentMethod"] = [ErpPermissions.FinanceSetup.Create, ErpPermissions.FinanceSetup.Update],
        ["HumanResourcesSetup"] = [ErpPermissions.HumanResourcesSetup.Update],
        ["HumanResourceUnitOfMeasure"] = [ErpPermissions.HumanResourcesSetup.Create, ErpPermissions.HumanResourcesSetup.Update],
        ["EmployeePostingGroup"] = [ErpPermissions.HumanResourcesSetup.Create, ErpPermissions.HumanResourcesSetup.Update],
        ["CauseOfAbsence"] = [ErpPermissions.HumanResourcesSetup.Create, ErpPermissions.HumanResourcesSetup.Update],
        ["Qualification"] = [ErpPermissions.HumanResourcesSetup.Create, ErpPermissions.HumanResourcesSetup.Update],
        ["Union"] = [ErpPermissions.HumanResourcesSetup.Create, ErpPermissions.HumanResourcesSetup.Update],
        ["EmploymentContract"] = [ErpPermissions.HumanResourcesSetup.Create, ErpPermissions.HumanResourcesSetup.Update],
        ["GroundsForTermination"] = [ErpPermissions.HumanResourcesSetup.Create, ErpPermissions.HumanResourcesSetup.Update],
        ["Employee"] = [ErpPermissions.Employees.Create, ErpPermissions.Employees.Update],
        ["EmployeeAbsence"] = [ErpPermissions.Employees.Create, ErpPermissions.Employees.Update],
    };

    /// <summary>The permission guarding a table, or null when the table may not be exported.</summary>
    public static string Find(string entityName) => Map.GetValueOrDefault(entityName ?? string.Empty);

    /// <summary>
    /// Every permission needed to write to a table, the read permission included; null when the
    /// table may not be written through an import.
    /// </summary>
    public static string[] FindWrite(string entityName)
    {
        var read = Find(entityName);
        var write = WriteMap.GetValueOrDefault(entityName ?? string.Empty);

        return read == null || write == null ? null : [read, .. write];
    }
}
