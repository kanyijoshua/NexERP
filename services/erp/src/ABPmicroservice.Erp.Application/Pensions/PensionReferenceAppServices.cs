using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Permissions;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Pensions;

/// <summary>Banks pensioners and other schemes are paid through.</summary>
public class PensionBankAppService : PensionCodeTableAppService<PensionBank, PensionBankDto, CreateUpdatePensionBankDto>, IPensionBankAppService
{
    private readonly IRepository<PensionBankBranch, Guid> _branches;

    public PensionBankAppService(IRepository<PensionBank, Guid> repository, IRepository<PensionBankBranch, Guid> branches)
        : base(repository)
    {
        _branches = branches;
    }

    protected override PensionBank NewEntity(Guid id, CreateUpdatePensionBankDto input) => new(id, input.Code, input.Description);

    protected override Task ApplyAsync(PensionBank entity, CreateUpdatePensionBankDto input)
    {
        entity.Set(input.SwiftCode);
        return Task.CompletedTask;
    }

    /// <summary>A bank goes with its branches.</summary>
    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var bank = await Repository.GetAsync(id);
        await _branches.DeleteAsync(b => b.BankCode == bank.Code);
        await Repository.DeleteAsync(bank, autoSave: true);
    }
}

/// <summary>The branches of the banks.</summary>
public class PensionBankBranchAppService
    : ErpTableAppService<PensionBankBranch, PensionBankBranchDto, GetPensionBankBranchListInput, CreateUpdatePensionBankBranchDto>,
        IPensionBankBranchAppService
{
    public PensionBankBranchAppService(IRepository<PensionBankBranch, Guid> repository)
        : base(repository, ErpPermissions.PensionSetup.Default) { }

    public override async Task<PensionBankBranchDto> CreateAsync(CreateUpdatePensionBankBranchDto input)
    {
        await CheckCreatePolicyAsync();
        await EnsureValidAsync(input, null);

        var branch = new PensionBankBranch(GuidGenerator.Create(), input.BankCode, input.BranchCode, input.Name);
        branch.Set(input.Name, input.SwiftCode);

        await Repository.InsertAsync(branch, autoSave: true);
        return await MapToGetOutputDtoAsync(branch);
    }

    public override async Task<PensionBankBranchDto> UpdateAsync(Guid id, CreateUpdatePensionBankBranchDto input)
    {
        await CheckUpdatePolicyAsync();

        var branch = await GetEntityByIdAsync(id);
        await EnsureValidAsync(input, id);
        branch.SetKey(input.BankCode, input.BranchCode);
        branch.Set(input.Name, input.SwiftCode);

        await Repository.UpdateAsync(branch, autoSave: true);
        return await MapToGetOutputDtoAsync(branch);
    }

    protected override async Task<IQueryable<PensionBankBranch>> CreateFilteredQueryAsync(GetPensionBankBranchListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var bank = input.BankCode?.Trim().ToUpperInvariant();
        var filter = input.Filter?.Trim().ToLower();

        return query
            .WhereIf(!bank.IsNullOrEmpty(), x => x.BankCode == bank)
            .WhereIf(!filter.IsNullOrEmpty(), x => x.BranchCode.ToLower().Contains(filter) || x.Name.ToLower().Contains(filter));
    }

    protected override IQueryable<PensionBankBranch> ApplyDefaultSorting(IQueryable<PensionBankBranch> query) =>
        query.OrderBy(x => x.BankCode).ThenBy(x => x.BranchCode);

    private async Task EnsureValidAsync(CreateUpdatePensionBankBranchDto input, Guid? exceptId)
    {
        await CodeTableChecker.EnsureExistsAsync<PensionBank>(input.BankCode);

        var bank = CodeTableEntity.NormalizeCode(input.BankCode);
        var code = CodeTableEntity.NormalizeCode(input.BranchCode);
        if (await Repository.AnyAsync(x => x.BankCode == bank && x.BranchCode == code && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Bank Branch").WithData("key", $"{bank} {code}");
        }
    }
}

/// <summary>How pensioners are paid.</summary>
public class PensionerPayModeAppService
    : PensionCodeTableAppService<PensionerPayMode, PensionerPayModeDto, CreateUpdatePensionerPayModeDto>,
        IPensionerPayModeAppService
{
    public PensionerPayModeAppService(IRepository<PensionerPayMode, Guid> repository)
        : base(repository) { }

    protected override PensionerPayMode NewEntity(Guid id, CreateUpdatePensionerPayModeDto input) => new(id, input.Code, input.Description);

    protected override Task ApplyAsync(PensionerPayMode entity, CreateUpdatePensionerPayModeDto input)
    {
        entity.Set(input.PaymentType);
        return Task.CompletedTask;
    }
}

/// <summary>Why pensions are suspended.</summary>
public class PensionerSuspensionReasonAppService
    : PensionCodeTableAppService<PensionerSuspensionReason, PensionerSuspensionReasonDto, CreateUpdatePensionerSuspensionReasonDto>,
        IPensionerSuspensionReasonAppService
{
    public PensionerSuspensionReasonAppService(IRepository<PensionerSuspensionReason, Guid> repository)
        : base(repository) { }

    protected override PensionerSuspensionReason NewEntity(Guid id, CreateUpdatePensionerSuspensionReasonDto input) => new(id, input.Code, input.Description);

    protected override Task ApplyAsync(PensionerSuspensionReason entity, CreateUpdatePensionerSuspensionReasonDto input)
    {
        entity.Set(input.LifeCertificate);
        return Task.CompletedTask;
    }
}

/// <summary>Why pensions are revised.</summary>
public class PensionRevisionReasonAppService
    : PensionCodeTableAppService<PensionRevisionReason, CodeTableDto, CreateUpdateCodeTableDto>,
        IPensionRevisionReasonAppService
{
    public PensionRevisionReasonAppService(IRepository<PensionRevisionReason, Guid> repository)
        : base(repository) { }

    protected override PensionRevisionReason NewEntity(Guid id, CreateUpdateCodeTableDto input) => new(id, input.Code, input.Description);
}

/// <summary>Other retirement benefits schemes members transfer between.</summary>
public class OtherPensionSchemeAppService
    : PensionCodeTableAppService<OtherPensionScheme, OtherPensionSchemeDto, CreateUpdateOtherPensionSchemeDto>,
        IOtherPensionSchemeAppService
{
    public OtherPensionSchemeAppService(IRepository<OtherPensionScheme, Guid> repository)
        : base(repository) { }

    protected override OtherPensionScheme NewEntity(Guid id, CreateUpdateOtherPensionSchemeDto input) => new(id, input.Code, input.Description);

    protected override async Task ApplyAsync(OtherPensionScheme entity, CreateUpdateOtherPensionSchemeDto input)
    {
        await LazyServiceProvider.LazyGetRequiredService<PensionBankResolver>().EnsureBranchAsync(input.BankCode, input.BankBranchCode);
        entity.Set(input.RegulatorReferenceNo, input.Address, input.City, input.ContactName, input.PhoneNo, input.Email);
        entity.SetBank(input.BankCode, input.BankBranchCode, input.BankAccountNo);
    }
}

/// <summary>The earnings and deductions pensioners can be given.</summary>
public class PensionerPayItemAppService
    : PensionCodeTableAppService<PensionerPayItem, PensionerPayItemDto, CreateUpdatePensionerPayItemDto>,
        IPensionerPayItemAppService
{
    private readonly IRepository<PensionerPayItemAssignment, Guid> _assignments;

    public PensionerPayItemAppService(IRepository<PensionerPayItem, Guid> repository, IRepository<PensionerPayItemAssignment, Guid> assignments)
        : base(repository)
    {
        _assignments = assignments;
    }

    protected override PensionerPayItem NewEntity(Guid id, CreateUpdatePensionerPayItemDto input) => new(id, input.Code, input.Description);

    protected override async Task ApplyAsync(PensionerPayItem entity, CreateUpdatePensionerPayItemDto input)
    {
        await LazyServiceProvider.LazyGetRequiredService<TableRelationChecker>().EnsureGLAccountsExistAsync(input.AccountNo);
        entity.Set(input.ItemType, input.Calculation, input.Amount, input.Pct, input.Taxable, input.AccountNo, input.Blocked);
    }

    /// <summary>An item pensioners have stays; block it instead.</summary>
    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var item = await Repository.GetAsync(id);
        if (await _assignments.AnyAsync(a => a.PayItemCode == item.Code))
        {
            throw new BusinessException(ErpErrorCodes.Pensions.PayItemInUse).WithData("code", item.Code);
        }

        await Repository.DeleteAsync(item, autoSave: true);
    }
}

