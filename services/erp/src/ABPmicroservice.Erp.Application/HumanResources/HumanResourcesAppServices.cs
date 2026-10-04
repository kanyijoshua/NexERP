using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Permissions;
using ABPmicroservice.Erp.Sales;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.HumanResources;

/// <summary>Human Resources Setup: one record per company, created on first read.</summary>
[Authorize(ErpPermissions.HumanResourcesSetup.Default)]
public class HumanResourcesSetupAppService : ErpAppService, IHumanResourcesSetupAppService
{
    private readonly HumanResourcesSetupManager _setupManager;
    private readonly IRepository<HumanResourcesSetup, Guid> _repository;
    private readonly NoSeriesCodeValidator _seriesValidator;
    private readonly CodeTableChecker _codeTableChecker;

    public HumanResourcesSetupAppService(
        HumanResourcesSetupManager setupManager,
        IRepository<HumanResourcesSetup, Guid> repository,
        NoSeriesCodeValidator seriesValidator,
        CodeTableChecker codeTableChecker
    )
    {
        _setupManager = setupManager;
        _repository = repository;
        _seriesValidator = seriesValidator;
        _codeTableChecker = codeTableChecker;
    }

    public async Task<HumanResourcesSetupDto> GetAsync()
    {
        return ObjectMapper.Map<HumanResourcesSetup, HumanResourcesSetupDto>(await _setupManager.GetAsync());
    }

    [Authorize(ErpPermissions.HumanResourcesSetup.Update)]
    public async Task<HumanResourcesSetupDto> UpdateAsync(HumanResourcesSetupDto input)
    {
        await _seriesValidator.EnsureExistAsync(input.EmployeeNos);
        await _codeTableChecker.EnsureExistsAsync<HumanResourceUnitOfMeasure>(input.BaseUnitOfMeasure);

        var setup = await _setupManager.GetAsync();
        setup.Set(input.EmployeeNos, input.BaseUnitOfMeasure);

        await _repository.UpdateAsync(setup, autoSave: true);
        return ObjectMapper.Map<HumanResourcesSetup, HumanResourcesSetupDto>(setup);
    }
}

/// <summary>An HR code table under the Human Resources Setup permissions.</summary>
[Authorize(ErpPermissions.HumanResourcesSetup.Default)]
public abstract class HumanResourcesCodeTableAppService<TEntity, TDto, TInput> : CodeTableAppServiceBase<TEntity, TDto, TInput>
    where TEntity : CodeTableEntity
    where TDto : CodeTableDto
    where TInput : CreateUpdateCodeTableDto
{
    protected HumanResourcesCodeTableAppService(IRepository<TEntity, Guid> repository)
        : base(repository, ErpPermissions.HumanResourcesSetup.Default) { }
}

/// <summary>Human Resource Units of Measure.</summary>
public class HumanResourceUnitOfMeasureAppService
    : HumanResourcesCodeTableAppService<HumanResourceUnitOfMeasure, HumanResourceUnitOfMeasureDto, CreateUpdateHumanResourceUnitOfMeasureDto>,
        IHumanResourceUnitOfMeasureAppService
{
    public HumanResourceUnitOfMeasureAppService(IRepository<HumanResourceUnitOfMeasure, Guid> repository)
        : base(repository) { }

    protected override HumanResourceUnitOfMeasure NewEntity(Guid id, CreateUpdateHumanResourceUnitOfMeasureDto input) =>
        new(id, input.Code, input.Description, input.QtyPerUnitOfMeasure);

    protected override Task ApplyAsync(HumanResourceUnitOfMeasure entity, CreateUpdateHumanResourceUnitOfMeasureDto input)
    {
        entity.SetQtyPerUnitOfMeasure(input.QtyPerUnitOfMeasure);
        return Task.CompletedTask;
    }
}

