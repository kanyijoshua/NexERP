using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Permissions;
using ABPmicroservice.Erp.Sales;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Pensions;

/// <summary>Sponsors: the employers that contribute to a scheme for their employees.</summary>
public class PensionSponsorAppService
    : ErpTableAppService<PensionSponsor, PensionSponsorDto, GetPensionSponsorListInput, CreateUpdatePensionSponsorDto>,
        IPensionSponsorAppService
{
    private readonly PensionSetupManager _setupManager;
    private readonly PensionSchemeManager _schemeManager;
    private readonly NoSeriesManager _noSeriesManager;
    private readonly IRepository<PensionMember, Guid> _members;

    public PensionSponsorAppService(
        IRepository<PensionSponsor, Guid> repository,
        PensionSetupManager setupManager,
        PensionSchemeManager schemeManager,
        NoSeriesManager noSeriesManager,
        IRepository<PensionMember, Guid> members
    )
        : base(repository, ErpPermissions.Pensions.Default)
    {
        _setupManager = setupManager;
        _schemeManager = schemeManager;
        _noSeriesManager = noSeriesManager;
        _members = members;
    }

    public override async Task<PensionSponsorDto> CreateAsync(CreateUpdatePensionSponsorDto input)
    {
        await CheckCreatePolicyAsync();
        await ValidateAsync(input);

        var setup = await _setupManager.GetAsync();
        var no = (await _noSeriesManager.ResolveNoAsync(setup.SponsorNos, input.No, Clock.Now)).ToUpperInvariant();
        await EnsureNoIsUniqueAsync(no, null);

        var sponsor = new PensionSponsor(GuidGenerator.Create(), no, input.Name, input.SchemeCode);
        Apply(sponsor, input);

        await Repository.InsertAsync(sponsor, autoSave: true);
        return await MapToGetOutputDtoAsync(sponsor);
    }

    public override async Task<PensionSponsorDto> UpdateAsync(Guid id, CreateUpdatePensionSponsorDto input)
    {
        await CheckUpdatePolicyAsync();

        var sponsor = await GetEntityByIdAsync(id);
        await ValidateAsync(input);

        var scheme = CodeTableEntity.NormalizeCode(input.SchemeCode);
        if (sponsor.SchemeCode != scheme && await _members.AnyAsync(m => m.SponsorNo == sponsor.No))
        {
            // The members would be left in a scheme their sponsor is no longer part of.
            throw new BusinessException(ErpErrorCodes.Pensions.SponsorHasMembers).WithData("sponsorNo", sponsor.No);
        }

        Apply(sponsor, input);

        await Repository.UpdateAsync(sponsor, autoSave: true);
        return await MapToGetOutputDtoAsync(sponsor);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var sponsor = await GetEntityByIdAsync(id);
        if (await _members.AnyAsync(m => m.SponsorNo == sponsor.No))
        {
            throw new BusinessException(ErpErrorCodes.Pensions.SponsorHasMembers).WithData("sponsorNo", sponsor.No);
        }

        await Repository.DeleteAsync(id, autoSave: true);
    }

    protected override async Task<IQueryable<PensionSponsor>> CreateFilteredQueryAsync(GetPensionSponsorListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var scheme = input.SchemeCode?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!scheme.IsNullOrEmpty(), x => x.SchemeCode == scheme)
            .WhereIf(!filter.IsNullOrEmpty(), x => x.No.ToLower().Contains(filter) || x.Name.ToLower().Contains(filter));
    }

    protected override IQueryable<PensionSponsor> ApplyDefaultSorting(IQueryable<PensionSponsor> query) => query.OrderBy(x => x.No);

    private async Task ValidateAsync(CreateUpdatePensionSponsorDto input)
    {
        await _schemeManager.GetAsync(input.SchemeCode);
        await Relations.EnsureNoExistsAsync<Customer>(input.CustomerNo);
    }

    private async Task EnsureNoIsUniqueAsync(string no, Guid? exceptId)
    {
        if (await Repository.AnyAsync(x => x.No == no && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Pension Sponsor").WithData("key", no);
        }
    }

    private static void Apply(PensionSponsor sponsor, CreateUpdatePensionSponsorDto input)
    {
        sponsor.Set(
            input.Name,
            input.SchemeCode,
            input.CustomerNo,
            input.Address,
            input.City,
            input.PhoneNo,
            input.Email,
            input.Contact,
            input.TaxPinNo,
            input.EmployeeRatePct,
            input.EmployerRatePct,
            input.Blocked
        );
    }
}