/// <summary>The earnings and deductions each pensioner has.</summary>
public class PensionerPayItemAssignmentAppService
    : ErpTableAppService<PensionerPayItemAssignment, PensionerPayItemAssignmentDto, GetPensionerPayItemAssignmentListInput, CreateUpdatePensionerPayItemAssignmentDto>,
        IPensionerPayItemAssignmentAppService
{
    private readonly IRepository<Pensioner, Guid> _pensioners;
    private readonly IRepository<PensionerPayItem, Guid> _items;

    public PensionerPayItemAssignmentAppService(
        IRepository<PensionerPayItemAssignment, Guid> repository,
        IRepository<Pensioner, Guid> pensioners,
        IRepository<PensionerPayItem, Guid> items
    )
        : base(repository, ErpPermissions.Pensions.Default)
    {
        _pensioners = pensioners;
        _items = items;
    }

    public override async Task<PensionerPayItemAssignmentDto> CreateAsync(CreateUpdatePensionerPayItemAssignmentDto input)
    {
        await CheckCreatePolicyAsync();
        await EnsureValidAsync(input);

        var assignment = new PensionerPayItemAssignment(GuidGenerator.Create(), input.PensionerNo, input.PayItemCode, StartOf(input));
        assignment.Set(input.PensionerNo, input.PayItemCode, input.Amount, StartOf(input), input.EndDate, input.Comment);

        await Repository.InsertAsync(assignment, autoSave: true);
        return await MapToGetOutputDtoAsync(assignment);
    }

    public override async Task<PensionerPayItemAssignmentDto> UpdateAsync(Guid id, CreateUpdatePensionerPayItemAssignmentDto input)
    {
        await CheckUpdatePolicyAsync();
        await EnsureValidAsync(input);

        var assignment = await GetEntityByIdAsync(id);
        assignment.Set(input.PensionerNo, input.PayItemCode, input.Amount, input.StartDate ?? assignment.StartDate, input.EndDate, input.Comment);

        await Repository.UpdateAsync(assignment, autoSave: true);
        return await MapToGetOutputDtoAsync(assignment);
    }

    protected override async Task<PensionerPayItemAssignmentDto> MapToGetOutputDtoAsync(PensionerPayItemAssignment entity)
    {
        var dto = await base.MapToGetOutputDtoAsync(entity);
        var item = await _items.FirstOrDefaultAsync(i => i.Code == entity.PayItemCode);
        dto.PayItemDescription = item?.Description;
        dto.ItemType = item?.ItemType ?? PensionerPayItemType.Earning;
        return dto;
    }

    protected override async Task<System.Collections.Generic.List<PensionerPayItemAssignmentDto>> MapToGetListOutputDtosAsync(
        System.Collections.Generic.List<PensionerPayItemAssignment> entities
    )
    {
        var dtos = await base.MapToGetListOutputDtosAsync(entities);
        var codes = entities.Select(e => e.PayItemCode).Distinct().ToList();
        var items = (await _items.GetListAsync(i => codes.Contains(i.Code))).ToDictionary(i => i.Code);

        foreach (var dto in dtos)
        {
            if (items.TryGetValue(dto.PayItemCode, out var item))
            {
                dto.PayItemDescription = item.Description;
                dto.ItemType = item.ItemType;
            }
        }

        return dtos;
    }

    protected override async Task<IQueryable<PensionerPayItemAssignment>> CreateFilteredQueryAsync(GetPensionerPayItemAssignmentListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var pensioner = input.PensionerNo?.Trim().ToUpperInvariant();
        var filter = input.Filter?.Trim().ToLower();

        return query
            .WhereIf(!pensioner.IsNullOrEmpty(), x => x.PensionerNo == pensioner)
            .WhereIf(!filter.IsNullOrEmpty(), x => x.PensionerNo.ToLower().Contains(filter) || x.PayItemCode.ToLower().Contains(filter));
    }

    protected override IQueryable<PensionerPayItemAssignment> ApplyDefaultSorting(IQueryable<PensionerPayItemAssignment> query) =>
        query.OrderBy(x => x.PensionerNo).ThenBy(x => x.PayItemCode).ThenBy(x => x.StartDate);

    private DateTime StartOf(CreateUpdatePensionerPayItemAssignmentDto input) => input.StartDate ?? Clock.Now.Date;

    private async Task EnsureValidAsync(CreateUpdatePensionerPayItemAssignmentDto input)
    {
        var no = CodeTableEntity.NormalizeCode(input.PensionerNo);
        if (!await _pensioners.AnyAsync(p => p.No == no))
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Pensioner").WithData("code", no ?? string.Empty);
        }

        await CodeTableChecker.EnsureExistsAsync<PensionerPayItem>(input.PayItemCode);
    }
}

