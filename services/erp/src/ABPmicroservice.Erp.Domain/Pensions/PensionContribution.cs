using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Sales;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Users;

namespace ABPmicroservice.Erp.Pensions;

/// <summary>
/// A contribution schedule: what one sponsor remits for its members for one period. It is
/// prepared (Open), checked and released, and then posted to the member ledger and the G/L.
/// </summary>
public class PensionContributionHeader : CompanyEntity, IHasNo
{
    public string No { get; private set; }
    public string SchemeCode { get; private set; }
    public string SponsorNo { get; private set; }
    public string SponsorName { get; private set; }
    public DateTime PostingDate { get; private set; }

    /// <summary>The first day of the month the contributions are for.</summary>
    public DateTime ContributionPeriod { get; private set; }

    public string Description { get; private set; }
    public PensionContributionMode ContributionMode { get; private set; } = PensionContributionMode.Normal;
    public PensionDocumentStatus Status { get; private set; }
    public DateTime? PostedDate { get; private set; }
    public string PostedBy { get; private set; }

    /// <summary>Total of the lines; kept on the header so a list of schedules needs no join.</summary>
    public decimal TotalAmount { get; internal set; }

    public int NoOfMembers { get; internal set; }

    protected PensionContributionHeader() { }

    public PensionContributionHeader(Guid id, string no, PensionSponsor sponsor, DateTime postingDate, DateTime contributionPeriod)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxDocumentNoLength).Trim().ToUpperInvariant();
        SetSponsor(sponsor);
        SetDates(postingDate, contributionPeriod);
    }

    public bool IsOpen => Status == PensionDocumentStatus.Open;

    public void SetSponsor(PensionSponsor sponsor)
    {
        EnsureOpen();
        SchemeCode = sponsor.SchemeCode;
        SponsorNo = sponsor.No;
        SponsorName = sponsor.Name;
    }

    public void SetDates(DateTime postingDate, DateTime contributionPeriod)
    {
        EnsureOpen();
        PostingDate = postingDate.Date;
        ContributionPeriod = new DateTime(contributionPeriod.Year, contributionPeriod.Month, 1);
    }

    public void SetDetails(string description, PensionContributionMode contributionMode)
    {
        EnsureOpen();
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        ContributionMode = contributionMode == PensionContributionMode.None ? PensionContributionMode.Normal : contributionMode;
    }

    /// <summary>The scheme a transfer-in schedule brings members' money from; only a transfer in has one.</summary>
    public string TransferSchemeCode { get; private set; }

    public void SetTransferScheme(string transferSchemeCode)
    {
        EnsureOpen();
        TransferSchemeCode = ContributionMode == PensionContributionMode.TransferIn
            ? CodeTableEntity.NormalizeCode(Check.Length(transferSchemeCode, nameof(transferSchemeCode), ErpDomainConsts.MaxCodeLength))
            : null;
    }

    public void Release()
    {
        EnsureOpen();
        Status = PensionDocumentStatus.Released;
    }

    public void Reopen()
    {
        if (Status != PensionDocumentStatus.Released)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.DocumentNotReleased).WithData("documentNo", No);
        }

        Status = PensionDocumentStatus.Open;
    }

    internal void MarkPosted(DateTime when, string by)
    {
        Status = PensionDocumentStatus.Posted;
        PostedDate = when;
        PostedBy = by;
    }

    /// <summary>Only an open schedule may be changed; a released one is waiting to be posted as it stands.</summary>
    public void EnsureOpen()
    {
        if (Status != PensionDocumentStatus.Open)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.DocumentNotOpen).WithData("documentNo", No ?? string.Empty);
        }
    }
}

/// <summary>One member's contributions on a schedule, by money type.</summary>
public class PensionContributionLine : CompanyEntity
{
    public string DocumentNo { get; private set; }
    public int LineNo { get; private set; }
    public string MemberNo { get; private set; }
    public string MemberName { get; private set; }
    public decimal BasicSalary { get; private set; }