/// <summary>Employee Posting Groups.</summary>
public class EmployeePostingGroupAppService
    : HumanResourcesCodeTableAppService<EmployeePostingGroup, EmployeePostingGroupDto, CreateUpdateEmployeePostingGroupDto>,
        IEmployeePostingGroupAppService
{
    public EmployeePostingGroupAppService(IRepository<EmployeePostingGroup, Guid> repository)
        : base(repository) { }

    protected override EmployeePostingGroup NewEntity(Guid id, CreateUpdateEmployeePostingGroupDto input) =>
        new(id, input.Code, input.PayablesAccountNo, input.Description);

    protected override async Task ApplyAsync(EmployeePostingGroup entity, CreateUpdateEmployeePostingGroupDto input)
    {
        await LazyServiceProvider.LazyGetRequiredService<PostingSetupManager>().EnsureGLAccountsExistAsync(input.PayablesAccountNo);
        entity.SetPayablesAccount(input.PayablesAccountNo);
    }
}

/// <summary>Causes of Absence.</summary>
public class CauseOfAbsenceAppService
    : HumanResourcesCodeTableAppService<CauseOfAbsence, CauseOfAbsenceDto, CreateUpdateCauseOfAbsenceDto>,
        ICauseOfAbsenceAppService
{
    public CauseOfAbsenceAppService(IRepository<CauseOfAbsence, Guid> repository)
        : base(repository) { }

    protected override CauseOfAbsence NewEntity(Guid id, CreateUpdateCauseOfAbsenceDto input) =>
        new(id, input.Code, input.Description);

    protected override async Task ApplyAsync(CauseOfAbsence entity, CreateUpdateCauseOfAbsenceDto input)
    {
        await CodeTableChecker.EnsureExistsAsync<HumanResourceUnitOfMeasure>(input.UnitOfMeasureCode);
        entity.SetUnitOfMeasure(input.UnitOfMeasureCode);
    }
}

/// <summary>Qualifications.</summary>
public class QualificationAppService
    : HumanResourcesCodeTableAppService<Qualification, CodeTableDto, CreateUpdateCodeTableDto>,
        IQualificationAppService
{
    public QualificationAppService(IRepository<Qualification, Guid> repository)
        : base(repository) { }

    protected override Qualification NewEntity(Guid id, CreateUpdateCodeTableDto input) => new(id, input.Code, input.Description);
}

/// <summary>Unions.</summary>
public class UnionAppService : HumanResourcesCodeTableAppService<Union, CodeTableDto, CreateUpdateCodeTableDto>, IUnionAppService
{
    public UnionAppService(IRepository<Union, Guid> repository)
        : base(repository) { }

    protected override Union NewEntity(Guid id, CreateUpdateCodeTableDto input) => new(id, input.Code, input.Description);
}

/// <summary>Employment Contracts.</summary>
public class EmploymentContractAppService
    : HumanResourcesCodeTableAppService<EmploymentContract, CodeTableDto, CreateUpdateCodeTableDto>,
        IEmploymentContractAppService
{
    public EmploymentContractAppService(IRepository<EmploymentContract, Guid> repository)
        : base(repository) { }

    protected override EmploymentContract NewEntity(Guid id, CreateUpdateCodeTableDto input) => new(id, input.Code, input.Description);
}

/// <summary>Grounds for Termination.</summary>
public class GroundsForTerminationAppService
    : HumanResourcesCodeTableAppService<GroundsForTermination, CodeTableDto, CreateUpdateCodeTableDto>,
        IGroundsForTerminationAppService
{
    public GroundsForTerminationAppService(IRepository<GroundsForTermination, Guid> repository)
        : base(repository) { }

    protected override GroundsForTermination NewEntity(Guid id, CreateUpdateCodeTableDto input) => new(id, input.Code, input.Description);
}

