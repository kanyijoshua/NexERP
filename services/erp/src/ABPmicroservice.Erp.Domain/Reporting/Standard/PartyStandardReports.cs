using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sales;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Reporting;

/// <summary>A customer or vendor as the balance reports see it.</summary>
public readonly record struct ReportParty(string No, string Name, string PhoneNo, string Contact, string PostingGroup, bool Blocked);

/// <summary>A customer or vendor ledger entry as the balance reports see it, in local currency.</summary>
public readonly record struct ReportPartyEntry(
    long EntryNo,
    string PartyNo,
    DateTime PostingDate,
    DateTime DueDate,
    string DocumentType,
    string DocumentNo,
    string Description,
    decimal Amount,
    decimal RemainingAmount,
    bool Open,
    bool Reversed
);

/// <summary>Reads customers or vendors and their ledgers in one shape, so each balance report is written once.</summary>
public class ReportPartyReader : ITransientDependency
{
    private readonly IRepository<Customer, Guid> _customers;
    private readonly IRepository<Vendor, Guid> _vendors;
    private readonly IRepository<CustomerLedgerEntry, Guid> _customerEntries;
    private readonly IRepository<VendorLedgerEntry, Guid> _vendorEntries;

    public ReportPartyReader(
        IRepository<Customer, Guid> customers,
        IRepository<Vendor, Guid> vendors,
        IRepository<CustomerLedgerEntry, Guid> customerEntries,
        IRepository<VendorLedgerEntry, Guid> vendorEntries
    )
    {
        _customers = customers;
        _vendors = vendors;
        _customerEntries = customerEntries;
        _vendorEntries = vendorEntries;
    }

    public async Task<List<ReportParty>> GetPartiesAsync(AgedLedgerKind kind, AccountTotaling filter)
    {
        var parties = kind == AgedLedgerKind.Receivables
            ? (await _customers.GetListAsync()).Select(c => new ReportParty(c.No, c.Name, c.PhoneNo, c.Contact, c.CustomerPostingGroup, c.Blocked))
            : (await _vendors.GetListAsync()).Select(v => new ReportParty(v.No, v.Name, v.PhoneNo, v.Contact, v.VendorPostingGroup, v.Blocked));

        return parties.Where(p => filter.Matches(p.No)).OrderBy(p => p.No, StringComparer.Ordinal).ToList();
    }

    /// <summary>Every entry posted on or before <paramref name="to"/>, keyed by customer or vendor number.</summary>
    public async Task<ILookup<string, ReportPartyEntry>> GetEntriesAsync(AgedLedgerKind kind, DateTime to)
    {
        var entries = kind == AgedLedgerKind.Receivables
            ? (await _customerEntries.GetListAsync(e => e.PostingDate <= to)).Select(e => new ReportPartyEntry(
                e.EntryNo, e.CustomerNo, e.PostingDate, e.DueDate, e.DocumentType, e.DocumentNo, e.Description,
                e.AmountLcy, e.RemainingAmountLcy, e.Open, e.Reversed))
            : (await _vendorEntries.GetListAsync(e => e.PostingDate <= to)).Select(e => new ReportPartyEntry(
                e.EntryNo, e.VendorNo, e.PostingDate, e.DueDate, e.DocumentType, e.DocumentNo, e.Description,
                e.AmountLcy, e.RemainingAmountLcy, e.Open, e.Reversed));

        return entries.ToLookup(e => e.PartyNo, StringComparer.Ordinal);
    }
}

/// <summary>What the customer and vendor versions of a balance report share.</summary>
public abstract class PartyReportBase : StandardReportBase
{
    protected ReportPartyReader Reader => LazyServiceProvider.LazyGetRequiredService<ReportPartyReader>();

    protected abstract AgedLedgerKind Kind { get; }

    protected string Area => Kind == AgedLedgerKind.Receivables ? StandardReportAreas.Sales : StandardReportAreas.Purchasing;
}