/// <summary>The earnings and deductions worked out on pension payroll lines. They change only by working the line out again.</summary>
public class PensionPayrollLineItemAppService
    : ErpReadOnlyAppService<PensionPayrollLineItem, PensionPayrollLineItemDto, Guid, GetPensionPayrollLineItemListInput>,
        IPensionPayrollLineItemAppService
{
    public PensionPayrollLineItemAppService(IRepository<PensionPayrollLineItem, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Pensions.Default;
        GetListPolicyName = ErpPermissions.Pensions.Default;
    }

    protected override async Task<IQueryable<PensionPayrollLineItem>> CreateFilteredQueryAsync(GetPensionPayrollLineItemListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var document = input.DocumentNo?.Trim().ToUpperInvariant();
        var pensioner = input.PensionerNo?.Trim().ToUpperInvariant();
        var filter = input.Filter?.Trim().ToLower();

        return query
            .WhereIf(!document.IsNullOrEmpty(), x => x.DocumentNo == document)
            .WhereIf(input.LineNo.HasValue, x => x.LineNo == input.LineNo.Value)
            .WhereIf(!pensioner.IsNullOrEmpty(), x => x.PensionerNo == pensioner)
            .WhereIf(!filter.IsNullOrEmpty(), x => x.PensionerNo.ToLower().Contains(filter) || x.PayItemCode.ToLower().Contains(filter));
    }

    protected override IQueryable<PensionPayrollLineItem> ApplyDefaultSorting(IQueryable<PensionPayrollLineItem> query) =>
        query.OrderBy(x => x.DocumentNo).ThenBy(x => x.LineNo).ThenBy(x => x.ItemType).ThenBy(x => x.PayItemCode);
}