/// <summary>Scheme members. A member belongs to the scheme of the member's sponsor.</summary>
public class PensionMemberAppService
    : ErpTableAppService<PensionMember, PensionMemberDto, GetPensionMemberListInput, CreateUpdatePensionMemberDto>,
        IPensionMemberAppService
{
    private readonly PensionSetupManager _setupManager;
    private readonly PensionSchemeManager _schemeManager;
    private readonly NoSeriesManager _noSeriesManager;
    private readonly MemberLedger _memberLedger;
    private readonly IRepository<PensionSponsor, Guid> _sponsors;
    private readonly IRepository<MemberLedgerEntry, Guid> _entries;

    public PensionMemberAppService(
        IRepository<PensionMember, Guid> repository,
        PensionSetupManager setupManager,
        PensionSchemeManager schemeManager,
        NoSeriesManager noSeriesManager,
        MemberLedger memberLedger,
        IRepository<PensionSponsor, Guid> sponsors,
        IRepository<MemberLedgerEntry, Guid> entries
    )
        : base(repository, ErpPermissions.Pensions.Default)
    {
        _setupManager = setupManager;
        _schemeManager = schemeManager;
        _noSeriesManager = noSeriesManager;
        _memberLedger = memberLedger;
        _sponsors = sponsors;
        _entries = entries;
    }

    public override async Task<PensionMemberDto> CreateAsync(CreateUpdatePensionMemberDto input)
    {
        await CheckCreatePolicyAsync();

        var sponsor = await GetSponsorAsync(input.SponsorNo);
        var scheme = await _schemeManager.GetOpenAsync(sponsor.SchemeCode);

        var setup = await _setupManager.GetAsync();
        var no = (await _noSeriesManager.ResolveNoAsync(setup.MemberNos, input.No, Clock.Now)).ToUpperInvariant();
        await EnsureNoIsUniqueAsync(no, null);

        var member = new PensionMember(GuidGenerator.Create(), no, sponsor.SchemeCode, sponsor.No, input.FirstName, input.LastName);
        Apply(member, input, scheme);

        await Repository.InsertAsync(member, autoSave: true);
        return await MapToGetOutputDtoAsync(member);
    }

    public override async Task<PensionMemberDto> UpdateAsync(Guid id, CreateUpdatePensionMemberDto input)
    {
        await CheckUpdatePolicyAsync();

        var member = await GetEntityByIdAsync(id);
        var sponsor = await GetSponsorAsync(input.SponsorNo);

        // A member's fund is recorded under the scheme it was paid into; a member with a fund moves
        // to another scheme by a transfer, not by changing the card.
        if (member.SchemeCode != sponsor.SchemeCode && await _entries.AnyAsync(e => e.MemberNo == member.No))
        {
            throw new BusinessException(ErpErrorCodes.Pensions.MemberHasEntries).WithData("memberNo", member.No);
        }

        var scheme = await _schemeManager.GetAsync(sponsor.SchemeCode);
        member.SetScheme(sponsor.SchemeCode, sponsor.No);
        Apply(member, input, scheme);

        await Repository.UpdateAsync(member, autoSave: true);
        return await MapToGetOutputDtoAsync(member);
    }

    /// <summary>A member with a fund keeps their history.</summary>
    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var member = await GetEntityByIdAsync(id);
        if (await _entries.AnyAsync(e => e.MemberNo == member.No))
        {
            throw new BusinessException(ErpErrorCodes.Pensions.MemberHasEntries).WithData("memberNo", member.No);
        }

        await Repository.DeleteAsync(id, autoSave: true);
    }

    public async Task<MemberBalanceDto> GetBalanceAsync(Guid id, DateTime? asOfDate = null)
    {
        await CheckGetPolicyAsync();

        var member = await GetEntityByIdAsync(id);
        var asOf = asOfDate?.Date ?? Clock.Now.Date;
        var balances = await _memberLedger.GetBalancesAsync(member.No, asOf);

        return new MemberBalanceDto
        {
            MemberNo = member.No,
            AsOfDate = asOf,
            Total = balances.Total,
            Employee = balances.Employee,
            Employer = balances.Employer,
            Registered = balances.Registered,
            Unregistered = balances.Unregistered,
            Lines = MoneyType.All
                .Select(m => new MemberBalanceLineDto { ContributionType = m.ContributionType, ExemptionType = m.ExemptionType, Amount = balances.Of(m) })
                .Where(l => l.Amount != 0m)
                .ToList(),
        };
    }

    protected override async Task<IQueryable<PensionMember>> CreateFilteredQueryAsync(GetPensionMemberListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var scheme = input.SchemeCode?.Trim().ToUpperInvariant();
        var sponsor = input.SponsorNo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!scheme.IsNullOrEmpty(), x => x.SchemeCode == scheme)
            .WhereIf(!sponsor.IsNullOrEmpty(), x => x.SponsorNo == sponsor)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status.Value)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.No.ToLower().Contains(filter)
                    || x.FullName.ToLower().Contains(filter)
                    || (x.NationalId != null && x.NationalId.ToLower().Contains(filter))
                    || (x.PayrollNo != null && x.PayrollNo.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<PensionMember> ApplyDefaultSorting(IQueryable<PensionMember> query) => query.OrderBy(x => x.No);

    private async Task<PensionSponsor> GetSponsorAsync(string sponsorNo)
    {
        var no = CodeTableEntity.NormalizeCode(sponsorNo);
        var sponsor = await _sponsors.FirstOrDefaultAsync(s => s.No == no)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Pension Sponsor").WithData("code", no ?? string.Empty);

        return sponsor.Blocked
            ? throw new BusinessException(ErpErrorCodes.Pensions.SponsorBlocked).WithData("sponsorNo", sponsor.No)
            : sponsor;
    }

    private async Task EnsureNoIsUniqueAsync(string no, Guid? exceptId)
    {
        if (await Repository.AnyAsync(x => x.No == no && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Pension Member").WithData("key", no);
        }
    }

    private static void Apply(PensionMember member, CreateUpdatePensionMemberDto input, PensionScheme scheme)
    {
        member.SetName(input.FirstName, input.OtherName, input.LastName);
        member.SetPersonal(input.NationalId, input.TaxPinNo, input.Gender, input.DateOfBirth, input.MaritalStatus, scheme.NormalRetirementAge);
        member.SetEmployment(input.PayrollNo, input.Designation, input.DateOfEmployment, input.JoinSchemeDate, input.CurrentSalary);
        member.SetStatus(input.Status, input.ContributionStatus);
        member.SetContact(input.Address, input.City, input.PhoneNo, input.Email);
        member.SetBank(input.BankName, input.BankBranch, input.BankAccountNo);
    }
}

/// <summary>The member ledger: every movement of every member's fund.</summary>
public class MemberLedgerEntryAppService
    : ErpReadOnlyAppService<MemberLedgerEntry, MemberLedgerEntryDto, Guid, GetMemberLedgerEntryListInput>,
        IMemberLedgerEntryAppService
{
    public MemberLedgerEntryAppService(IRepository<MemberLedgerEntry, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Pensions.Default;
        GetListPolicyName = ErpPermissions.Pensions.Default;
    }

    protected override async Task<IQueryable<MemberLedgerEntry>> CreateFilteredQueryAsync(GetMemberLedgerEntryListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var member = input.MemberNo?.Trim().ToUpperInvariant();
        var scheme = input.SchemeCode?.Trim().ToUpperInvariant();
        var document = input.DocumentNo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!member.IsNullOrEmpty(), x => x.MemberNo == member)
            .WhereIf(!scheme.IsNullOrEmpty(), x => x.SchemeCode == scheme)
            .WhereIf(!document.IsNullOrEmpty(), x => x.DocumentNo == document)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.MemberNo.ToLower().Contains(filter)
                    || x.DocumentNo.ToLower().Contains(filter)
                    || (x.Description != null && x.Description.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<MemberLedgerEntry> ApplyDefaultSorting(IQueryable<MemberLedgerEntry> query) =>
        query.OrderByDescending(x => x.EntryNo);
}

/// <summary>Contribution schedules: prepared, released and posted.</summary>
public class PensionContributionAppService
    : ErpTableAppService<PensionContributionHeader, PensionContributionHeaderDto, GetPensionContributionListInput, CreateUpdatePensionContributionHeaderDto>,
        IPensionContributionAppService
{
    private readonly PensionContributionEngine _engine;
    private readonly PensionSetupManager _setupManager;
    private readonly PensionSchemeManager _schemeManager;
    private readonly NoSeriesManager _noSeriesManager;
    private readonly IRepository<PensionSponsor, Guid> _sponsors;
    private readonly IRepository<PensionContributionLine, Guid> _lines;

    public PensionContributionAppService(
        IRepository<PensionContributionHeader, Guid> repository,
        PensionContributionEngine engine,
        PensionSetupManager setupManager,
        PensionSchemeManager schemeManager,
        NoSeriesManager noSeriesManager,
        IRepository<PensionSponsor, Guid> sponsors,
        IRepository<PensionContributionLine, Guid> lines
    )
        : base(repository, ErpPermissions.Pensions.Default)
    {
        _engine = engine;
        _setupManager = setupManager;
        _schemeManager = schemeManager;
        _noSeriesManager = noSeriesManager;
        _sponsors = sponsors;
        _lines = lines;
    }

    public override async Task<PensionContributionHeaderDto> CreateAsync(CreateUpdatePensionContributionHeaderDto input)
    {
        await CheckCreatePolicyAsync();

        var sponsor = await GetSponsorAsync(input.SponsorNo);
        await _schemeManager.GetOpenAsync(sponsor.SchemeCode);

        var setup = await _setupManager.GetAsync();
        var postingDate = input.PostingDate == default ? Clock.Now.Date : input.PostingDate;
        var no = (await _noSeriesManager.ResolveNoAsync(setup.ContributionNos, input.No, postingDate)).ToUpperInvariant();

        if (await Repository.AnyAsync(x => x.No == no))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Contribution Schedule").WithData("key", no);
        }

        var header = new PensionContributionHeader(
            GuidGenerator.Create(),
            no,
            sponsor,
            postingDate,
            input.ContributionPeriod == default ? postingDate : input.ContributionPeriod
        );
        header.SetDetails(input.Description, input.ContributionMode);

        await Repository.InsertAsync(header, autoSave: true);
        return await MapToGetOutputDtoAsync(header);
    }

    public override async Task<PensionContributionHeaderDto> UpdateAsync(Guid id, CreateUpdatePensionContributionHeaderDto input)
    {
        await CheckUpdatePolicyAsync();

        var header = await GetEntityByIdAsync(id);
        var sponsor = await GetSponsorAsync(input.SponsorNo);

        // The lines are members of the schedule's sponsor; another sponsor would orphan them.
        if (header.SponsorNo != sponsor.No && await _lines.AnyAsync(l => l.DocumentNo == header.No))
        {
            throw new BusinessException(ErpErrorCodes.Pensions.ScheduleHasLines).WithData("documentNo", header.No);
        }

        header.SetSponsor(sponsor);
        header.SetDates(input.PostingDate, input.ContributionPeriod);
        header.SetDetails(input.Description, input.ContributionMode);

        await Repository.UpdateAsync(header, autoSave: true);
        return await MapToGetOutputDtoAsync(header);
    }

    /// <summary>A posted schedule is the source of ledger entries and stays; any other goes with its lines.</summary>
    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var header = await GetEntityByIdAsync(id);
        header.EnsureOpen();

        await _lines.DeleteAsync(l => l.DocumentNo == header.No, autoSave: true);
        await Repository.DeleteAsync(id, autoSave: true);
    }

    [Authorize(ErpPermissions.Pensions.Update)]
    public async Task<PensionContributionHeaderDto> SuggestLinesAsync(Guid id)
    {
        var header = await GetEntityByIdAsync(id);
        await _engine.SuggestLinesAsync(header);
        return await MapToGetOutputDtoAsync(header);
    }

    [Authorize(ErpPermissions.Pensions.Update)]
    public async Task<PensionContributionHeaderDto> ReleaseAsync(Guid id)
    {
        var header = await GetEntityByIdAsync(id);
        await _engine.UpdateTotalsAsync(header);
        await _engine.ReleaseAsync(header);
        return await MapToGetOutputDtoAsync(header);
    }

    [Authorize(ErpPermissions.Pensions.Update)]
    public async Task<PensionContributionHeaderDto> ReopenAsync(Guid id)
    {
        var header = await GetEntityByIdAsync(id);
        header.Reopen();
        await Repository.UpdateAsync(header, autoSave: true);
        return await MapToGetOutputDtoAsync(header);
    }

    [Authorize(ErpPermissions.Pensions.Post)]
    public async Task<PensionContributionHeaderDto> RunPostingAsync(Guid id)
    {
        var header = await GetEntityByIdAsync(id);
        await _engine.PostAsync(header);
        return await MapToGetOutputDtoAsync(header);
    }

    protected override async Task<IQueryable<PensionContributionHeader>> CreateFilteredQueryAsync(GetPensionContributionListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var sponsor = input.SponsorNo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!sponsor.IsNullOrEmpty(), x => x.SponsorNo == sponsor)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status.Value)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.No.ToLower().Contains(filter)
                    || x.SponsorNo.ToLower().Contains(filter)
                    || (x.SponsorName != null && x.SponsorName.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<PensionContributionHeader> ApplyDefaultSorting(IQueryable<PensionContributionHeader> query) =>
        query.OrderByDescending(x => x.PostingDate).ThenByDescending(x => x.No);

    private async Task<PensionSponsor> GetSponsorAsync(string sponsorNo)
    {
        var no = CodeTableEntity.NormalizeCode(sponsorNo);
        var sponsor = await _sponsors.FirstOrDefaultAsync(s => s.No == no)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Pension Sponsor").WithData("code", no ?? string.Empty);

        return sponsor.Blocked
            ? throw new BusinessException(ErpErrorCodes.Pensions.SponsorBlocked).WithData("sponsorNo", sponsor.No)
            : sponsor;
    }
}