/// <summary>
/// Detail Trial Balance.(customer) and 304 (vendor): the
/// balance brought forward, each entry of the period and the running balance after it.
/// </summary>
public abstract class PartyDetailTrialBalanceReport : PartyReportBase
{
    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Date(result, "postingDate", "Posting Date");
        Text(result, "documentType", "Document Type");
        Text(result, "documentNo", "Document No.");
        Text(result, "description", "Description");
        Number(result, "debit", "Debit");
        Number(result, "credit", "Credit");
        Number(result, "balance", "Balance");

        var entries = await Reader.GetEntriesAsync(Kind, request.To);
        var grandTotal = 0m;

        foreach (var party in await Reader.GetPartiesAsync(Kind, Filter(request)))
        {
            var balance = entries[party.No].Where(e => e.PostingDate < request.From).Sum(e => e.Amount);
            var inPeriod = entries[party.No].Where(e => e.PostingDate >= request.From).OrderBy(e => e.PostingDate).ThenBy(e => e.EntryNo).ToList();
            if (balance == 0m && inPeriod.Count == 0)
            {
                continue;
            }

            BoldRow(result, ("documentNo", party.No), ("description", party.Name), ("balance", balance));

            foreach (var entry in inPeriod)
            {
                balance += entry.Amount;
                var row = Row(
                    result,
                    ("postingDate", entry.PostingDate),
                    ("documentType", entry.DocumentType),
                    ("documentNo", entry.DocumentNo),
                    ("description", entry.Description),
                    ("debit", Debit(entry.Amount)),
                    ("credit", Credit(entry.Amount)),
                    ("balance", balance)
                );
                row.Indentation = 1;
                row.Italic = entry.Reversed;
            }

            grandTotal += balance;
        }

        BoldRow(result, ("description", "Total"), ("balance", grandTotal));
        return result;
    }
}

public class CustomerDetailTrialBalanceReport : PartyDetailTrialBalanceReport
{
    protected override AgedLedgerKind Kind => AgedLedgerKind.Receivables;

    public override StandardReportDefinition Definition { get; } =
        new(104, "CustomerDetailTrialBalance", "Customer - Detail Trial Bal.", StandardReportAreas.Sales, StandardReportParameters.Period | StandardReportParameters.NoFilter);
}

public class VendorDetailTrialBalanceReport : PartyDetailTrialBalanceReport
{
    protected override AgedLedgerKind Kind => AgedLedgerKind.Payables;

    public override StandardReportDefinition Definition { get; } =
        new(304, "VendorDetailTrialBalance", "Vendor - Detail Trial Balance", StandardReportAreas.Purchasing, StandardReportParameters.Period | StandardReportParameters.NoFilter);
}

/// <summary>
/// Trial Balance.(customer) and 329 (vendor): the opening
/// balance, the debits and credits of the period and the closing balance of each party.
/// </summary>
public abstract class PartyTrialBalanceReport : PartyReportBase
{
    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "no", "No.");
        Text(result, "name", "Name");
        Number(result, "beginningBalance", "Beginning Balance");
        Number(result, "debit", "Debit");
        Number(result, "credit", "Credit");
        Number(result, "endingBalance", "Ending Balance");

        var entries = await Reader.GetEntriesAsync(Kind, request.To);
        var totals = new decimal[4];

        foreach (var party in await Reader.GetPartiesAsync(Kind, Filter(request)))
        {
            var beginning = entries[party.No].Where(e => e.PostingDate < request.From).Sum(e => e.Amount);
            var inPeriod = entries[party.No].Where(e => e.PostingDate >= request.From).ToList();
            var debit = inPeriod.Sum(e => Debit(e.Amount));
            var credit = inPeriod.Sum(e => Credit(e.Amount));
            var ending = beginning + debit - credit;

            if (beginning == 0m && debit == 0m && credit == 0m)
            {
                continue;
            }

            Row(result, ("no", party.No), ("name", party.Name), ("beginningBalance", beginning), ("debit", debit), ("credit", credit), ("endingBalance", ending));

            totals[0] += beginning;
            totals[1] += debit;
            totals[2] += credit;
            totals[3] += ending;
        }

        BoldRow(result, ("no", string.Empty), ("name", "Total"), ("beginningBalance", totals[0]), ("debit", totals[1]), ("credit", totals[2]), ("endingBalance", totals[3]));
        return result;
    }
}