/// <summary>The documents each exit reason needs.</summary>
public class ExitReasonDocumentAppService
    : ErpTableAppService<ExitReasonDocument, ExitReasonDocumentDto, GetExitReasonDocumentListInput, CreateUpdateExitReasonDocumentDto>,
        IExitReasonDocumentAppService
{
    public ExitReasonDocumentAppService(IRepository<ExitReasonDocument, Guid> repository)
        : base(repository, ErpPermissions.PensionSetup.Default) { }

    public override async Task<ExitReasonDocumentDto> CreateAsync(CreateUpdateExitReasonDocumentDto input)
    {
        await CheckCreatePolicyAsync();
        await CodeTableChecker.EnsureExistsAsync<ExitReason>(input.ExitReasonCode);

        var reason = CodeTableEntity.NormalizeCode(input.ExitReasonCode);
        var lineNo = input.LineNo;
        if (lineNo <= 0)
        {
            var query = await Repository.GetQueryableAsync();
            var last = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.ExitReasonCode == reason).OrderByDescending(x => x.LineNo));
            lineNo = (last?.LineNo ?? 0) + 10000;
        }

        var document = new ExitReasonDocument(GuidGenerator.Create(), reason, lineNo, input.DocumentName);
        document.Set(input.DocumentName, input.Mandatory);

        await Repository.InsertAsync(document, autoSave: true);
        return await MapToGetOutputDtoAsync(document);
    }

    public override async Task<ExitReasonDocumentDto> UpdateAsync(Guid id, CreateUpdateExitReasonDocumentDto input)
    {
        await CheckUpdatePolicyAsync();

        var document = await GetEntityByIdAsync(id);
        document.Set(input.DocumentName, input.Mandatory);

        await Repository.UpdateAsync(document, autoSave: true);
        return await MapToGetOutputDtoAsync(document);
    }

    protected override async Task<IQueryable<ExitReasonDocument>> CreateFilteredQueryAsync(GetExitReasonDocumentListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var reason = input.ExitReasonCode?.Trim().ToUpperInvariant();
        var filter = input.Filter?.Trim().ToLower();

        return query
            .WhereIf(!reason.IsNullOrEmpty(), x => x.ExitReasonCode == reason)
            .WhereIf(!filter.IsNullOrEmpty(), x => x.ExitReasonCode.ToLower().Contains(filter) || x.DocumentName.ToLower().Contains(filter));
    }

    protected override IQueryable<ExitReasonDocument> ApplyDefaultSorting(IQueryable<ExitReasonDocument> query) =>
        query.OrderBy(x => x.ExitReasonCode).ThenBy(x => x.LineNo);
}

