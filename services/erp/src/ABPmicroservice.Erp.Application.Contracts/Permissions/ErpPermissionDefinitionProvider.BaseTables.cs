using Volo.Abp.Authorization.Permissions;

namespace ABPmicroservice.Erp.Permissions;

public partial class ErpPermissionDefinitionProvider
{
    private static void DefineBaseTables(PermissionGroupDefinition group)
    {
        AddCrud(group, ErpPermissions.FixedAssets.Default, ErpPermissions.FixedAssets.Create, ErpPermissions.FixedAssets.Update, ErpPermissions.FixedAssets.Delete, "FixedAssets");
        AddCrud(group, ErpPermissions.FixedAssetSetup.Default, ErpPermissions.FixedAssetSetup.Create, ErpPermissions.FixedAssetSetup.Update, ErpPermissions.FixedAssetSetup.Delete, "FixedAssetSetup");
        AddCrud(group, ErpPermissions.Budgets.Default, ErpPermissions.Budgets.Create, ErpPermissions.Budgets.Update, ErpPermissions.Budgets.Delete, "Budgets");
        AddCrud(group, ErpPermissions.BankReconciliations.Default, ErpPermissions.BankReconciliations.Create, ErpPermissions.BankReconciliations.Update, ErpPermissions.BankReconciliations.Delete, "BankReconciliations");
        AddCrud(group, ErpPermissions.UserSetup.Default, ErpPermissions.UserSetup.Create, ErpPermissions.UserSetup.Update, ErpPermissions.UserSetup.Delete, "UserSetup");
        AddCrud(group, ErpPermissions.Comments.Default, ErpPermissions.Comments.Create, ErpPermissions.Comments.Update, ErpPermissions.Comments.Delete, "Comments");
        AddCrud(group, ErpPermissions.ReportSelections.Default, ErpPermissions.ReportSelections.Create, ErpPermissions.ReportSelections.Update, ErpPermissions.ReportSelections.Delete, "ReportSelections");
        AddCrud(group, ErpPermissions.WorkflowUserGroups.Default, ErpPermissions.WorkflowUserGroups.Create, ErpPermissions.WorkflowUserGroups.Update, ErpPermissions.WorkflowUserGroups.Delete, "WorkflowUserGroups");
        AddCrud(group, ErpPermissions.ApprovalComments.Default, ErpPermissions.ApprovalComments.Create, ErpPermissions.ApprovalComments.Update, ErpPermissions.ApprovalComments.Delete, "ApprovalComments");
    }
}
