using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Sequences;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Pensions;

/// <summary>
/// A kind of money in a member's fund: whose it is and whether it is registered. A member's
/// balance is kept apart by money type because an exit pays and taxes each one differently.
/// </summary>
public readonly record struct MoneyType(PensionContributionType ContributionType, PensionExemptionType ExemptionType)
{
    /// <summary>The eight money types of a normal contribution, in the order schedules list them.</summary>
    public static readonly IReadOnlyList<MoneyType> All =
    [
        new(PensionContributionType.EmployeeContribution, PensionExemptionType.TaxExempt),
        new(PensionContributionType.EmployeeContribution, PensionExemptionType.NonTaxExempt),
        new(PensionContributionType.EmployeeAdditional, PensionExemptionType.TaxExempt),
        new(PensionContributionType.EmployeeAdditional, PensionExemptionType.NonTaxExempt),
        new(PensionContributionType.EmployerContribution, PensionExemptionType.TaxExempt),
        new(PensionContributionType.EmployerContribution, PensionExemptionType.NonTaxExempt),
        new(PensionContributionType.EmployerAdditional, PensionExemptionType.TaxExempt),
        new(PensionContributionType.EmployerAdditional, PensionExemptionType.NonTaxExempt),
    ];

    /// <summary>The member's own money: the employee contribution and the employee's voluntary contributions.</summary>
    public bool IsEmployee => ContributionType is PensionContributionType.EmployeeContribution or PensionContributionType.EmployeeAdditional or PensionContributionType.Pre90Employee;

    public bool IsRegistered => ExemptionType == PensionExemptionType.TaxExempt;
}

/// <summary>
/// One movement of a member's fund: a contribution, interest, a withdrawal. The member ledger is
/// to a member what the vendor ledger is to a vendor, with each entry also saying which scheme,
/// which sponsor and which money type it belongs to. A positive amount adds to the member's fund.
/// </summary>
public class MemberLedgerEntry : LedgerEntryBase
{
    public string SchemeCode { get; private set; }
    public string MemberNo { get; private set; }
    public string SponsorNo { get; private set; }
    public DateTime PostingDate { get; private set; }

    /// <summary>The month the money is for, which for arrears is not the month it was posted in.</summary>
    public DateTime? ContributionPeriod { get; private set; }

    public string DocumentNo { get; private set; }
    public string Description { get; private set; }
    public PensionTransactionType TransactionType { get; private set; }
    public PensionContributionType ContributionType { get; private set; }
    public PensionContributionMode ContributionMode { get; private set; }
    public PensionExemptionType ExemptionType { get; private set; }
    public decimal Amount { get; private set; }

    /// <summary>The pensionable salary the contribution was worked out on.</summary>
    public decimal Salary { get; private set; }

    public Guid DimensionSetId { get; private set; }
    public long TransactionNo { get; private set; }
    public long RegisterNo { get; private set; }
    public string UserName { get; private set; }

    protected MemberLedgerEntry() { }

    public MemberLedgerEntry(
        Guid id,
        string schemeCode,
        string memberNo,
        string sponsorNo,
        DateTime postingDate,
        DateTime? contributionPeriod,
        string documentNo,
        string description,
        PensionTransactionType transactionType,
        MoneyType moneyType,
        PensionContributionMode contributionMode,
        decimal amount,
        decimal salary,
        Guid dimensionSetId,
        GLPostingContext context,
        string userName
    )
        : base(id)
    {
        SchemeCode = schemeCode;
        MemberNo = memberNo;
        SponsorNo = sponsorNo;
        PostingDate = postingDate.Date;
        ContributionPeriod = contributionPeriod?.Date;
        DocumentNo = documentNo;
        Description = description;
        TransactionType = transactionType;
        ContributionType = moneyType.ContributionType;
        ExemptionType = moneyType.ExemptionType;
        ContributionMode = contributionMode;
        Amount = amount;
        Salary = salary;
        DimensionSetId = dimensionSetId;
        TransactionNo = context.TransactionNo;
        RegisterNo = context.Register.No;
        UserName = userName;
    }