/// <summary>The documents of each exit, ticked off as they come in. They change only while the exit is open.</summary>
public class MemberExitDocumentAppService
    : ErpTableAppService<MemberExitDocument, MemberExitDocumentDto, GetMemberExitDocumentListInput, CreateUpdateMemberExitDocumentDto>,
        IMemberExitDocumentAppService
{
    private readonly IRepository<MemberExit, Guid> _exits;

    public MemberExitDocumentAppService(IRepository<MemberExitDocument, Guid> repository, IRepository<MemberExit, Guid> exits)
        : base(repository, ErpPermissions.Pensions.Default)
    {
        _exits = exits;
    }

    public override async Task<MemberExitDocumentDto> CreateAsync(CreateUpdateMemberExitDocumentDto input)
    {
        await CheckCreatePolicyAsync();

        var exit = await GetOpenExitAsync(input.ExitNo);
        var lineNo = input.LineNo;
        if (lineNo <= 0)
        {
            var query = await Repository.GetQueryableAsync();
            var last = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.ExitNo == exit.No).OrderByDescending(x => x.LineNo));
            lineNo = (last?.LineNo ?? 0) + 10000;
        }

        var document = new MemberExitDocument(GuidGenerator.Create(), exit.No, lineNo, input.DocumentName, input.Mandatory);
        document.Set(input.DocumentName, input.Mandatory, input.Received, input.ReceivedDate, input.Remarks);

        await Repository.InsertAsync(document, autoSave: true);
        return await MapToGetOutputDtoAsync(document);
    }

    public override async Task<MemberExitDocumentDto> UpdateAsync(Guid id, CreateUpdateMemberExitDocumentDto input)
    {
        await CheckUpdatePolicyAsync();

        var document = await GetEntityByIdAsync(id);
        await GetOpenExitAsync(document.ExitNo);
        document.Set(input.DocumentName, input.Mandatory, input.Received, input.ReceivedDate, input.Remarks);

        await Repository.UpdateAsync(document, autoSave: true);
        return await MapToGetOutputDtoAsync(document);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var document = await GetEntityByIdAsync(id);
        await GetOpenExitAsync(document.ExitNo);
        await Repository.DeleteAsync(document, autoSave: true);
    }

    protected override async Task<IQueryable<MemberExitDocument>> CreateFilteredQueryAsync(GetMemberExitDocumentListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var exit = input.ExitNo?.Trim().ToUpperInvariant();
        var filter = input.Filter?.Trim().ToLower();

        return query
            .WhereIf(!exit.IsNullOrEmpty(), x => x.ExitNo == exit)
            .WhereIf(!filter.IsNullOrEmpty(), x => x.ExitNo.ToLower().Contains(filter) || x.DocumentName.ToLower().Contains(filter));
    }

    protected override IQueryable<MemberExitDocument> ApplyDefaultSorting(IQueryable<MemberExitDocument> query) =>
        query.OrderBy(x => x.ExitNo).ThenBy(x => x.LineNo);

    private async Task<MemberExit> GetOpenExitAsync(string exitNo)
    {
        var no = CodeTableEntity.NormalizeCode(exitNo);
        var exit = await _exits.FirstOrDefaultAsync(e => e.No == no)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Member Exit").WithData("code", no ?? string.Empty);

        exit.EnsureOpen();
        return exit;
    }
}