    public decimal EmployeeTaxExempt { get; private set; }
    public decimal EmployeeNonTaxExempt { get; private set; }
    public decimal EmployeeAvcTaxExempt { get; private set; }
    public decimal EmployeeAvcNonTaxExempt { get; private set; }
    public decimal EmployerTaxExempt { get; private set; }
    public decimal EmployerNonTaxExempt { get; private set; }
    public decimal EmployerAvcTaxExempt { get; private set; }
    public decimal EmployerAvcNonTaxExempt { get; private set; }

    public decimal TotalAmount { get; private set; }

    protected PensionContributionLine() { }

    public PensionContributionLine(Guid id, string documentNo, int lineNo, PensionMember member)
        : base(id)
    {
        DocumentNo = Check.NotNullOrWhiteSpace(documentNo, nameof(documentNo), ErpDomainConsts.MaxDocumentNoLength);
        LineNo = lineNo;
        SetMember(member);
    }

    public void SetMember(PensionMember member)
    {
        MemberNo = member.No;
        MemberName = member.FullName;
    }

    /// <param name="amounts">The eight money-type amounts, in the order of <see cref="MoneyType.All"/>.</param>
    public void SetAmounts(decimal basicSalary, IReadOnlyList<decimal> amounts)
    {
        if (amounts.Count != MoneyType.All.Count)
        {
            throw new ArgumentException("One amount per money type is expected.", nameof(amounts));
        }

        if (basicSalary < 0 || amounts.Any(a => a < 0))
        {
            throw new BusinessException(ErpErrorCodes.Pensions.NegativeAmount).WithData("field", "Contribution");
        }

        BasicSalary = basicSalary;
        EmployeeTaxExempt = amounts[0];
        EmployeeNonTaxExempt = amounts[1];
        EmployeeAvcTaxExempt = amounts[2];
        EmployeeAvcNonTaxExempt = amounts[3];
        EmployerTaxExempt = amounts[4];
        EmployerNonTaxExempt = amounts[5];
        EmployerAvcTaxExempt = amounts[6];
        EmployerAvcNonTaxExempt = amounts[7];
        TotalAmount = amounts.Sum();
    }

    /// <summary>The line's amounts in the order of <see cref="MoneyType.All"/>.</summary>
    public decimal[] Amounts() =>
    [
        EmployeeTaxExempt,
        EmployeeNonTaxExempt,
        EmployeeAvcTaxExempt,
        EmployeeAvcNonTaxExempt,
        EmployerTaxExempt,
        EmployerNonTaxExempt,
        EmployerAvcTaxExempt,
        EmployerAvcNonTaxExempt,
    ];
}

/// <summary>
/// Prepares and posts contribution schedules.
/// <para>
/// Posting credits each member's fund in the member ledger, one entry per money type, and in the
/// G/L credits the member funds liability and debits the sponsor: the sponsor's customer account
/// when the sponsor is a customer, otherwise the contribution accrual account. Every entry carries
/// the scheme's dimension.
/// </para>
/// </summary>
public class PensionContributionEngine : DomainService
{
    public const string SourceCode = "PENCONTR";

    private readonly IRepository<PensionContributionHeader, Guid> _headers;
    private readonly IRepository<PensionContributionLine, Guid> _lines;
    private readonly IRepository<PensionMember, Guid> _members;
    private readonly IRepository<PensionSponsor, Guid> _sponsors;
    private readonly IRepository<Customer, Guid> _customers;
    private readonly PensionSetupManager _setupManager;
    private readonly PensionSchemeManager _schemeManager;
    private readonly MemberLedger _memberLedger;
    private readonly GenJnlPostLine _genJnlPostLine;
    private readonly GLRegisterManager _registerManager;
    private readonly GeneralLedgerSetupManager _glSetupManager;
    private readonly SponsorTermsManager _termsManager;
    private readonly TaxReliefLimitManager _limitManager;
    private readonly MemberHistoryRecorder _history;
    private readonly ICurrentUser _currentUser;

