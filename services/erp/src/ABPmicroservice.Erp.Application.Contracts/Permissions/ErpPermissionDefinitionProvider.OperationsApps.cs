using Volo.Abp.Authorization.Permissions;

namespace ABPmicroservice.Erp.Permissions;

public partial class ErpPermissionDefinitionProvider
{
    private static void DefineOperationsApps(PermissionGroupDefinition group)
    {
        AddCrud(group, ErpPermissions.Recruitment.Default, ErpPermissions.Recruitment.Create, ErpPermissions.Recruitment.Update, ErpPermissions.Recruitment.Delete, "Recruitment");
        group.GetPermissionOrNull(ErpPermissions.Recruitment.Default).AddChild(ErpPermissions.Recruitment.Hire, L("Permission:Erp:Recruitment:Hire"));
        AddCrud(group, ErpPermissions.Appraisals.Default, ErpPermissions.Appraisals.Create, ErpPermissions.Appraisals.Update, ErpPermissions.Appraisals.Delete, "Appraisals");
        group.GetPermissionOrNull(ErpPermissions.Appraisals.Default).AddChild(ErpPermissions.Appraisals.Review, L("Permission:Erp:Appraisals:Review"));
        AddCrud(group, ErpPermissions.Subscriptions.Default, ErpPermissions.Subscriptions.Create, ErpPermissions.Subscriptions.Update, ErpPermissions.Subscriptions.Delete, "Subscriptions");
        group.GetPermissionOrNull(ErpPermissions.Subscriptions.Default).AddChild(ErpPermissions.Subscriptions.Invoice, L("Permission:Erp:Subscriptions:Invoice"));
        AddCrud(group, ErpPermissions.PointOfSale.Default, ErpPermissions.PointOfSale.Create, ErpPermissions.PointOfSale.Update, ErpPermissions.PointOfSale.Delete, "PointOfSale");
        group.GetPermissionOrNull(ErpPermissions.PointOfSale.Default).AddChild(ErpPermissions.PointOfSale.Sell, L("Permission:Erp:PointOfSale:Sell"));
        AddCrud(group, ErpPermissions.Maintenance.Default, ErpPermissions.Maintenance.Create, ErpPermissions.Maintenance.Update, ErpPermissions.Maintenance.Delete, "Maintenance");
        AddCrud(group, ErpPermissions.Quality.Default, ErpPermissions.Quality.Create, ErpPermissions.Quality.Update, ErpPermissions.Quality.Delete, "Quality");
        group.GetPermissionOrNull(ErpPermissions.Quality.Default).AddChild(ErpPermissions.Quality.Record, L("Permission:Erp:Quality:Record"));
        AddCrud(group, ErpPermissions.Fleet.Default, ErpPermissions.Fleet.Create, ErpPermissions.Fleet.Update, ErpPermissions.Fleet.Delete, "Fleet");
        AddCrud(group, ErpPermissions.Marketing.Default, ErpPermissions.Marketing.Create, ErpPermissions.Marketing.Update, ErpPermissions.Marketing.Delete, "Marketing");
        group.GetPermissionOrNull(ErpPermissions.Marketing.Default).AddChild(ErpPermissions.Marketing.Send, L("Permission:Erp:Marketing:Send"));
    }
}
