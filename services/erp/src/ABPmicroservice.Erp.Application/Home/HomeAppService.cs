using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Modules;
using ABPmicroservice.Erp.Permissions;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sales;
using ABPmicroservice.Erp.Workflows;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Home;

/// <summary>
/// The landing page.
/// <para>
/// A Business Central Role Center opens on what needs doing rather than on a menu, and Odoo opens
/// on the apps you have installed. This serves both from one call: the activity cues first, then
/// the modules this company has switched on, as tiles.
/// </para>
/// <para>
/// Every cue is filtered twice — by the module it belongs to and by the permission that guards the
/// list it counts — so the home page can never advertise a figure the user cannot go and look at.
/// </para>
/// </summary>
[Authorize]
public class HomeAppService : ErpAppService, IHomeAppService
{
    private readonly IModuleAppService _modules;
    private readonly ErpModuleManager _moduleManager;
    private readonly IRepository<ApprovalEntry, Guid> _approvalRepository;
    private readonly IRepository<SalesHeader, Guid> _salesRepository;
    private readonly IRepository<PurchaseHeader, Guid> _purchaseRepository;
    private readonly IRepository<CustomerLedgerEntry, Guid> _customerLedgerRepository;
    private readonly IRepository<VendorLedgerEntry, Guid> _vendorLedgerRepository;
    private readonly IRepository<GenJournalLine, Guid> _journalLineRepository;

    public HomeAppService(
        IModuleAppService modules,
        ErpModuleManager moduleManager,
        IRepository<ApprovalEntry, Guid> approvalRepository,
        IRepository<SalesHeader, Guid> salesRepository,
        IRepository<PurchaseHeader, Guid> purchaseRepository,
        IRepository<CustomerLedgerEntry, Guid> customerLedgerRepository,
        IRepository<VendorLedgerEntry, Guid> vendorLedgerRepository,
        IRepository<GenJournalLine, Guid> journalLineRepository
    )
    {
        _modules = modules;
        _moduleManager = moduleManager;
        _approvalRepository = approvalRepository;
        _salesRepository = salesRepository;
        _purchaseRepository = purchaseRepository;
        _customerLedgerRepository = customerLedgerRepository;
        _vendorLedgerRepository = vendorLedgerRepository;
        _journalLineRepository = journalLineRepository;
    }

    public async Task<HomeSummaryDto> GetSummaryAsync()
    {
        var summary = new HomeSummaryDto
        {
            CompanyName = CurrentCompany.Name,
            UserName = CurrentUser.Name ?? CurrentUser.UserName,
        };

        // Only modules that are on and have a screen: a tile that opens nothing is not a tile.
        var modules = await _modules.GetListAsync();
        summary.Apps = modules.Items.Where(m => m.Enabled && !m.Route.IsNullOrWhiteSpace()).ToList();

        summary.Cues = await GetCuesAsync();

        return summary;
    }

    private async Task<List<ActivityCueDto>> GetCuesAsync()
    {
        var cues = new List<ActivityCueDto>();
        var today = Clock.Now.Date;

        if (await CanSeeAsync(ErpModuleRegistry.Approvals, ErpPermissions.Workflows.Approve))
        {
            // Mine to approve, not everyone's: a Role Center is one person's list of work.
            var waiting = await _approvalRepository.CountAsync(a =>
                a.Status == ApprovalStatus.Open && a.ApproverId == CurrentUser.Id
            );

            cues.Add(Cue("PendingApprovals", waiting, "fas fa-user-clock", "/erp/approvals", ActivityCueTone.Attention));
        }

        if (await CanSeeAsync(ErpModuleRegistry.Finance, ErpPermissions.Journals.Default))
        {
            var unposted = await _journalLineRepository.CountAsync();

            cues.Add(
                Cue("UnpostedJournalLines", unposted, "fas fa-pen-to-square", "/erp/finance/general-journal")
            );
        }

        if (await CanSeeAsync(ErpModuleRegistry.Sales, ErpPermissions.SalesDocuments.Default))
        {
            var open = await _salesRepository.CountAsync(d => !d.Posted && d.Status != DocumentStatus.Cancelled);
            cues.Add(Cue("OpenSalesInvoices", open, "fas fa-file-invoice-dollar", "/erp/sales-invoices"));

            // Totalled in memory: the test provider is SQLite, which cannot sum a decimal. Only
            // open, overdue entries are read, so the set is small.
            var overdueEntries = await _customerLedgerRepository.GetListAsync(e => e.Open && e.DueDate < today);
            var overdue = overdueEntries.Sum(e => e.RemainingAmount);

            cues.Add(
                Cue(
                    "OverdueReceivables",
                    overdue,
                    "fas fa-hand-holding-dollar",
                    "/erp/reports/financial",
                    overdue > 0 ? ActivityCueTone.Overdue : ActivityCueTone.Neutral,
                    isAmount: true
                )
            );
        }

        if (await CanSeeAsync(ErpModuleRegistry.Purchasing, ErpPermissions.PurchaseDocuments.Default))
        {
            var open = await _purchaseRepository.CountAsync(d => !d.Posted && d.Status != DocumentStatus.Cancelled);
            cues.Add(Cue("OpenPurchaseInvoices", open, "fas fa-file-invoice", "/erp/purchase-invoices"));

            var overdueEntries = await _vendorLedgerRepository.GetListAsync(e => e.Open && e.DueDate < today);
            var overdue = overdueEntries.Sum(e => e.RemainingAmount);

            cues.Add(
                Cue(
                    "OverduePayables",
                    overdue,
                    "fas fa-money-bill-transfer",
                    "/erp/reports/financial",
                    overdue > 0 ? ActivityCueTone.Overdue : ActivityCueTone.Neutral,
                    isAmount: true
                )
            );
        }

        return cues;
    }

    private async Task<bool> CanSeeAsync(string moduleCode, string permission)
    {
        return await _moduleManager.IsEnabledAsync(moduleCode)
            && await AuthorizationService.IsGrantedAsync(permission);
    }

    private ActivityCueDto Cue(
        string key,
        decimal value,
        string icon,
        string route,
        ActivityCueTone tone = ActivityCueTone.Neutral,
        bool isAmount = false
    )
    {
        return new ActivityCueDto
        {
            Key = key,
            DisplayName = L[$"Cue:{key}"],
            Value = value,
            IsAmount = isAmount,
            Tone = value > 0 ? tone : ActivityCueTone.Neutral,
            Icon = icon,
            Route = route,
        };
    }
}