    public PensionContributionEngine(
        IRepository<PensionContributionHeader, Guid> headers,
        IRepository<PensionContributionLine, Guid> lines,
        IRepository<PensionMember, Guid> members,
        IRepository<PensionSponsor, Guid> sponsors,
        IRepository<Customer, Guid> customers,
        PensionSetupManager setupManager,
        PensionSchemeManager schemeManager,
        MemberLedger memberLedger,
        GenJnlPostLine genJnlPostLine,
        GLRegisterManager registerManager,
        GeneralLedgerSetupManager glSetupManager,
        SponsorTermsManager termsManager,
        TaxReliefLimitManager limitManager,
        MemberHistoryRecorder history,
        ICurrentUser currentUser
    )
    {
        _termsManager = termsManager;
        _limitManager = limitManager;
        _history = history;
        _headers = headers;
        _lines = lines;
        _members = members;
        _sponsors = sponsors;
        _customers = customers;
        _setupManager = setupManager;
        _schemeManager = schemeManager;
        _memberLedger = memberLedger;
        _genJnlPostLine = genJnlPostLine;
        _registerManager = registerManager;
        _glSetupManager = glSetupManager;
        _currentUser = currentUser;
    }

    /// <summary>Stores the totals of the lines on the header.</summary>
    public async Task UpdateTotalsAsync(PensionContributionHeader header)
    {
        var lines = await _lines.GetListAsync(l => l.DocumentNo == header.No);
        header.TotalAmount = lines.Sum(l => l.TotalAmount);
        header.NoOfMembers = lines.Count;
        await _headers.UpdateAsync(header, autoSave: true);
    }

    /// <summary>
    /// Fills the schedule with every contributing member of the sponsor, at the rates in force for
    /// the contribution period on each member's current salary, split into registered and
    /// unregistered money by the tax relief limit. Members already on the schedule are left as they
    /// are. Returns the number of lines added.
    /// </summary>
    public async Task<int> SuggestLinesAsync(PensionContributionHeader header)
    {
        header.EnsureOpen();

        var sponsor = await GetSponsorAsync(header.SponsorNo);
        var existing = await _lines.GetListAsync(l => l.DocumentNo == header.No);
        var onSchedule = existing.Select(l => l.MemberNo).ToHashSet(StringComparer.Ordinal);
        var lineNo = existing.Count == 0 ? 0 : existing.Max(l => l.LineNo);
        var (employeeRate, employerRate) = await _termsManager.GetRatesAsync(sponsor, header.ContributionPeriod);
        var split = await GetSplitterAsync(header, employeeRate, employerRate);

        var members = (await _members.GetListAsync(m => m.SponsorNo == sponsor.No && m.SchemeCode == sponsor.SchemeCode))
            .Where(m => m.IsContributing && !onSchedule.Contains(m.No))
            .OrderBy(m => m.No, StringComparer.Ordinal)
            .ToList();
        var used = await _limitManager.GetUsedAsync(members.Select(m => m.No).ToList(), header.ContributionPeriod);

        foreach (var member in members)
        {
            var employee = Math.Round(member.CurrentSalary * employeeRate / 100m, 2, MidpointRounding.AwayFromZero);
            var employer = Math.Round(member.CurrentSalary * employerRate / 100m, 2, MidpointRounding.AwayFromZero);

            lineNo += 10000;
            var line = new PensionContributionLine(GuidGenerator.Create(), header.No, lineNo, member);
            line.SetAmounts(member.CurrentSalary, split(new ContributionAmounts(employee, 0m, employer, 0m), used.GetValueOrDefault(member.No)));
            await _lines.InsertAsync(line, autoSave: true);
        }

        await UpdateTotalsAsync(header);
        return members.Count;
    }

    /// <summary>
    /// Splits every line of the schedule again into registered and unregistered money by the tax
    /// relief limit of its period, keeping what each line contributes by employee, employer and
    /// voluntary contributions. Returns the number of lines whose split changed.
    /// </summary>
    public async Task<int> SplitLinesAsync(PensionContributionHeader header)
    {
        header.EnsureOpen();

        var sponsor = await GetSponsorAsync(header.SponsorNo);
        var (employeeRate, employerRate) = await _termsManager.GetRatesAsync(sponsor, header.ContributionPeriod);
        var split = await GetSplitterAsync(header, employeeRate, employerRate);
        var lines = await _lines.GetListAsync(l => l.DocumentNo == header.No);
        var used = await _limitManager.GetUsedAsync(lines.Select(l => l.MemberNo).ToList(), header.ContributionPeriod);
        var changed = 0;

        foreach (var line in lines)
        {
            var before = line.Amounts();
            var after = split(ContributionAmounts.Of(before), used.GetValueOrDefault(line.MemberNo));
            if (before.SequenceEqual(after))
            {
                continue;
            }

            line.SetAmounts(line.BasicSalary, after);
            await _lines.UpdateAsync(line, autoSave: true);
            changed++;
        }

        await UpdateTotalsAsync(header);
        return changed;
    }