/// <summary>Employees.</summary>
[Authorize(ErpPermissions.Employees.Default)]
public class EmployeeAppService
    : ErpCrudAppService<Employee, EmployeeDto, Guid, GetEmployeeListInput, CreateUpdateEmployeeDto, CreateUpdateEmployeeDto>,
        IEmployeeAppService
{
    private readonly HumanResourcesSetupManager _setupManager;
    private readonly NoSeriesManager _noSeriesManager;
    private readonly CodeTableChecker _codeTableChecker;

    public EmployeeAppService(
        IRepository<Employee, Guid> repository,
        HumanResourcesSetupManager setupManager,
        NoSeriesManager noSeriesManager,
        CodeTableChecker codeTableChecker
    )
        : base(repository)
    {
        _setupManager = setupManager;
        _noSeriesManager = noSeriesManager;
        _codeTableChecker = codeTableChecker;
        GetPolicyName = ErpPermissions.Employees.Default;
        GetListPolicyName = ErpPermissions.Employees.Default;
        CreatePolicyName = ErpPermissions.Employees.Create;
        UpdatePolicyName = ErpPermissions.Employees.Update;
        DeletePolicyName = ErpPermissions.Employees.Delete;
    }

    /// <summary>An employee with ledger entries keeps their history; block them instead.</summary>
    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var entries = LazyServiceProvider.LazyGetRequiredService<IRepository<EmployeeLedgerEntry, Guid>>();
        if (await entries.AnyAsync(e => e.EmployeeId == id))
        {
            throw new BusinessException(ErpErrorCodes.HumanResources.CannotDeleteWithEntries);
        }

        await Repository.DeleteAsync(id, autoSave: true);
    }

    public override async Task<EmployeeDto> CreateAsync(CreateUpdateEmployeeDto input)
    {
        await CheckCreatePolicyAsync();
        await ValidateAsync(input);

        // Blank takes the next number of the Employee Nos. series.
        var setup = await _setupManager.GetAsync();
        var no = await _noSeriesManager.ResolveNoAsync(setup.EmployeeNos, input.No, Clock.Now);
        await EnsureNoIsUniqueAsync(no, null);

        var employee = new Employee(GuidGenerator.Create(), no, input.FirstName, input.LastName);
        Apply(employee, input);

        await Repository.InsertAsync(employee, autoSave: true);
        return await MapToGetOutputDtoAsync(employee);
    }

    public override async Task<EmployeeDto> UpdateAsync(Guid id, CreateUpdateEmployeeDto input)
    {
        await CheckUpdatePolicyAsync();

        var employee = await GetEntityByIdAsync(id);
        await ValidateAsync(input);

        if (!input.No.IsNullOrWhiteSpace() && !string.Equals(employee.No, input.No.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            await EnsureNoIsUniqueAsync(input.No.Trim(), id);
            employee.SetNo(input.No);
        }

        Apply(employee, input);

        await Repository.UpdateAsync(employee, autoSave: true);
        return await MapToGetOutputDtoAsync(employee);
    }

    protected override async Task<IQueryable<Employee>> CreateFilteredQueryAsync(GetEmployeeListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();

        return query
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.No.ToLower().Contains(filter)
                    || x.FirstName.ToLower().Contains(filter)
                    || (x.LastName != null && x.LastName.ToLower().Contains(filter))
                    || (x.JobTitle != null && x.JobTitle.ToLower().Contains(filter))
            )
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status.Value);
    }

    protected override IQueryable<Employee> ApplyDefaultSorting(IQueryable<Employee> query)
    {
        return query.OrderBy(x => x.No);
    }

    private async Task ValidateAsync(CreateUpdateEmployeeDto input)
    {
        await _codeTableChecker.EnsureExistsAsync<EmploymentContract>(input.EmplymtContractCode);
        await _codeTableChecker.EnsureExistsAsync<Union>(input.UnionCode);
        await _codeTableChecker.EnsureExistsAsync<GroundsForTermination>(input.GroundsForTermCode);
        await _codeTableChecker.EnsureExistsAsync<EmployeePostingGroup>(input.EmployeePostingGroup);
        await _codeTableChecker.EnsureExistsAsync<SalespersonPurchaser>(input.SalespersPurchCode);
    }

    private async Task EnsureNoIsUniqueAsync(string no, Guid? exceptId)
    {
        if (await Repository.AnyAsync(x => x.No == no && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.HumanResources.EmployeeAlreadyExists).WithData("no", no);
        }
    }

    private static void Apply(Employee employee, CreateUpdateEmployeeDto input)
    {
        employee.SetName(input.FirstName, input.MiddleName, input.LastName);
        employee.SetJobTitle(input.JobTitle);
        employee.SetAddress(input.Address, input.City, input.PostCode, input.CountryRegionCode);
        employee.SetContact(input.PhoneNo, input.MobilePhoneNo, input.Email, input.CompanyEmail);
        employee.SetPersonal(input.BirthDate, input.SocialSecurityNo);
        employee.SetEmployment(input.EmploymentDate, input.EmplymtContractCode, input.UnionCode);
        employee.SetStatus(input.Status, input.InactiveDate, input.TerminationDate, input.GroundsForTermCode);
        employee.SetPayment(input.EmployeePostingGroup, input.BankAccountNo, input.Iban, input.SalespersPurchCode);
        employee.SetAdditionalFields(
            input.Initials,
            input.SearchName,
            input.Address2,
            input.County,
            input.Gender,
            input.Extension,
            input.FaxNo,
            input.Pager,
            input.ManagerNo,
            input.StatisticsGroupCode,
            input.CauseOfInactivityCode,
            input.GlobalDimension1Code,
            input.GlobalDimension2Code,
            input.AltAddressCode,
            input.AltAddressStartDate,
            input.AltAddressEndDate,
            input.BankBranchNo,
            input.SwiftCode,
            input.CurrencyCode,
            input.ApplicationMethod,
            input.UnionMembershipNo,
            input.PrivacyBlocked
        );
    }
}