/// <summary>The lines of contribution schedules. They can be changed only while their schedule is open.</summary>
public class PensionContributionLineAppService
    : ErpTableAppService<PensionContributionLine, PensionContributionLineDto, GetPensionContributionLineListInput, CreateUpdatePensionContributionLineDto>,
        IPensionContributionLineAppService
{
    private readonly PensionContributionEngine _engine;
    private readonly IRepository<PensionContributionHeader, Guid> _headers;
    private readonly IRepository<PensionMember, Guid> _members;

    public PensionContributionLineAppService(
        IRepository<PensionContributionLine, Guid> repository,
        PensionContributionEngine engine,
        IRepository<PensionContributionHeader, Guid> headers,
        IRepository<PensionMember, Guid> members
    )
        : base(repository, ErpPermissions.Pensions.Default)
    {
        _engine = engine;
        _headers = headers;
        _members = members;
    }

    public override async Task<PensionContributionLineDto> CreateAsync(CreateUpdatePensionContributionLineDto input)
    {
        await CheckCreatePolicyAsync();

        var header = await GetOpenHeaderAsync(input.DocumentNo);
        var member = await GetMemberAsync(input.MemberNo, header);

        var lineNo = input.LineNo;
        if (lineNo <= 0)
        {
            var query = await Repository.GetQueryableAsync();
            var last = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.DocumentNo == header.No).OrderByDescending(x => x.LineNo));
            lineNo = (last?.LineNo ?? 0) + 10000;
        }

        await EnsureMemberOnceAsync(header.No, member.No, null);

        var line = new PensionContributionLine(GuidGenerator.Create(), header.No, lineNo, member);
        line.SetAmounts(input.BasicSalary, AmountsOf(input));

        await Repository.InsertAsync(line, autoSave: true);
        await _engine.UpdateTotalsAsync(header);
        return await MapToGetOutputDtoAsync(line);
    }

    public override async Task<PensionContributionLineDto> UpdateAsync(Guid id, CreateUpdatePensionContributionLineDto input)
    {
        await CheckUpdatePolicyAsync();

        var line = await GetEntityByIdAsync(id);
        var header = await GetOpenHeaderAsync(line.DocumentNo);
        var member = await GetMemberAsync(input.MemberNo, header);
        await EnsureMemberOnceAsync(header.No, member.No, id);

        line.SetMember(member);
        line.SetAmounts(input.BasicSalary, AmountsOf(input));

        await Repository.UpdateAsync(line, autoSave: true);
        await _engine.UpdateTotalsAsync(header);
        return await MapToGetOutputDtoAsync(line);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var line = await GetEntityByIdAsync(id);
        var header = await GetOpenHeaderAsync(line.DocumentNo);

        await Repository.DeleteAsync(id, autoSave: true);
        await _engine.UpdateTotalsAsync(header);
    }

    protected override async Task<IQueryable<PensionContributionLine>> CreateFilteredQueryAsync(GetPensionContributionLineListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var document = input.DocumentNo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!document.IsNullOrEmpty(), x => x.DocumentNo == document)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.DocumentNo.ToLower().Contains(filter)
                    || x.MemberNo.ToLower().Contains(filter)
                    || (x.MemberName != null && x.MemberName.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<PensionContributionLine> ApplyDefaultSorting(IQueryable<PensionContributionLine> query) =>
        query.OrderBy(x => x.DocumentNo).ThenBy(x => x.LineNo);

    private async Task<PensionContributionHeader> GetOpenHeaderAsync(string documentNo)
    {
        var no = CodeTableEntity.NormalizeCode(documentNo);
        var header = await _headers.FirstOrDefaultAsync(h => h.No == no)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Contribution Schedule").WithData("code", no ?? string.Empty);

        header.EnsureOpen();
        return header;
    }

    private async Task<PensionMember> GetMemberAsync(string memberNo, PensionContributionHeader header)
    {
        var no = CodeTableEntity.NormalizeCode(memberNo);
        var member = await _members.FirstOrDefaultAsync(m => m.No == no)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Pension Member").WithData("code", no ?? string.Empty);

        return member.SchemeCode == header.SchemeCode
            ? member
            : throw new BusinessException(ErpErrorCodes.Pensions.MemberNotInScheme).WithData("memberNo", member.No).WithData("scheme", header.SchemeCode);
    }

    private async Task EnsureMemberOnceAsync(string documentNo, string memberNo, Guid? exceptId)
    {
        if (await Repository.AnyAsync(x => x.DocumentNo == documentNo && x.MemberNo == memberNo && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.Pensions.MemberDuplicated).WithData("memberNo", memberNo).WithData("documentNo", documentNo);
        }
    }

    private static decimal[] AmountsOf(CreateUpdatePensionContributionLineDto input) =>
    [
        input.EmployeeTaxExempt,
        input.EmployeeNonTaxExempt,
        input.EmployeeAvcTaxExempt,
        input.EmployeeAvcNonTaxExempt,
        input.EmployerTaxExempt,
        input.EmployerNonTaxExempt,
        input.EmployerAvcTaxExempt,
        input.EmployerAvcNonTaxExempt,
    ];
}

