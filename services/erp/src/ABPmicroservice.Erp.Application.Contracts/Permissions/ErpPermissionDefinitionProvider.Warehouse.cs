using Volo.Abp.Authorization.Permissions;

namespace ABPmicroservice.Erp.Permissions;

public partial class ErpPermissionDefinitionProvider
{
    private static void DefineWarehouse(PermissionGroupDefinition group)
    {
        AddCrud(group, ErpPermissions.Warehouse.Default, ErpPermissions.Warehouse.Create, ErpPermissions.Warehouse.Update, ErpPermissions.Warehouse.Delete, "Warehouse");
        group.GetPermissionOrNull(ErpPermissions.Warehouse.Default).AddChild(ErpPermissions.Warehouse.Register, L("Permission:Erp:Warehouse:Register"));
        group.AddPermission(ErpPermissions.Reservations.Default, L("Permission:Erp:Reservations"))
            .AddChild(ErpPermissions.Reservations.Reserve, L("Permission:Erp:Reservations:Reserve"));
        AddCrud(group, ErpPermissions.PhysInventory.Default, ErpPermissions.PhysInventory.Create, ErpPermissions.PhysInventory.Update, ErpPermissions.PhysInventory.Delete, "PhysInventory");
        group.GetPermissionOrNull(ErpPermissions.PhysInventory.Default).AddChild(ErpPermissions.PhysInventory.Post, L("Permission:Erp:PhysInventory:Post"));
        AddCrud(group, ErpPermissions.Assembly.Default, ErpPermissions.Assembly.Create, ErpPermissions.Assembly.Update, ErpPermissions.Assembly.Delete, "Assembly");
        group.GetPermissionOrNull(ErpPermissions.Assembly.Default).AddChild(ErpPermissions.Assembly.Post, L("Permission:Erp:Assembly:Post"));
    }
}