/// <summary>Employee Absences.</summary>
[Authorize(ErpPermissions.Employees.Default)]
public class EmployeeAbsenceAppService
    : ErpCrudAppService<EmployeeAbsence, EmployeeAbsenceDto, Guid, GetEmployeeAbsenceListInput, CreateUpdateEmployeeAbsenceDto, CreateUpdateEmployeeAbsenceDto>,
        IEmployeeAbsenceAppService
{
    private readonly IRepository<Employee, Guid> _employeeRepository;
    private readonly IRepository<CauseOfAbsence, Guid> _causeRepository;
    private readonly HumanResourcesSetupManager _setupManager;
    private readonly CodeTableChecker _codeTableChecker;

    public EmployeeAbsenceAppService(
        IRepository<EmployeeAbsence, Guid> repository,
        IRepository<Employee, Guid> employeeRepository,
        IRepository<CauseOfAbsence, Guid> causeRepository,
        HumanResourcesSetupManager setupManager,
        CodeTableChecker codeTableChecker
    )
        : base(repository)
    {
        _employeeRepository = employeeRepository;
        _causeRepository = causeRepository;
        _setupManager = setupManager;
        _codeTableChecker = codeTableChecker;
        GetPolicyName = ErpPermissions.Employees.Default;
        GetListPolicyName = ErpPermissions.Employees.Default;
        CreatePolicyName = ErpPermissions.Employees.Create;
        UpdatePolicyName = ErpPermissions.Employees.Update;
        DeletePolicyName = ErpPermissions.Employees.Delete;
    }

    public override async Task<EmployeeAbsenceDto> CreateAsync(CreateUpdateEmployeeAbsenceDto input)
    {
        await CheckCreatePolicyAsync();

        var employee = await GetEmployeeAsync(input.EmployeeNo);
        var absence = new EmployeeAbsence(GuidGenerator.Create(), employee.Id, employee.No);
        await ApplyAsync(absence, input);

        await Repository.InsertAsync(absence, autoSave: true);
        return await MapToGetOutputDtoAsync(absence);
    }

    public override async Task<EmployeeAbsenceDto> UpdateAsync(Guid id, CreateUpdateEmployeeAbsenceDto input)
    {
        await CheckUpdatePolicyAsync();

        var absence = await GetEntityByIdAsync(id);
        var employee = await GetEmployeeAsync(input.EmployeeNo);
        absence.SetEmployee(employee.Id, employee.No);
        await ApplyAsync(absence, input);

        await Repository.UpdateAsync(absence, autoSave: true);
        return await MapToGetOutputDtoAsync(absence);
    }

    protected override async Task<IQueryable<EmployeeAbsence>> CreateFilteredQueryAsync(GetEmployeeAbsenceListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();

        return query
            .WhereIf(input.EmployeeId.HasValue, x => x.EmployeeId == input.EmployeeId.Value)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.EmployeeNo.ToLower().Contains(filter)
                    || (x.CauseOfAbsenceCode != null && x.CauseOfAbsenceCode.ToLower().Contains(filter))
                    || (x.Description != null && x.Description.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<EmployeeAbsence> ApplyDefaultSorting(IQueryable<EmployeeAbsence> query)
    {
        return query.OrderByDescending(x => x.FromDate);
    }

    private async Task<Employee> GetEmployeeAsync(string employeeNo)
    {
        var no = employeeNo?.Trim();
        return await _employeeRepository.FirstOrDefaultAsync(e => e.No == no)
            ?? throw new BusinessException(ErpErrorCodes.HumanResources.EmployeeNotFound).WithData("no", no ?? "");
    }

    // The unit defaults to the cause's, then to the Human Resources Setup's base unit.
    private async Task ApplyAsync(EmployeeAbsence absence, CreateUpdateEmployeeAbsenceDto input)
    {
        await _codeTableChecker.EnsureExistsAsync<CauseOfAbsence>(input.CauseOfAbsenceCode);
        await _codeTableChecker.EnsureExistsAsync<HumanResourceUnitOfMeasure>(input.UnitOfMeasureCode);

        var unit = input.UnitOfMeasureCode;
        var description = input.Description;
        var causeCode = CodeTableEntity.NormalizeCode(input.CauseOfAbsenceCode);
        if (causeCode != null)
        {
            var cause = await _causeRepository.FirstOrDefaultAsync(c => c.Code == causeCode);
            unit ??= cause?.UnitOfMeasureCode;
            description ??= cause?.Description;
        }

        unit ??= (await _setupManager.GetAsync()).BaseUnitOfMeasure;

        absence.Set(input.FromDate, input.ToDate, causeCode, description, input.Quantity, unit);
    }
}

/// <summary>Employee Ledger Entries: expense claims and the payouts that settle them.</summary>
public class EmployeeLedgerEntryAppService
    : ErpReadOnlyAppService<EmployeeLedgerEntry, EmployeeLedgerEntryDto, Guid, GetEmployeeLedgerEntryListInput>,
        IEmployeeLedgerEntryAppService
{
    public EmployeeLedgerEntryAppService(IRepository<EmployeeLedgerEntry, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Employees.Default;
        GetListPolicyName = ErpPermissions.Employees.Default;
    }

    protected override async Task<IQueryable<EmployeeLedgerEntry>> CreateFilteredQueryAsync(GetEmployeeLedgerEntryListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var employeeNo = input.EmployeeNo?.Trim();

        return query
            .WhereIf(!employeeNo.IsNullOrEmpty(), x => x.EmployeeNo == employeeNo)
            .WhereIf(input.OnlyOpen, x => x.Open)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.DocumentNo.ToLower().Contains(filter)
                    || x.EmployeeNo.ToLower().Contains(filter)
                    || (x.Description != null && x.Description.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<EmployeeLedgerEntry> ApplyDefaultSorting(IQueryable<EmployeeLedgerEntry> query) =>
        query.OrderByDescending(x => x.EntryNo);
}
