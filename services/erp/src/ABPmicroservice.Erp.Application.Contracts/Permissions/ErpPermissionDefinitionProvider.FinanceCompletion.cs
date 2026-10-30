using Volo.Abp.Authorization.Permissions;

namespace ABPmicroservice.Erp.Permissions;

public partial class ErpPermissionDefinitionProvider
{
    private static void DefineFinanceCompletion(PermissionGroupDefinition group)
    {
        group.AddPermission(ErpPermissions.PaymentExport.Default, L("Permission:Erp:PaymentExport"));
        AddCrud(group, ErpPermissions.DirectDebits.Default, ErpPermissions.DirectDebits.Create, ErpPermissions.DirectDebits.Update, ErpPermissions.DirectDebits.Delete, "DirectDebits");
        group.AddPermission(ErpPermissions.InventoryCosting.Default, L("Permission:Erp:InventoryCosting"));
        AddCrud(group, ErpPermissions.Consolidation.Default, ErpPermissions.Consolidation.Create, ErpPermissions.Consolidation.Update, ErpPermissions.Consolidation.Delete, "Consolidation");
        group.GetPermissionOrNull(ErpPermissions.Consolidation.Default).AddChild(ErpPermissions.Consolidation.Run, L("Permission:Erp:Consolidation:Run"));
        AddCrud(group, ErpPermissions.Intercompany.Default, ErpPermissions.Intercompany.Create, ErpPermissions.Intercompany.Update, ErpPermissions.Intercompany.Delete, "Intercompany");
        group.GetPermissionOrNull(ErpPermissions.Intercompany.Default).AddChild(ErpPermissions.Intercompany.Process, L("Permission:Erp:Intercompany:Process"));
        AddCrud(group, ErpPermissions.CostAccounting.Default, ErpPermissions.CostAccounting.Create, ErpPermissions.CostAccounting.Update, ErpPermissions.CostAccounting.Delete, "CostAccounting");
        group.GetPermissionOrNull(ErpPermissions.CostAccounting.Default).AddChild(ErpPermissions.CostAccounting.Post, L("Permission:Erp:CostAccounting:Post"));
        AddCrud(group, ErpPermissions.StandardCodes.Default, ErpPermissions.StandardCodes.Create, ErpPermissions.StandardCodes.Update, ErpPermissions.StandardCodes.Delete, "StandardCodes");
    }
}
