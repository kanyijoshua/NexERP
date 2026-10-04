using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Pensions;

/// <summary>Pensioners: the people a scheme pays a monthly pension to.</summary>
public class PensionerAppService
    : ErpTableAppService<Pensioner, PensionerDto, GetPensionerListInput, CreateUpdatePensionerDto>,
        IPensionerAppService
{
    private readonly PensionSetupManager _setupManager;
    private readonly PensionSchemeManager _schemeManager;
    private readonly NoSeriesManager _noSeriesManager;
    private readonly IRepository<PensionMember, Guid> _members;
    private readonly IRepository<PensionPayrollLine, Guid> _payrollLines;

    public PensionerAppService(
        IRepository<Pensioner, Guid> repository,
        PensionSetupManager setupManager,
        PensionSchemeManager schemeManager,
        NoSeriesManager noSeriesManager,
        IRepository<PensionMember, Guid> members,
        IRepository<PensionPayrollLine, Guid> payrollLines
    )
        : base(repository, ErpPermissions.Pensions.Default)
    {
        _setupManager = setupManager;
        _schemeManager = schemeManager;
        _noSeriesManager = noSeriesManager;
        _members = members;
        _payrollLines = payrollLines;
    }

    public override async Task<PensionerDto> CreateAsync(CreateUpdatePensionerDto input)
    {
        await CheckCreatePolicyAsync();

        var (scheme, name, member) = await ResolveAsync(input);
        var setup = await _setupManager.GetAsync();
        var startDate = input.StartDate == default ? Clock.Now.Date : input.StartDate;
        var no = (await _noSeriesManager.ResolveNoAsync(setup.PensionerNos, input.No, startDate)).ToUpperInvariant();

        if (await Repository.AnyAsync(x => x.No == no))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Pensioner").WithData("key", no);
        }

        var pensioner = new Pensioner(GuidGenerator.Create(), no, scheme, name, startDate);
        Apply(pensioner, input, scheme, name, member, startDate);

        await Repository.InsertAsync(pensioner, autoSave: true);
        return await MapToGetOutputDtoAsync(pensioner);
    }

    public override async Task<PensionerDto> UpdateAsync(Guid id, CreateUpdatePensionerDto input)
    {
        await CheckUpdatePolicyAsync();

        var pensioner = await GetEntityByIdAsync(id);
        var (scheme, name, member) = await ResolveAsync(input);

        // Pensions already paid were paid out of the scheme the pensioner was in.
        if (pensioner.SchemeCode != scheme && await _payrollLines.AnyAsync(l => l.PensionerNo == pensioner.No))
        {
            throw new BusinessException(ErpErrorCodes.Pensions.PensionerHasPayroll).WithData("pensionerNo", pensioner.No);
        }

        Apply(pensioner, input, scheme, name, member, input.StartDate == default ? pensioner.StartDate : input.StartDate);

        await Repository.UpdateAsync(pensioner, autoSave: true);
        return await MapToGetOutputDtoAsync(pensioner);
    }

    /// <summary>A pensioner who has been paid keeps their record; cease the pension instead.</summary>
    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var pensioner = await GetEntityByIdAsync(id);
        if (await _payrollLines.AnyAsync(l => l.PensionerNo == pensioner.No))
        {
            throw new BusinessException(ErpErrorCodes.Pensions.PensionerHasPayroll).WithData("pensionerNo", pensioner.No);
        }

        await Repository.DeleteAsync(id, autoSave: true);
    }

    protected override async Task<IQueryable<Pensioner>> CreateFilteredQueryAsync(GetPensionerListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var scheme = input.SchemeCode?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!scheme.IsNullOrEmpty(), x => x.SchemeCode == scheme)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status.Value)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.No.ToLower().Contains(filter)
                    || x.Name.ToLower().Contains(filter)
                    || (x.MemberNo != null && x.MemberNo.ToLower().Contains(filter))
                    || (x.NationalId != null && x.NationalId.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<Pensioner> ApplyDefaultSorting(IQueryable<Pensioner> query) => query.OrderBy(x => x.No);

    /// <summary>The scheme and name of the pensioner: those given, or those of the member the pension arises from.</summary>
    private async Task<(string Scheme, string Name, PensionMember Member)> ResolveAsync(CreateUpdatePensionerDto input)
    {
        var memberNo = CodeTableEntity.NormalizeCode(input.MemberNo);
        PensionMember member = null;
        if (memberNo != null)
        {
            member = await _members.FirstOrDefaultAsync(m => m.No == memberNo)
                ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Pension Member").WithData("code", memberNo);
        }

        var scheme = (await _schemeManager.GetAsync(input.SchemeCode.IsNullOrWhiteSpace() ? member?.SchemeCode : input.SchemeCode)).Code;
        var name = input.Name.IsNullOrWhiteSpace() ? member?.FullName : input.Name;

        return (scheme, Check.NotNullOrWhiteSpace(name, nameof(input.Name)), member);
    }

    private static void Apply(Pensioner pensioner, CreateUpdatePensionerDto input, string scheme, string name, PensionMember member, DateTime startDate)
    {
        pensioner.Set(
            scheme,
            member?.No,
            name,
            input.NationalId.IsNullOrWhiteSpace() ? member?.NationalId : input.NationalId,
            input.TaxPinNo.IsNullOrWhiteSpace() ? member?.TaxPinNo : input.TaxPinNo,
            input.DateOfBirth ?? member?.DateOfBirth
        );
        pensioner.SetPension(input.MonthlyPension, startDate, input.EndDate, input.Status);
        pensioner.SetContact(
            input.PhoneNo.IsNullOrWhiteSpace() ? member?.PhoneNo : input.PhoneNo,
            input.Email.IsNullOrWhiteSpace() ? member?.Email : input.Email,
            input.BankName.IsNullOrWhiteSpace() ? member?.BankName : input.BankName,
            input.BankBranch.IsNullOrWhiteSpace() ? member?.BankBranch : input.BankBranch,
            input.BankAccountNo.IsNullOrWhiteSpace() ? member?.BankAccountNo : input.BankAccountNo
        );
    }
}