/// <summary>Member exits (claims): calculated, approved and posted.</summary>
public class MemberExitAppService
    : ErpTableAppService<MemberExit, MemberExitDto, GetMemberExitListInput, CreateUpdateMemberExitDto>,
        IMemberExitAppService
{
    private readonly MemberExitEngine _engine;
    private readonly PensionSetupManager _setupManager;
    private readonly NoSeriesManager _noSeriesManager;
    private readonly IRepository<PensionMember, Guid> _members;

    public MemberExitAppService(
        IRepository<MemberExit, Guid> repository,
        MemberExitEngine engine,
        PensionSetupManager setupManager,
        NoSeriesManager noSeriesManager,
        IRepository<PensionMember, Guid> members
    )
        : base(repository, ErpPermissions.Pensions.Default)
    {
        _engine = engine;
        _setupManager = setupManager;
        _noSeriesManager = noSeriesManager;
        _members = members;
    }

    public override async Task<MemberExitDto> CreateAsync(CreateUpdateMemberExitDto input)
    {
        await CheckCreatePolicyAsync();
        await CodeTableChecker.EnsureExistsAsync<ExitReason>(input.ReasonCode);

        var member = await GetMemberAsync(input.MemberNo);
        var exitDate = input.ExitDate == default ? Clock.Now.Date : input.ExitDate;

        var setup = await _setupManager.GetAsync();
        var no = (await _noSeriesManager.ResolveNoAsync(setup.ExitNos, input.No, exitDate)).ToUpperInvariant();

        if (await Repository.AnyAsync(x => x.No == no))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Member Exit").WithData("key", no);
        }

        var exit = new MemberExit(GuidGenerator.Create(), no, member, input.ReasonCode, exitDate);
        exit.Set(member, input.ReasonCode, input.WithdrawalType, exitDate, input.DateOfCalculation, input.Comment);

        await Repository.InsertAsync(exit, autoSave: true);
        await _engine.CalculateAsync(exit);
        return await MapToGetOutputDtoAsync(exit);
    }

    public override async Task<MemberExitDto> UpdateAsync(Guid id, CreateUpdateMemberExitDto input)
    {
        await CheckUpdatePolicyAsync();
        await CodeTableChecker.EnsureExistsAsync<ExitReason>(input.ReasonCode);

        var exit = await GetEntityByIdAsync(id);
        var member = await GetMemberAsync(input.MemberNo);
        exit.Set(member, input.ReasonCode, input.WithdrawalType, input.ExitDate, input.DateOfCalculation, input.Comment);

        await Repository.UpdateAsync(exit, autoSave: true);
        await _engine.CalculateAsync(exit);
        return await MapToGetOutputDtoAsync(exit);
    }

    /// <summary>A posted exit paid the member out and stays on record.</summary>
    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var exit = await GetEntityByIdAsync(id);
        if (exit.Status == MemberExitStatus.Posted)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.DocumentNotOpen).WithData("documentNo", exit.No);
        }

        await Repository.DeleteAsync(id, autoSave: true);
    }

    [Authorize(ErpPermissions.Pensions.Update)]
    public async Task<MemberExitDto> CalculateAsync(Guid id)
    {
        var exit = await GetEntityByIdAsync(id);
        await _engine.CalculateAsync(exit);
        return await MapToGetOutputDtoAsync(exit);
    }

    [Authorize(ErpPermissions.Pensions.Update)]
    public async Task<MemberExitDto> ApproveAsync(Guid id)
    {
        var exit = await GetEntityByIdAsync(id);

        // Approved on the figures as they stand now, not as they stood when the exit was keyed in.
        await _engine.CalculateAsync(exit);
        exit.Approve();

        await Repository.UpdateAsync(exit, autoSave: true);
        return await MapToGetOutputDtoAsync(exit);
    }

    [Authorize(ErpPermissions.Pensions.Update)]
    public async Task<MemberExitDto> ReopenAsync(Guid id)
    {
        var exit = await GetEntityByIdAsync(id);
        exit.Reopen();

        await Repository.UpdateAsync(exit, autoSave: true);
        return await MapToGetOutputDtoAsync(exit);
    }

    [Authorize(ErpPermissions.Pensions.Post)]
    public async Task<MemberExitDto> RunPostingAsync(Guid id, PostMemberExitInput input)
    {
        var exit = await GetEntityByIdAsync(id);
        await _engine.PostAsync(exit, input?.PostingDate ?? exit.ExitDate);
        return await MapToGetOutputDtoAsync(exit);
    }

    [Authorize(ErpPermissions.Pensions.Post)]
    public async Task<MemberExitDto> RaisePaymentVoucherAsync(Guid id)
    {
        var exit = await GetEntityByIdAsync(id);
        await LazyServiceProvider.LazyGetRequiredService<PensionPaymentManager>().RaiseForExitAsync(exit, Clock.Now.Date);
        return await MapToGetOutputDtoAsync(exit);
    }

    protected override async Task<IQueryable<MemberExit>> CreateFilteredQueryAsync(GetMemberExitListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var member = input.MemberNo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!member.IsNullOrEmpty(), x => x.MemberNo == member)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status.Value)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.No.ToLower().Contains(filter)
                    || x.MemberNo.ToLower().Contains(filter)
                    || (x.MemberName != null && x.MemberName.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<MemberExit> ApplyDefaultSorting(IQueryable<MemberExit> query) =>
        query.OrderByDescending(x => x.ExitDate).ThenByDescending(x => x.No);

    private async Task<PensionMember> GetMemberAsync(string memberNo)
    {
        var no = CodeTableEntity.NormalizeCode(memberNo);
        return await _members.FirstOrDefaultAsync(m => m.No == no)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Pension Member").WithData("code", no ?? string.Empty);
    }
}