public class CustomerTrialBalanceReport : PartyTrialBalanceReport
{
    protected override AgedLedgerKind Kind => AgedLedgerKind.Receivables;

    public override StandardReportDefinition Definition { get; } =
        new(129, "CustomerTrialBalance", "Customer - Trial Balance", StandardReportAreas.Sales, StandardReportParameters.Period | StandardReportParameters.NoFilter);
}

public class VendorTrialBalanceReport : PartyTrialBalanceReport
{
    protected override AgedLedgerKind Kind => AgedLedgerKind.Payables;

    public override StandardReportDefinition Definition { get; } =
        new(329, "VendorTrialBalance", "Vendor - Trial Balance", StandardReportAreas.Purchasing, StandardReportParameters.Period | StandardReportParameters.NoFilter);
}

/// <summary>
/// Balance to Date.(customer) and 321 (vendor): the entries
/// still open as at a date, with what remains of each.
/// </summary>
public abstract class PartyBalanceToDateReport : PartyReportBase
{
    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Date(result, "postingDate", "Posting Date");
        Text(result, "documentType", "Document Type");
        Text(result, "documentNo", "Document No.");
        Text(result, "description", "Description");
        Date(result, "dueDate", "Due Date");
        Number(result, "amount", "Original Amount");
        Number(result, "remainingAmount", "Remaining Amount");

        var entries = await Reader.GetEntriesAsync(Kind, request.To);
        var grandTotal = 0m;

        foreach (var party in await Reader.GetPartiesAsync(Kind, Filter(request)))
        {
            var open = entries[party.No].Where(e => e.Open && e.RemainingAmount != 0m).OrderBy(e => e.PostingDate).ThenBy(e => e.EntryNo).ToList();
            if (open.Count == 0)
            {
                continue;
            }

            var total = open.Sum(e => e.RemainingAmount);
            BoldRow(result, ("documentNo", party.No), ("description", party.Name), ("remainingAmount", total));

            foreach (var entry in open)
            {
                var row = Row(
                    result,
                    ("postingDate", entry.PostingDate),
                    ("documentType", entry.DocumentType),
                    ("documentNo", entry.DocumentNo),
                    ("description", entry.Description),
                    ("dueDate", entry.DueDate),
                    ("amount", entry.Amount),
                    ("remainingAmount", entry.RemainingAmount)
                );
                row.Indentation = 1;
            }

            grandTotal += total;
        }

        BoldRow(result, ("description", "Total"), ("remainingAmount", grandTotal));
        return result;
    }
}

public class CustomerBalanceToDateReport : PartyBalanceToDateReport
{
    protected override AgedLedgerKind Kind => AgedLedgerKind.Receivables;

    public override StandardReportDefinition Definition { get; } =
        new(121, "CustomerBalanceToDate", "Customer - Balance to Date", StandardReportAreas.Sales, StandardReportParameters.AsOfDate | StandardReportParameters.NoFilter);
}

public class VendorBalanceToDateReport : PartyBalanceToDateReport
{
    protected override AgedLedgerKind Kind => AgedLedgerKind.Payables;

    public override StandardReportDefinition Definition { get; } =
        new(321, "VendorBalanceToDate", "Vendor - Balance to Date", StandardReportAreas.Purchasing, StandardReportParameters.AsOfDate | StandardReportParameters.NoFilter);
}