/// <summary>Pension payrolls: prepared, released, posted and then paid through a payment voucher.</summary>
public class PensionPayrollAppService
    : ErpTableAppService<PensionPayrollHeader, PensionPayrollHeaderDto, GetPensionPayrollListInput, CreateUpdatePensionPayrollHeaderDto>,
        IPensionPayrollAppService
{
    private readonly PensionPayrollEngine _engine;
    private readonly PensionPaymentManager _paymentManager;
    private readonly PensionSetupManager _setupManager;
    private readonly PensionSchemeManager _schemeManager;
    private readonly NoSeriesManager _noSeriesManager;
    private readonly IRepository<PensionPayrollLine, Guid> _lines;

    public PensionPayrollAppService(
        IRepository<PensionPayrollHeader, Guid> repository,
        PensionPayrollEngine engine,
        PensionPaymentManager paymentManager,
        PensionSetupManager setupManager,
        PensionSchemeManager schemeManager,
        NoSeriesManager noSeriesManager,
        IRepository<PensionPayrollLine, Guid> lines
    )
        : base(repository, ErpPermissions.Pensions.Default)
    {
        _engine = engine;
        _paymentManager = paymentManager;
        _setupManager = setupManager;
        _schemeManager = schemeManager;
        _noSeriesManager = noSeriesManager;
        _lines = lines;
    }

    public override async Task<PensionPayrollHeaderDto> CreateAsync(CreateUpdatePensionPayrollHeaderDto input)
    {
        await CheckCreatePolicyAsync();

        var scheme = await _schemeManager.GetAsync(input.SchemeCode);
        var setup = await _setupManager.GetAsync();
        var postingDate = input.PostingDate == default ? Clock.Now.Date : input.PostingDate;
        var no = (await _noSeriesManager.ResolveNoAsync(setup.PayrollNos, input.No, postingDate)).ToUpperInvariant();

        if (await Repository.AnyAsync(x => x.No == no))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Pension Payroll").WithData("key", no);
        }

        var payPeriod = input.PayPeriod == default ? postingDate : input.PayPeriod;
        var header = new PensionPayrollHeader(GuidGenerator.Create(), no, scheme.Code, payPeriod, postingDate);
        header.Set(scheme.Code, payPeriod, postingDate, input.Description, input.TaxRatePct, input.TaxFreeAmount);

        await Repository.InsertAsync(header, autoSave: true);
        return await MapToGetOutputDtoAsync(header);
    }

    public override async Task<PensionPayrollHeaderDto> UpdateAsync(Guid id, CreateUpdatePensionPayrollHeaderDto input)
    {
        await CheckUpdatePolicyAsync();

        var header = await GetEntityByIdAsync(id);
        var scheme = await _schemeManager.GetAsync(input.SchemeCode);

        // The lines are pensioners of the payroll's scheme; another scheme would orphan them.
        if (header.SchemeCode != scheme.Code && await _lines.AnyAsync(l => l.DocumentNo == header.No))
        {
            throw new BusinessException(ErpErrorCodes.Pensions.ScheduleHasLines).WithData("documentNo", header.No);
        }

        header.Set(
            scheme.Code,
            input.PayPeriod == default ? header.PayPeriod : input.PayPeriod,
            input.PostingDate == default ? header.PostingDate : input.PostingDate,
            input.Description,
            input.TaxRatePct,
            input.TaxFreeAmount
        );

        await Repository.UpdateAsync(header, autoSave: true);
        return await MapToGetOutputDtoAsync(header);
    }

    /// <summary>A posted payroll is the source of G/L entries and stays; an open one goes with its lines.</summary>
    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var header = await GetEntityByIdAsync(id);
        header.EnsureOpen();

        await _lines.DeleteAsync(l => l.DocumentNo == header.No, autoSave: true);
        await Repository.DeleteAsync(id, autoSave: true);
    }

    [Authorize(ErpPermissions.Pensions.Update)]
    public async Task<PensionPayrollHeaderDto> SuggestLinesAsync(Guid id)
    {
        var header = await GetEntityByIdAsync(id);
        await _engine.SuggestLinesAsync(header);
        return await MapToGetOutputDtoAsync(header);
    }

    [Authorize(ErpPermissions.Pensions.Update)]
    public async Task<PensionPayrollHeaderDto> ReleaseAsync(Guid id)
    {
        var header = await GetEntityByIdAsync(id);
        await _engine.UpdateTotalsAsync(header);
        await _engine.ReleaseAsync(header);
        return await MapToGetOutputDtoAsync(header);
    }

    [Authorize(ErpPermissions.Pensions.Update)]
    public async Task<PensionPayrollHeaderDto> ReopenAsync(Guid id)
    {
        var header = await GetEntityByIdAsync(id);
        header.Reopen();
        await Repository.UpdateAsync(header, autoSave: true);
        return await MapToGetOutputDtoAsync(header);
    }

    [Authorize(ErpPermissions.Pensions.Post)]
    public async Task<PensionPayrollHeaderDto> RunPostingAsync(Guid id)
    {
        var header = await GetEntityByIdAsync(id);
        await _engine.PostAsync(header);
        return await MapToGetOutputDtoAsync(header);
    }

    [Authorize(ErpPermissions.Pensions.Post)]
    public async Task<PensionPayrollHeaderDto> RaisePaymentVoucherAsync(Guid id)
    {
        var header = await GetEntityByIdAsync(id);
        await _paymentManager.RaiseForPayrollAsync(header, Clock.Now.Date);
        return await MapToGetOutputDtoAsync(header);
    }

    protected override async Task<IQueryable<PensionPayrollHeader>> CreateFilteredQueryAsync(GetPensionPayrollListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var scheme = input.SchemeCode?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!scheme.IsNullOrEmpty(), x => x.SchemeCode == scheme)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status.Value)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.No.ToLower().Contains(filter)
                    || x.SchemeCode.ToLower().Contains(filter)
                    || (x.Description != null && x.Description.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<PensionPayrollHeader> ApplyDefaultSorting(IQueryable<PensionPayrollHeader> query) =>
        query.OrderByDescending(x => x.PayPeriod).ThenByDescending(x => x.No);
}

