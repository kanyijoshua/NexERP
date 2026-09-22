using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;

namespace ABPmicroservice.Erp.Modules;

/// <summary>
/// One installable area of the system.
/// <para>
/// Odoo calls these apps and lets you install and uninstall them; Business Central calls the
/// equivalent extensions and features, and turns them on per company. This follows both: a module
/// is switched on or off for one company, and switching it off hides its screens and closes its
/// endpoints without touching a row of its data.
/// </para>
/// </summary>
public sealed class ErpModuleDefinition
{
    public ErpModuleDefinition(
        string code,
        string group,
        string icon,
        string route,
        bool isCore,
        IReadOnlyList<string> dependsOn,
        IReadOnlyList<string> areas
    )
    {
        Code = code;
        Group = group;
        Icon = icon;
        Route = route;
        IsCore = isCore;
        DependsOn = dependsOn;
        Areas = areas;
    }

    /// <summary>Stable key, also the localization key suffix: "Erp::Module:Sales".</summary>
    public string Code { get; }

    /// <summary>Heading the module sits under on the home page.</summary>
    public string Group { get; }

    public string Icon { get; }

    /// <summary>Where the module's tile goes. Null when the module has no screen of its own yet.</summary>
    public string Route { get; }

    /// <summary>A module the system cannot run without. It is always on and cannot be switched off.</summary>
    public bool IsCore { get; }

    public IReadOnlyList<string> DependsOn { get; }

    /// <summary>
    /// Namespaces under <c>ABPmicroservice.Erp</c> that belong to this module. This is what closes
    /// the module's endpoints when it is off, so every application service must fall under exactly
    /// one module; <c>ErpModuleRegistry_Tests</c> fails otherwise.
    /// </summary>
    public IReadOnlyList<string> Areas { get; }
}

/// <summary>The modules this system is made of, and what each one covers.</summary>
public static class ErpModuleRegistry
{
    public const string GroupFinance = "Finance";
    public const string GroupOperations = "Operations";
    public const string GroupCollaboration = "Collaboration";
    public const string GroupSystem = "System";

    public const string Core = "Core";
    public const string Finance = "Finance";
    public const string Sales = "Sales";
    public const string Purchasing = "Purchasing";
    public const string Inventory = "Inventory";
    public const string Reporting = "Reporting";
    public const string Approvals = "Approvals";
    public const string Integration = "Integration";
    public const string DataExport = "DataExport";
    public const string Chatter = "Chatter";
    public const string Kanban = "Kanban";

    private static readonly List<ErpModuleDefinition> Definitions =
    [
        // Companies, dimensions and number series are what every other module is addressed by,
        // so they are not something a company can switch off.
        // Home and Modules belong here so that the pages which manage the switches, and the page
        // the switches are read from, can never themselves be switched off.
        new(
            Core,
            GroupSystem,
            "fas fa-building",
            "/erp/setup",
            true,
            [],
            ["Companies", "Dimensions", "Numbering", "Home", "Modules"]
        ),

        // The general ledger is the backbone every posting module writes to.
        new(Finance, GroupFinance, "fas fa-coins", "/erp/chart-of-accounts", true, [], ["Finance"]),

        new(Sales, GroupOperations, "fas fa-file-invoice-dollar", "/erp/sales-invoices", false, [Finance], ["Sales"]),
        new(Purchasing, GroupOperations, "fas fa-truck", "/erp/purchase-invoices", false, [Finance], ["Purchasing"]),
        new(Inventory, GroupOperations, "fas fa-boxes-stacked", null, false, [Finance], ["Inventory"]),
        new(Reporting, GroupFinance, "fas fa-chart-line", "/erp/reports/financial", false, [Finance], ["Reporting"]),
        new(Approvals, GroupCollaboration, "fas fa-user-check", "/erp/approvals", false, [], ["Workflows"]),
        new(Chatter, GroupCollaboration, "fas fa-comments", null, false, [], ["Chatter"]),
        new(Kanban, GroupCollaboration, "fas fa-table-columns", null, false, [], ["Kanban"]),
        new(Integration, GroupSystem, "fas fa-plug", "/erp/setup/web-services", false, [], ["Integration"]),
        new(DataExport, GroupSystem, "fas fa-file-export", "/erp/setup/data-export", false, [], ["Exporting"]),
    ];

    public static IReadOnlyList<ErpModuleDefinition> All => Definitions;

    public static ErpModuleDefinition Find(string code)
    {
        return Definitions.FirstOrDefault(m => string.Equals(m.Code, code, StringComparison.OrdinalIgnoreCase));
    }

    public static ErpModuleDefinition Get(string code)
    {
        return Find(code)
            ?? throw new BusinessException(ErpErrorCodes.Modules.UnknownModule).WithData("module", code);
    }

    /// <summary>
    /// The module an application service belongs to, found from its namespace. Returns null for a
    /// type outside the ERP service, which is how the request filter lets everything else through.
    /// </summary>
    public static ErpModuleDefinition ForType(Type type)
    {
        var area = AreaOf(type);

        return area == null
            ? null
            : Definitions.FirstOrDefault(m => m.Areas.Contains(area, StringComparer.Ordinal));
    }

    /// <summary>The segment after <c>ABPmicroservice.Erp.</c>, e.g. "Sales".</summary>
    public static string AreaOf(Type type)
    {
        const string root = "ABPmicroservice.Erp.";

        var ns = type?.Namespace;
        if (ns == null || !ns.StartsWith(root, StringComparison.Ordinal))
        {
            return null;
        }

        var rest = ns[root.Length..];
        var dot = rest.IndexOf('.', StringComparison.Ordinal);

        return dot < 0 ? rest : rest[..dot];
    }

    /// <summary>Modules that would break if this one were switched off.</summary>
    public static IReadOnlyList<ErpModuleDefinition> DependantsOf(string code)
    {
        return Definitions.Where(m => m.DependsOn.Contains(code, StringComparer.OrdinalIgnoreCase)).ToList();
    }
}