/// <summary>The age factors of defined benefit schemes.</summary>
public class PensionAgeFactorAppService
    : ErpTableAppService<PensionAgeFactor, PensionAgeFactorDto, GetPensionAgeFactorListInput, CreateUpdatePensionAgeFactorDto>,
        IPensionAgeFactorAppService
{
    private readonly PensionSchemeManager _schemeManager;

    public PensionAgeFactorAppService(IRepository<PensionAgeFactor, Guid> repository, PensionSchemeManager schemeManager)
        : base(repository, ErpPermissions.PensionSetup.Default)
    {
        _schemeManager = schemeManager;
    }

    public override async Task<PensionAgeFactorDto> CreateAsync(CreateUpdatePensionAgeFactorDto input)
    {
        await CheckCreatePolicyAsync();
        await EnsureValidAsync(input, null);

        var factor = new PensionAgeFactor(GuidGenerator.Create(), input.SchemeCode, input.FactorType, input.Age);
        factor.Set(input.MaleFactor, input.FemaleFactor);

        await Repository.InsertAsync(factor, autoSave: true);
        return await MapToGetOutputDtoAsync(factor);
    }

    public override async Task<PensionAgeFactorDto> UpdateAsync(Guid id, CreateUpdatePensionAgeFactorDto input)
    {
        await CheckUpdatePolicyAsync();

        var factor = await GetEntityByIdAsync(id);
        await EnsureValidAsync(input, id);
        factor.SetKey(input.SchemeCode, input.FactorType, input.Age);
        factor.Set(input.MaleFactor, input.FemaleFactor);

        await Repository.UpdateAsync(factor, autoSave: true);
        return await MapToGetOutputDtoAsync(factor);
    }

    protected override async Task<IQueryable<PensionAgeFactor>> CreateFilteredQueryAsync(GetPensionAgeFactorListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var scheme = (input.SchemeCode ?? input.Filter)?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!scheme.IsNullOrEmpty(), x => x.SchemeCode == scheme)
            .WhereIf(input.FactorType.HasValue, x => x.FactorType == input.FactorType.Value);
    }

    protected override IQueryable<PensionAgeFactor> ApplyDefaultSorting(IQueryable<PensionAgeFactor> query) =>
        query.OrderBy(x => x.SchemeCode).ThenBy(x => x.FactorType).ThenBy(x => x.Age);

    private async Task EnsureValidAsync(CreateUpdatePensionAgeFactorDto input, Guid? exceptId)
    {
        var scheme = (await _schemeManager.GetAsync(input.SchemeCode)).Code;
        if (await Repository.AnyAsync(x => x.SchemeCode == scheme && x.FactorType == input.FactorType && x.Age == input.Age && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists)
                .WithData("table", "Pension Age Factor")
                .WithData("key", $"{scheme} {input.FactorType} {input.Age}");
        }
    }
}