/// <summary>The lines of pension payrolls. They can be changed only while their payroll is open.</summary>
public class PensionPayrollLineAppService
    : ErpTableAppService<PensionPayrollLine, PensionPayrollLineDto, GetPensionPayrollLineListInput, CreateUpdatePensionPayrollLineDto>,
        IPensionPayrollLineAppService
{
    private readonly PensionPayrollEngine _engine;
    private readonly IRepository<PensionPayrollHeader, Guid> _headers;
    private readonly IRepository<Pensioner, Guid> _pensioners;

    public PensionPayrollLineAppService(
        IRepository<PensionPayrollLine, Guid> repository,
        PensionPayrollEngine engine,
        IRepository<PensionPayrollHeader, Guid> headers,
        IRepository<Pensioner, Guid> pensioners
    )
        : base(repository, ErpPermissions.Pensions.Default)
    {
        _engine = engine;
        _headers = headers;
        _pensioners = pensioners;
    }

    public override async Task<PensionPayrollLineDto> CreateAsync(CreateUpdatePensionPayrollLineDto input)
    {
        await CheckCreatePolicyAsync();

        var header = await GetOpenHeaderAsync(input.DocumentNo);
        var pensioner = await GetPensionerAsync(input.PensionerNo, header);
        await EnsurePensionerOnceAsync(header.No, pensioner.No, null);

        var lineNo = input.LineNo;
        if (lineNo <= 0)
        {
            var query = await Repository.GetQueryableAsync();
            var last = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.DocumentNo == header.No).OrderByDescending(x => x.LineNo));
            lineNo = (last?.LineNo ?? 0) + 10000;
        }

        var line = new PensionPayrollLine(GuidGenerator.Create(), header.No, lineNo, pensioner);
        SetAmounts(line, header, pensioner, input);

        await Repository.InsertAsync(line, autoSave: true);
        await _engine.UpdateTotalsAsync(header);
        return await MapToGetOutputDtoAsync(line);
    }

    public override async Task<PensionPayrollLineDto> UpdateAsync(Guid id, CreateUpdatePensionPayrollLineDto input)
    {
        await CheckUpdatePolicyAsync();

        var line = await GetEntityByIdAsync(id);
        var header = await GetOpenHeaderAsync(line.DocumentNo);
        var pensioner = await GetPensionerAsync(input.PensionerNo, header);
        await EnsurePensionerOnceAsync(header.No, pensioner.No, id);

        line.SetPensioner(pensioner);
        SetAmounts(line, header, pensioner, input);

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

    protected override async Task<IQueryable<PensionPayrollLine>> CreateFilteredQueryAsync(GetPensionPayrollLineListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var document = input.DocumentNo?.Trim().ToUpperInvariant();
        var pensioner = input.PensionerNo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!document.IsNullOrEmpty(), x => x.DocumentNo == document)
            .WhereIf(!pensioner.IsNullOrEmpty(), x => x.PensionerNo == pensioner)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.DocumentNo.ToLower().Contains(filter)
                    || x.PensionerNo.ToLower().Contains(filter)
                    || (x.PensionerName != null && x.PensionerName.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<PensionPayrollLine> ApplyDefaultSorting(IQueryable<PensionPayrollLine> query) =>
        query.OrderBy(x => x.DocumentNo).ThenBy(x => x.LineNo);

    private static void SetAmounts(PensionPayrollLine line, PensionPayrollHeader header, Pensioner pensioner, CreateUpdatePensionPayrollLineDto input)
    {
        var gross = input.GrossPension ?? pensioner.MonthlyPension;
        line.SetAmounts(gross, input.TaxAmount ?? header.TaxOn(gross));
    }

    private async Task<PensionPayrollHeader> GetOpenHeaderAsync(string documentNo)
    {
        var no = CodeTableEntity.NormalizeCode(documentNo);
        var header = await _headers.FirstOrDefaultAsync(h => h.No == no)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Pension Payroll").WithData("code", no ?? string.Empty);

        header.EnsureOpen();
        return header;
    }

    private async Task<Pensioner> GetPensionerAsync(string pensionerNo, PensionPayrollHeader header)
    {
        var no = CodeTableEntity.NormalizeCode(pensionerNo);
        var pensioner = await _pensioners.FirstOrDefaultAsync(p => p.No == no)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Pensioner").WithData("code", no ?? string.Empty);

        return pensioner.SchemeCode == header.SchemeCode
            ? pensioner
            : throw new BusinessException(ErpErrorCodes.Pensions.PensionerNotInScheme).WithData("pensionerNo", pensioner.No).WithData("scheme", header.SchemeCode);
    }

    private async Task EnsurePensionerOnceAsync(string documentNo, string pensionerNo, Guid? exceptId)
    {
        if (await Repository.AnyAsync(x => x.DocumentNo == documentNo && x.PensionerNo == pensionerNo && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.Pensions.PensionerDuplicated).WithData("pensionerNo", pensionerNo).WithData("documentNo", documentNo);
        }
    }
}