    public MoneyType MoneyType => new(ContributionType, ExemptionType);
}

/// <summary>A member's fund by money type, as at a date.</summary>
public class MemberBalances
{
    private readonly Dictionary<MoneyType, decimal> _byMoneyType;

    public MemberBalances(IEnumerable<MemberLedgerEntry> entries)
    {
        _byMoneyType = entries.GroupBy(e => e.MoneyType).ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));
    }

    public IReadOnlyDictionary<MoneyType, decimal> ByMoneyType => _byMoneyType;

    public decimal Of(MoneyType moneyType) => _byMoneyType.GetValueOrDefault(moneyType);

    public decimal Total => _byMoneyType.Values.Sum();

    public decimal Employee => _byMoneyType.Where(kv => kv.Key.IsEmployee).Sum(kv => kv.Value);

    public decimal Employer => _byMoneyType.Where(kv => !kv.Key.IsEmployee).Sum(kv => kv.Value);

    public decimal Registered => _byMoneyType.Where(kv => kv.Key.IsRegistered).Sum(kv => kv.Value);

    public decimal Unregistered => _byMoneyType.Where(kv => !kv.Key.IsRegistered).Sum(kv => kv.Value);
}

/// <summary>Writes the member ledger and reads balances from it.</summary>
public class MemberLedger : DomainService
{
    public const string SequenceName = "MEMBERLEDG";

    private readonly IRepository<MemberLedgerEntry, Guid> _entries;
    private readonly IEntryNoGenerator _entryNoGenerator;

    public MemberLedger(IRepository<MemberLedgerEntry, Guid> entries, IEntryNoGenerator entryNoGenerator)
    {
        _entries = entries;
        _entryNoGenerator = entryNoGenerator;
    }

    /// <summary>Adds an entry to the member ledger. A zero amount writes nothing.</summary>
    public async Task<MemberLedgerEntry> PostAsync(
        PensionMember member,
        DateTime postingDate,
        DateTime? contributionPeriod,
        string documentNo,
        string description,
        PensionTransactionType transactionType,
        MoneyType moneyType,
        PensionContributionMode contributionMode,
        decimal amount,
        decimal salary,
        Guid dimensionSetId,
        GLPostingContext context,
        string userName
    )
    {
        if (amount == 0m)
        {
            return null;
        }

        var entry = new MemberLedgerEntry(
            GuidGenerator.Create(),
            member.SchemeCode,
            member.No,
            member.SponsorNo,
            postingDate,
            contributionPeriod,
            documentNo,
            description,
            transactionType,
            moneyType,
            contributionMode,
            amount,
            salary,
            dimensionSetId,
            context,
            userName
        )
        {
            EntryNo = await _entryNoGenerator.NextAsync(SequenceName),
        };

        return await _entries.InsertAsync(entry);
    }

    /// <summary>The member's fund by money type, counting every entry posted on or before <paramref name="asOf"/>.</summary>
    public async Task<MemberBalances> GetBalancesAsync(string memberNo, DateTime asOf)
    {
        var date = asOf.Date;
        return new MemberBalances(await _entries.GetListAsync(e => e.MemberNo == memberNo && e.PostingDate <= date));
    }

    /// <summary>The balances of every member of a scheme, by member number.</summary>
    public async Task<Dictionary<string, MemberBalances>> GetSchemeBalancesAsync(string schemeCode, DateTime asOf)
    {
        var date = asOf.Date;
        return (await _entries.GetListAsync(e => e.SchemeCode == schemeCode && e.PostingDate <= date))
            .GroupBy(e => e.MemberNo)
            .ToDictionary(g => g.Key, g => new MemberBalances(g), StringComparer.Ordinal);
    }
}
