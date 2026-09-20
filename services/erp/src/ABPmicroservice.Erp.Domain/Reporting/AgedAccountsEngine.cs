using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sales;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Reporting;

/// <summary>Parameters of an aged receivables or payables report.</summary>
public class AgedAccountsRequest
{
    public AgedLedgerKind Kind { get; set; }

    public DateTime AsOfDate { get; set; }

    public AgingMethod AgingMethod { get; set; } = AgingMethod.DueDate;

    /// <summary>Width of each ageing bucket. BC's request page defaults to one month.</summary>
    public int PeriodLengthDays { get; set; } = 30;

    /// <summary>Leaves out parties whose open entries net to nothing.</summary>
    public bool ExcludeZeroBalances { get; set; } = true;
}

/// <summary>
/// Aged Accounts Receivable and Payable.
/// Mirrors Business Central reports 120 "Aged Accounts Receivable" and 322 "Aged Accounts Payable".
/// <para>
/// Only open entries count, and each is placed in a bucket by how long it has been due (or
/// posted) as at the report date. The buckets total back to the party's open balance.
/// </para>
/// </summary>
public class AgedAccountsEngine : DomainService
{
    private readonly IRepository<CustomerLedgerEntry, Guid> _customerLedgerRepository;
    private readonly IRepository<VendorLedgerEntry, Guid> _vendorLedgerRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<Vendor, Guid> _vendorRepository;

    public AgedAccountsEngine(
        IRepository<CustomerLedgerEntry, Guid> customerLedgerRepository,
        IRepository<VendorLedgerEntry, Guid> vendorLedgerRepository,
        IRepository<Customer, Guid> customerRepository,
        IRepository<Vendor, Guid> vendorRepository
    )
    {
        _customerLedgerRepository = customerLedgerRepository;
        _vendorLedgerRepository = vendorLedgerRepository;
        _customerRepository = customerRepository;
        _vendorRepository = vendorRepository;
    }

    public async Task<ReportResult> GenerateAsync(AgedAccountsRequest request)
    {
        var openEntries = request.Kind == AgedLedgerKind.Receivables
            ? await GetOpenReceivablesAsync(request.AsOfDate)
            : await GetOpenPayablesAsync(request.AsOfDate);

        var names = request.Kind == AgedLedgerKind.Receivables
            ? (await _customerRepository.GetListAsync()).ToDictionary(c => c.No, c => c.Name, StringComparer.Ordinal)
            : (await _vendorRepository.GetListAsync()).ToDictionary(v => v.No, v => v.Name, StringComparer.Ordinal);

        var periodLength = Math.Max(1, request.PeriodLengthDays);

        var result = new ReportResult
        {
            Title = request.Kind == AgedLedgerKind.Receivables ? "Aged Accounts Receivable" : "Aged Accounts Payable",
            FromDate = request.AsOfDate,
            ToDate = request.AsOfDate,
        };

        result.Columns.Add(new ReportColumnDefinition("partyNo", "No.", ReportColumnKind.Text));
        result.Columns.Add(new ReportColumnDefinition("name", "Name", ReportColumnKind.Text));
        result.Columns.Add(new ReportColumnDefinition("notDue", "Not Due"));
        result.Columns.Add(new ReportColumnDefinition("bucket1", $"1-{periodLength}"));
        result.Columns.Add(new ReportColumnDefinition("bucket2", $"{periodLength + 1}-{periodLength * 2}"));
        result.Columns.Add(new ReportColumnDefinition("bucket3", $"{periodLength * 2 + 1}-{periodLength * 3}"));
        result.Columns.Add(new ReportColumnDefinition("bucket4", $"Over {periodLength * 3}"));
        result.Columns.Add(new ReportColumnDefinition("balance", "Balance"));

        var totals = new decimal[6];

        foreach (var group in openEntries.GroupBy(e => e.PartyNo).OrderBy(g => g.Key, StringComparer.Ordinal))
        {
            var buckets = new decimal[5];

            foreach (var entry in group)
            {
                var referenceDate = request.AgingMethod == AgingMethod.DueDate ? entry.DueDate : entry.PostingDate;
                buckets[BucketOf(referenceDate, request.AsOfDate, periodLength)] += entry.RemainingAmount;
            }

            var balance = buckets.Sum();
            if (request.ExcludeZeroBalances && balance == 0m)
            {
                continue;
            }

            var row = result.AddRow();
            row.Values["partyNo"] = group.Key;
            row.Values["name"] = names.GetValueOrDefault(group.Key, string.Empty);
            row.Values["notDue"] = buckets[0];
            row.Values["bucket1"] = buckets[1];
            row.Values["bucket2"] = buckets[2];
            row.Values["bucket3"] = buckets[3];
            row.Values["bucket4"] = buckets[4];
            row.Values["balance"] = balance;

            for (var i = 0; i < 5; i++)
            {
                totals[i] += buckets[i];
            }

            totals[5] += balance;
        }

        var totalRow = result.AddRow();
        totalRow.Bold = true;
        totalRow.Values["partyNo"] = string.Empty;
        totalRow.Values["name"] = "Total";
        totalRow.Values["notDue"] = totals[0];
        totalRow.Values["bucket1"] = totals[1];
        totalRow.Values["bucket2"] = totals[2];
        totalRow.Values["bucket3"] = totals[3];
        totalRow.Values["bucket4"] = totals[4];
        totalRow.Values["balance"] = totals[5];

        return result;
    }

    /// <summary>
    /// Bucket 0 is not yet due; the rest are periods of <paramref name="periodLength"/> days,
    /// with everything beyond three periods falling into the last one.
    /// </summary>
    public static int BucketOf(DateTime referenceDate, DateTime asOfDate, int periodLength)
    {
        var daysOverdue = (asOfDate.Date - referenceDate.Date).Days;

        if (daysOverdue <= 0)
        {
            return 0;
        }

        var bucket = (daysOverdue - 1) / periodLength + 1;
        return Math.Min(bucket, 4);
    }

    private async Task<List<AgedEntry>> GetOpenReceivablesAsync(DateTime asOfDate)
    {
        var entries = await _customerLedgerRepository.GetListAsync(e => e.Open && e.PostingDate <= asOfDate);
        return entries
            .Where(e => e.RemainingAmount != 0m)
            .Select(e => new AgedEntry(e.CustomerNo, e.PostingDate, e.DueDate, e.RemainingAmount))
            .ToList();
    }

    private async Task<List<AgedEntry>> GetOpenPayablesAsync(DateTime asOfDate)
    {
        var entries = await _vendorLedgerRepository.GetListAsync(e => e.Open && e.PostingDate <= asOfDate);
        return entries
            .Where(e => e.RemainingAmount != 0m)
            .Select(e => new AgedEntry(e.VendorNo, e.PostingDate, e.DueDate, e.RemainingAmount))
            .ToList();
    }

    private readonly record struct AgedEntry(string PartyNo, DateTime PostingDate, DateTime DueDate, decimal RemainingAmount);
}
