using ABPmicroservice.Erp.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace ABPmicroservice.Erp.Permissions;

public class ErpPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var erpGroup = context.AddGroup(ErpPermissions.GroupName, L("Permission:Erp"));

        AddCrud(
            erpGroup,
            ErpPermissions.Items.Default,
            ErpPermissions.Items.Create,
            ErpPermissions.Items.Update,
            ErpPermissions.Items.Delete,
            "Items"
        );
        AddCrud(
            erpGroup,
            ErpPermissions.ItemCategories.Default,
            ErpPermissions.ItemCategories.Create,
            ErpPermissions.ItemCategories.Update,
            ErpPermissions.ItemCategories.Delete,
            "ItemCategories"
        );
        AddCrud(
            erpGroup,
            ErpPermissions.UnitsOfMeasure.Default,
            ErpPermissions.UnitsOfMeasure.Create,
            ErpPermissions.UnitsOfMeasure.Update,
            ErpPermissions.UnitsOfMeasure.Delete,
            "UnitsOfMeasure"
        );
        AddCrud(
            erpGroup,
            ErpPermissions.Customers.Default,
            ErpPermissions.Customers.Create,
            ErpPermissions.Customers.Update,
            ErpPermissions.Customers.Delete,
            "Customers"
        );
        AddCrud(
            erpGroup,
            ErpPermissions.Vendors.Default,
            ErpPermissions.Vendors.Create,
            ErpPermissions.Vendors.Update,
            ErpPermissions.Vendors.Delete,
            "Vendors"
        );
        AddCrud(
            erpGroup,
            ErpPermissions.GLAccounts.Default,
            ErpPermissions.GLAccounts.Create,
            ErpPermissions.GLAccounts.Update,
            ErpPermissions.GLAccounts.Delete,
            "GLAccounts"
        );

        erpGroup.AddPermission(ErpPermissions.GLEntries.Default, L("Permission:Erp:GLEntries"));

        var sales = erpGroup.AddPermission(
            ErpPermissions.SalesDocuments.Default,
            L("Permission:Erp:SalesDocuments")
        );
        sales.AddChild(ErpPermissions.SalesDocuments.Create, L("Permission:Erp:SalesDocuments:Create"));
        sales.AddChild(ErpPermissions.SalesDocuments.Update, L("Permission:Erp:SalesDocuments:Update"));
        sales.AddChild(ErpPermissions.SalesDocuments.Delete, L("Permission:Erp:SalesDocuments:Delete"));
        sales.AddChild(ErpPermissions.SalesDocuments.Post, L("Permission:Erp:SalesDocuments:Post"));

        var purchase = erpGroup.AddPermission(
            ErpPermissions.PurchaseDocuments.Default,
            L("Permission:Erp:PurchaseDocuments")
        );
        purchase.AddChild(
            ErpPermissions.PurchaseDocuments.Create,
            L("Permission:Erp:PurchaseDocuments:Create")
        );
        purchase.AddChild(
            ErpPermissions.PurchaseDocuments.Update,
            L("Permission:Erp:PurchaseDocuments:Update")
        );
        purchase.AddChild(
            ErpPermissions.PurchaseDocuments.Delete,
            L("Permission:Erp:PurchaseDocuments:Delete")
        );
        purchase.AddChild(
            ErpPermissions.PurchaseDocuments.Post,
            L("Permission:Erp:PurchaseDocuments:Post")
        );

        AddCrud(
            erpGroup,
            ErpPermissions.Dimensions.Default,
            ErpPermissions.Dimensions.Create,
            ErpPermissions.Dimensions.Update,
            ErpPermissions.Dimensions.Delete,
            "Dimensions"
        );

        var journals = erpGroup.AddPermission(ErpPermissions.Journals.Default, L("Permission:Erp:Journals"));
        journals.AddChild(ErpPermissions.Journals.Create, L("Permission:Erp:Journals:Create"));
        journals.AddChild(ErpPermissions.Journals.Delete, L("Permission:Erp:Journals:Delete"));
        journals.AddChild(ErpPermissions.Journals.Post, L("Permission:Erp:Journals:Post"));

        var workflows = erpGroup.AddPermission(ErpPermissions.Workflows.Default, L("Permission:Erp:Workflows"));
        workflows.AddChild(ErpPermissions.Workflows.Manage, L("Permission:Erp:Workflows:Manage"));
        workflows.AddChild(ErpPermissions.Workflows.Approve, L("Permission:Erp:Workflows:Approve"));

        var rapidStart = erpGroup.AddPermission(ErpPermissions.RapidStart.Default, L("Permission:Erp:RapidStart"));
        rapidStart.AddChild(ErpPermissions.RapidStart.Import, L("Permission:Erp:RapidStart:Import"));
        rapidStart.AddChild(ErpPermissions.RapidStart.Export, L("Permission:Erp:RapidStart:Export"));

        var reports = erpGroup.AddPermission(ErpPermissions.Reports.Default, L("Permission:Erp:Reports"));
        reports.AddChild(ErpPermissions.Reports.ExportExcel, L("Permission:Erp:Reports:ExportExcel"));

        var companies = erpGroup.AddPermission(ErpPermissions.Companies.Default, L("Permission:Erp:Companies"));
        companies.AddChild(ErpPermissions.Companies.Create, L("Permission:Erp:Companies:Create"));
        companies.AddChild(ErpPermissions.Companies.Update, L("Permission:Erp:Companies:Update"));
        companies.AddChild(ErpPermissions.Companies.Delete, L("Permission:Erp:Companies:Delete"));
        companies.AddChild(ErpPermissions.Companies.Copy, L("Permission:Erp:Companies:Copy"));

        var kanban = erpGroup.AddPermission(ErpPermissions.Kanban.Default, L("Permission:Erp:Kanban"));
        kanban.AddChild(ErpPermissions.Kanban.Manage, L("Permission:Erp:Kanban:Manage"));

        var chatter = erpGroup.AddPermission(ErpPermissions.Chatter.Default, L("Permission:Erp:Chatter"));
        chatter.AddChild(ErpPermissions.Chatter.Create, L("Permission:Erp:Chatter:Create"));

        var reportLayouts = erpGroup.AddPermission(ErpPermissions.ReportLayouts.Default, L("Permission:Erp:ReportLayouts"));
        reportLayouts.AddChild(ErpPermissions.ReportLayouts.Manage, L("Permission:Erp:ReportLayouts:Manage"));

        AddCrud(
            erpGroup,
            ErpPermissions.NoSeries.Default,
            ErpPermissions.NoSeries.Create,
            ErpPermissions.NoSeries.Update,
            ErpPermissions.NoSeries.Delete,
            "NoSeries"
        );
        AddCrud(
            erpGroup,
            ErpPermissions.ApprovalUserSetup.Default,
            ErpPermissions.ApprovalUserSetup.Create,
            ErpPermissions.ApprovalUserSetup.Update,
            ErpPermissions.ApprovalUserSetup.Delete,
            "ApprovalUserSetup"
        );

        var salesSetup = erpGroup.AddPermission(ErpPermissions.SalesSetup.Default, L("Permission:Erp:SalesSetup"));
        salesSetup.AddChild(ErpPermissions.SalesSetup.Update, L("Permission:Erp:SalesSetup:Update"));

        var purchaseSetup = erpGroup.AddPermission(ErpPermissions.PurchaseSetup.Default, L("Permission:Erp:PurchaseSetup"));
        purchaseSetup.AddChild(ErpPermissions.PurchaseSetup.Update, L("Permission:Erp:PurchaseSetup:Update"));
    }

    private static void AddCrud(
        PermissionGroupDefinition group,
        string defaultPermission,
        string create,
        string update,
        string delete,
        string name
    )
    {
        var permission = group.AddPermission(defaultPermission, L($"Permission:Erp:{name}"));
        permission.AddChild(create, L($"Permission:Erp:{name}:Create"));
        permission.AddChild(update, L($"Permission:Erp:{name}:Update"));
        permission.AddChild(delete, L($"Permission:Erp:{name}:Delete"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ErpResource>(name);
    }
}
