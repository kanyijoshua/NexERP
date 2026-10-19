using Volo.Abp.Authorization.Permissions;

namespace ABPmicroservice.Erp.Permissions;

public partial class ErpPermissionDefinitionProvider
{
    private static void DefineFinanceOperations(PermissionGroupDefinition group)
    {
        group.AddPermission(ErpPermissions.EntryApplication.Default, L("Permission:Erp:EntryApplication"))
            .AddChild(ErpPermissions.EntryApplication.Unapply, L("Permission:Erp:EntryApplication:Unapply"));
        group.GetPermissionOrNull(ErpPermissions.BankReconciliations.Default)
            .AddChild(ErpPermissions.BankReconciliationPosting.Default, L("Permission:Erp:BankReconciliations:Post"));

        AddCrud(group, ErpPermissions.Reminders.Default, ErpPermissions.Reminders.Create, ErpPermissions.Reminders.Update, ErpPermissions.Reminders.Delete, "Reminders");
        group.GetPermissionOrNull(ErpPermissions.Reminders.Default).AddChild(ErpPermissions.Reminders.Issue, L("Permission:Erp:Reminders:Issue"));
        AddCrud(group, ErpPermissions.FinanceChargeMemos.Default, ErpPermissions.FinanceChargeMemos.Create, ErpPermissions.FinanceChargeMemos.Update, ErpPermissions.FinanceChargeMemos.Delete, "FinanceChargeMemos");
        group.GetPermissionOrNull(ErpPermissions.FinanceChargeMemos.Default).AddChild(ErpPermissions.FinanceChargeMemos.Issue, L("Permission:Erp:FinanceChargeMemos:Issue"));
        AddCrud(group, ErpPermissions.CollectionSetup.Default, ErpPermissions.CollectionSetup.Create, ErpPermissions.CollectionSetup.Update, ErpPermissions.CollectionSetup.Delete, "CollectionSetup");
        AddCrud(group, ErpPermissions.Deferrals.Default, ErpPermissions.Deferrals.Create, ErpPermissions.Deferrals.Update, ErpPermissions.Deferrals.Delete, "Deferrals");
        AddCrud(group, ErpPermissions.CashFlow.Default, ErpPermissions.CashFlow.Create, ErpPermissions.CashFlow.Update, ErpPermissions.CashFlow.Delete, "CashFlow");
        group.GetPermissionOrNull(ErpPermissions.CashFlow.Default).AddChild(ErpPermissions.CashFlow.Register, L("Permission:Erp:CashFlow:Register"));
        AddCrud(group, ErpPermissions.Service.Default, ErpPermissions.Service.Create, ErpPermissions.Service.Update, ErpPermissions.Service.Delete, "Service");
        group.GetPermissionOrNull(ErpPermissions.Service.Default).AddChild(ErpPermissions.Service.Post, L("Permission:Erp:Service:Post"));
        AddCrud(group, ErpPermissions.ServiceSetup.Default, ErpPermissions.ServiceSetup.Create, ErpPermissions.ServiceSetup.Update, ErpPermissions.ServiceSetup.Delete, "ServiceSetup");
        AddCrud(group, ErpPermissions.Manufacturing.Default, ErpPermissions.Manufacturing.Create, ErpPermissions.Manufacturing.Update, ErpPermissions.Manufacturing.Delete, "Manufacturing");
        group.GetPermissionOrNull(ErpPermissions.Manufacturing.Default).AddChild(ErpPermissions.Manufacturing.Post, L("Permission:Erp:Manufacturing:Post"));
        AddCrud(group, ErpPermissions.ManufacturingSetup.Default, ErpPermissions.ManufacturingSetup.Create, ErpPermissions.ManufacturingSetup.Update, ErpPermissions.ManufacturingSetup.Delete, "ManufacturingSetup");
        AddCrud(group, ErpPermissions.Projects.Default, ErpPermissions.Projects.Create, ErpPermissions.Projects.Update, ErpPermissions.Projects.Delete, "Projects");
        group.GetPermissionOrNull(ErpPermissions.Projects.Default).AddChild(ErpPermissions.Projects.Post, L("Permission:Erp:Projects:Post"));
        AddCrud(group, ErpPermissions.TimeSheets.Default, ErpPermissions.TimeSheets.Create, ErpPermissions.TimeSheets.Update, ErpPermissions.TimeSheets.Delete, "TimeSheets");
        group.GetPermissionOrNull(ErpPermissions.TimeSheets.Default).AddChild(ErpPermissions.TimeSheets.Approve, L("Permission:Erp:TimeSheets:Approve"));
        AddCrud(group, ErpPermissions.ProjectsSetup.Default, ErpPermissions.ProjectsSetup.Create, ErpPermissions.ProjectsSetup.Update, ErpPermissions.ProjectsSetup.Delete, "ProjectsSetup");
        AddCrud(group, ErpPermissions.Crm.Default, ErpPermissions.Crm.Create, ErpPermissions.Crm.Update, ErpPermissions.Crm.Delete, "Crm");
        AddCrud(group, ErpPermissions.CrmSetup.Default, ErpPermissions.CrmSetup.Create, ErpPermissions.CrmSetup.Update, ErpPermissions.CrmSetup.Delete, "CrmSetup");
        group.AddPermission(ErpPermissions.FAJournals.Default, L("Permission:Erp:FAJournals"))
            .AddChild(ErpPermissions.FAJournals.Post, L("Permission:Erp:FAJournals:Post"));
    }
}