/// <summary>List.(customer) and 301 (vendor): the master records and their balance.</summary>
public abstract class PartyListReport : PartyReportBase
{
    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "no", "No.");
        Text(result, "name", "Name");
        Text(result, "postingGroup", "Posting Group");
        Text(result, "contact", "Contact");
        Text(result, "phoneNo", "Phone No.");
        Text(result, "blocked", "Blocked");
        Number(result, "balance", "Balance (LCY)");

        var entries = await Reader.GetEntriesAsync(Kind, request.To);
        var total = 0m;

        foreach (var party in await Reader.GetPartiesAsync(Kind, Filter(request)))
        {
            var balance = entries[party.No].Sum(e => e.Amount);
            total += balance;

            Row(
                result,
                ("no", party.No),
                ("name", party.Name),
                ("postingGroup", party.PostingGroup),
                ("contact", party.Contact),
                ("phoneNo", party.PhoneNo),
                ("blocked", party.Blocked ? "Yes" : string.Empty),
                ("balance", balance)
            );
        }

        BoldRow(result, ("no", string.Empty), ("name", "Total"), ("balance", total));
        return result;
    }
}

public class CustomerListReport : PartyListReport
{
    protected override AgedLedgerKind Kind => AgedLedgerKind.Receivables;

    public override StandardReportDefinition Definition { get; } =
        new(101, "CustomerList", "Customer - List", StandardReportAreas.Sales, StandardReportParameters.NoFilter);
}

public class VendorListReport : PartyListReport
{
    protected override AgedLedgerKind Kind => AgedLedgerKind.Payables;

    public override StandardReportDefinition Definition { get; } =
        new(301, "VendorList", "Vendor - List", StandardReportAreas.Purchasing, StandardReportParameters.NoFilter);
}

/// <summary>
/// Top 10 List.(customer) and 311 (vendor): the ten parties
/// with the largest movement in the period, and each one's share of the total.
/// </summary>
public abstract class PartyTop10Report : PartyReportBase
{
    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "rank", "Rank");
        Text(result, "no", "No.");
        Text(result, "name", "Name");
        Number(result, "amount", Kind == AgedLedgerKind.Receivables ? "Sales (LCY)" : "Purchases (LCY)");
        Number(result, "percent", "% of Total");
        Number(result, "balance", "Balance (LCY)");

        var entries = await Reader.GetEntriesAsync(Kind, request.To);

        // Invoices less credit memos: what was sold to, or bought from, the party in the period.
        var ranked = (await Reader.GetPartiesAsync(Kind, Filter(request)))
            .Select(p => new
            {
                Party = p,
                Amount = Math.Abs(entries[p.No]
                    .Where(e => e.PostingDate >= request.From && (e.DocumentType == "Invoice" || e.DocumentType == "CreditMemo" || e.DocumentType == "Credit Memo"))
                    .Sum(e => e.Amount)),
                Balance = entries[p.No].Sum(e => e.Amount),
            })
            .Where(x => x.Amount != 0m)
            .OrderByDescending(x => x.Amount)
            .ToList();

        var total = ranked.Sum(x => x.Amount);
        var rank = 0;

        foreach (var item in ranked.Take(10))
        {
            Row(
                result,
                ("rank", (++rank).ToString()),
                ("no", item.Party.No),
                ("name", item.Party.Name),
                ("amount", item.Amount),
                ("percent", total == 0m ? 0m : Math.Round(item.Amount / total * 100m, 1)),
                ("balance", item.Balance)
            );
        }

        BoldRow(result, ("rank", string.Empty), ("no", string.Empty), ("name", "Total of all"), ("amount", total), ("percent", total == 0m ? 0m : 100m));
        return result;
    }
}

public class CustomerTop10Report : PartyTop10Report
{
    protected override AgedLedgerKind Kind => AgedLedgerKind.Receivables;

    public override StandardReportDefinition Definition { get; } =
        new(111, "CustomerTop10List", "Customer - Top 10 List", StandardReportAreas.Sales, StandardReportParameters.Period);
}

public class VendorTop10Report : PartyTop10Report
{
    protected override AgedLedgerKind Kind => AgedLedgerKind.Payables;

    public override StandardReportDefinition Definition { get; } =
        new(311, "VendorTop10List", "Vendor - Top 10 List", StandardReportAreas.Purchasing, StandardReportParameters.Period);
}
