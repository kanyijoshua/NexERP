using ABPmicroservice.MultiTenancy;
using ABPmicroservice.WebApp.Localization;
using Volo.Abp.Identity.Blazor;
using Volo.Abp.SettingManagement.Blazor.Menus;
using Volo.Abp.TenantManagement.Blazor.Navigation;
using Volo.Abp.UI.Navigation;

namespace ABPmicroservice.WebApp.Blazor.Client.Menus;

public class WebAppMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            await ConfigureMainMenuAsync(context);
        }
    }

    private static Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        var l = context.GetLocalizer<WebAppResource>();

        context.Menu.Items.Insert(
            0,
            new ApplicationMenuItem(WebAppMenus.Home, l["Menu:Home"], "/", icon: "fas fa-home")
        );

        var erpMenu = new ApplicationMenuItem(
            WebAppMenus.Erp,
            "Business Central ERP",
            icon: "fas fa-calculator"
        );

        erpMenu.AddItem(new ApplicationMenuItem(
            WebAppMenus.ChartOfAccounts,
            "Chart of Accounts",
            "/erp/chart-of-accounts",
            icon: "fas fa-list"
        ));

        erpMenu.AddItem(new ApplicationMenuItem(
            WebAppMenus.SalesInvoices,
            "Sales Invoices",
            "/erp/sales-invoices",
            icon: "fas fa-file-invoice-dollar"
        ));

        erpMenu.AddItem(new ApplicationMenuItem(
            WebAppMenus.PurchaseInvoices,
            "Purchase Invoices",
            "/erp/purchase-invoices",
            icon: "fas fa-shopping-cart"
        ));

        erpMenu.AddItem(new ApplicationMenuItem(
            WebAppMenus.FinancialReports,
            "Financial Reports",
            "/erp/financial-reports",
            icon: "fas fa-chart-bar"
        ));

        context.Menu.AddItem(erpMenu);

        var administration = context.Menu.GetAdministration();

        if (MultiTenancyConsts.IsEnabled)
        {
            administration.SetSubItemOrder(TenantManagementMenuNames.GroupName, 1);
        }
        else
        {
#pragma warning disable CS0162 // Unreachable code detected
            administration.TryRemoveMenuItem(TenantManagementMenuNames.GroupName);
#pragma warning restore CS0162 // Unreachable code detected
        }

        administration.SetSubItemOrder(IdentityMenuNames.GroupName, 2);
        administration.SetSubItemOrder(SettingManagementMenus.GroupName, 3);

        return Task.CompletedTask;
    }
}