    /// <summary>
    /// How the schedule's contributions are split: by the limit in force for its period, less what
    /// the member has already had registered for that month. A transfer from another scheme keeps
    /// the split it arrives with, so it is taken as registered here.
    /// </summary>
    private async Task<Func<ContributionAmounts, decimal, decimal[]>> GetSplitterAsync(PensionContributionHeader header, decimal employeeRate, decimal employerRate)
    {
        var setup = await _setupManager.GetAsync();
        var limit = header.ContributionMode == PensionContributionMode.TransferIn ? null : await _limitManager.GetLimitAsync(header.ContributionPeriod);

        return (amounts, alreadyUsed) => ContributionSplitter.Split(
            amounts,
            limit.HasValue ? limit.Value - alreadyUsed : null,
            setup.ExcessContributionAllocation,
            employeeRate,
            employerRate
        );
    }

    /// <summary>Releases the schedule after checking that it can be posted as it stands.</summary>
    public async Task ReleaseAsync(PensionContributionHeader header)
    {
        await CheckAsync(header);
        header.Release();
        await _headers.UpdateAsync(header, autoSave: true);
    }

    public async Task PostAsync(PensionContributionHeader header)
    {
        if (header.Status != PensionDocumentStatus.Released)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.DocumentNotReleased).WithData("documentNo", header.No);
        }

        await _glSetupManager.CheckPostingDateAsync(header.PostingDate);
        var (lines, members) = await CheckAsync(header);

        var setup = await _setupManager.GetAsync();
        var memberFunds = setup.Require(setup.MemberFundsAccountNo, "Member Funds Account No.");
        var sponsor = await GetSponsorAsync(header.SponsorNo);
        var dimensionSetId = await _schemeManager.GetDimensionSetIdAsync(header.SchemeCode);

        var register = await _registerManager.OpenAsync(header.PostingDate, SourceCode, header.No);
        var context = new GLPostingContext(register, SourceCode);
        var description = header.Description.IsNullOrWhiteSpace() ? $"Contributions {header.ContributionPeriod:MMM yyyy} {sponsor.Name}" : header.Description;

        foreach (var line in lines)
        {
            var member = members[line.MemberNo];
            var amounts = line.Amounts();

            for (var i = 0; i < amounts.Length; i++)
            {
                await _memberLedger.PostAsync(
                    member,
                    header.PostingDate,
                    header.ContributionPeriod,
                    header.No,
                    description,
                    PensionTransactionType.Contribution,
                    MoneyType.All[i],
                    header.ContributionMode,
                    amounts[i],
                    line.BasicSalary,
                    dimensionSetId,
                    context,
                    _currentUser.UserName
                );
            }

            if (member.Status == MemberStatus.Dormant)
            {
                member.Reactivate();
                await _members.UpdateAsync(member);
                await _history.RecordStatusAsync(member, MemberStatus.Dormant, header.PostingDate, header.No);
            }

            await _history.RecordSalaryAsync(member, header.ContributionPeriod, line.BasicSalary, header.No);
        }

        var total = lines.Sum(l => l.TotalAmount);

        // The fund now owes its members this much more...
        await _genJnlPostLine.PostGLDirectAsync(
            memberFunds,
            header.PostingDate,
            GLEntryDocumentType.None,
            header.No,
            description,
            -total,
            sponsor.No,
            dimensionSetId,
            context
        );

        // ...and the sponsor owes the fund the same; a transfer is owed by the scheme it comes from.
        if (header.ContributionMode == PensionContributionMode.TransferIn)
        {
            await _genJnlPostLine.PostGLDirectAsync(
                setup.Require(setup.TransfersInAccountNo, "Transfers In Account No."),
                header.PostingDate,
                GLEntryDocumentType.None,
                header.No,
                description,
                total,
                sponsor.No,
                dimensionSetId,
                context
            );
        }
        else if (sponsor.CustomerNo.IsNullOrWhiteSpace())
        {
            await _genJnlPostLine.PostGLDirectAsync(
                setup.Require(setup.ContributionAccrualAccountNo, "Contribution Accrual Account No."),
                header.PostingDate,
                GLEntryDocumentType.None,
                header.No,
                description,
                total,
                sponsor.No,
                dimensionSetId,
                context
            );
        }
        else
        {
            if (!await _customers.AnyAsync(c => c.No == sponsor.CustomerNo))
            {
                throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Customer").WithData("code", sponsor.CustomerNo);
            }

            await _genJnlPostLine.PostLineAsync(
                new GenJournalLine(
                    GuidGenerator.Create(),
                    Guid.Empty,
                    1,
                    header.PostingDate,
                    GLEntryDocumentType.Invoice,
                    header.No,
                    GenJournalAccountType.Customer,
                    sponsor.CustomerNo,
                    description,
                    total,
                    dimensionSetId: dimensionSetId
                ),
                context
            );
        }

        await _registerManager.CloseAsync(register);

        header.MarkPosted(Clock.Now, _currentUser.UserName);
        await _headers.UpdateAsync(header);

        sponsor.LastScheduleDate = header.PostingDate;
        await _sponsors.UpdateAsync(sponsor, autoSave: true);
    }

    /// <summary>What must hold before a schedule is released or posted. Returns its lines and their members.</summary>
    private async Task<(List<PensionContributionLine> Lines, Dictionary<string, PensionMember> Members)> CheckAsync(PensionContributionHeader header)
    {
        await _schemeManager.GetOpenAsync(header.SchemeCode);

        var lines = (await _lines.GetListAsync(l => l.DocumentNo == header.No)).OrderBy(l => l.LineNo).ToList();
        if (lines.Count == 0 || lines.Sum(l => l.TotalAmount) == 0m)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.NothingToPost).WithData("documentNo", header.No);
        }

        var duplicate = lines.GroupBy(l => l.MemberNo).FirstOrDefault(g => g.Count() > 1);
        if (duplicate != null)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.MemberDuplicated).WithData("memberNo", duplicate.Key).WithData("documentNo", header.No);
        }

        var numbers = lines.Select(l => l.MemberNo).ToList();
        var members = (await _members.GetListAsync(m => numbers.Contains(m.No))).ToDictionary(m => m.No, StringComparer.Ordinal);

        foreach (var line in lines)
        {
            if (!members.TryGetValue(line.MemberNo, out var member) || member.SchemeCode != header.SchemeCode)
            {
                throw new BusinessException(ErpErrorCodes.Pensions.MemberNotInScheme).WithData("memberNo", line.MemberNo).WithData("scheme", header.SchemeCode);
            }

            if (!member.IsContributing)
            {
                throw new BusinessException(ErpErrorCodes.Pensions.MemberNotContributing).WithData("memberNo", member.No).WithData("status", member.Status);
            }
        }

        var setup = await _setupManager.GetAsync();
        if (!setup.AllowContributionDuplication && header.ContributionMode == PensionContributionMode.Normal)
        {
            var other = await _headers.FirstOrDefaultAsync(h =>
                h.Id != header.Id
                && h.SponsorNo == header.SponsorNo
                && h.ContributionPeriod == header.ContributionPeriod
                && h.ContributionMode == PensionContributionMode.Normal
                && h.Status == PensionDocumentStatus.Posted
            );

            if (other != null)
            {
                throw new BusinessException(ErpErrorCodes.Pensions.PeriodAlreadyPosted)
                    .WithData("sponsorNo", header.SponsorNo)
                    .WithData("period", header.ContributionPeriod.ToString("MMM yyyy"))
                    .WithData("documentNo", other.No);
            }
        }

        return (lines, members);
    }

    private async Task<PensionSponsor> GetSponsorAsync(string sponsorNo)
    {
        return await _sponsors.FirstOrDefaultAsync(s => s.No == sponsorNo)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Pension Sponsor").WithData("code", sponsorNo ?? string.Empty);
    }
}
