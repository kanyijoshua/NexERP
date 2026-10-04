using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.HumanResources;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Payroll;

/// <summary>Payroll Setup: one record per company, created on first read.</summary>
[Authorize(ErpPermissions.PayrollSetup.Default)]
public class PayrollSetupAppService : ErpAppService, IPayrollSetupAppService
{
    private readonly PayrollSetupManager _setupManager;
    private readonly IRepository<PayrollSetup, Guid> _repository;
    private readonly NoSeriesCodeValidator _seriesValidator;

    public PayrollSetupAppService(PayrollSetupManager setupManager, IRepository<PayrollSetup, Guid> repository, NoSeriesCodeValidator seriesValidator)
    {
        _setupManager = setupManager;
        _repository = repository;
        _seriesValidator = seriesValidator;
    }

    public async Task<PayrollSetupDto> GetAsync()
    {
        return ObjectMapper.Map<PayrollSetup, PayrollSetupDto>(await _setupManager.GetAsync());
    }

    [Authorize(ErpPermissions.PayrollSetup.Update)]
    public async Task<PayrollSetupDto> UpdateAsync(PayrollSetupDto input)
    {
        await _seriesValidator.EnsureExistAsync(input.PayrollRunNos);

        var setup = await _setupManager.GetAsync();
        setup.Set(input.PayrollRunNos, input.PersonalRelief);

        await _repository.UpdateAsync(setup, autoSave: true);
        return ObjectMapper.Map<PayrollSetup, PayrollSetupDto>(setup);
    }
}

/// <summary>A payroll code table under the Payroll Setup permissions.</summary>
[Authorize(ErpPermissions.PayrollSetup.Default)]
public abstract class PayrollCodeTableAppService<TEntity, TDto, TInput> : CodeTableAppServiceBase<TEntity, TDto, TInput>
    where TEntity : CodeTableEntity
    where TDto : CodeTableDto
    where TInput : CreateUpdateCodeTableDto
{
    protected TableRelationChecker Relations => LazyServiceProvider.LazyGetRequiredService<TableRelationChecker>();

    protected IRepository<EmployeePayItem, Guid> PayItems => LazyServiceProvider.LazyGetRequiredService<IRepository<EmployeePayItem, Guid>>();

    protected IRepository<PayslipLine, Guid> PayslipLines => LazyServiceProvider.LazyGetRequiredService<IRepository<PayslipLine, Guid>>();

    protected PayrollCodeTableAppService(IRepository<TEntity, Guid> repository)
        : base(repository, ErpPermissions.PayrollSetup.Default) { }

    /// <summary>An earning or deduction on an employee's pay items or a payslip stays; block it instead.</summary>
    protected async Task EnsureNotInUseAsync(PayItemType itemType, string table, string code)
    {
        var lineType = itemType == PayItemType.Earning ? PayslipLineType.Earning : PayslipLineType.Deduction;
        if (await PayItems.AnyAsync(i => i.ItemType == itemType && i.Code == code) || await PayslipLines.AnyAsync(l => l.LineType == lineType && l.Code == code))
        {
            throw new BusinessException(ErpErrorCodes.Payroll.RecordInUse).WithData("table", table).WithData("code", code);
        }
    }
}

public class PayrollEarningAppService : PayrollCodeTableAppService<PayrollEarning, PayrollEarningDto, CreateUpdatePayrollEarningDto>, IPayrollEarningAppService
{
    public PayrollEarningAppService(IRepository<PayrollEarning, Guid> repository)
        : base(repository) { }

    protected override PayrollEarning NewEntity(Guid id, CreateUpdatePayrollEarningDto input) => new(id, input.Code, input.Description);

    protected override async Task ApplyAsync(PayrollEarning entity, CreateUpdatePayrollEarningDto input)
    {
        await Relations.EnsureGLAccountsExistAsync(input.GLAccountNo);
        entity.Set(input.CalculationMethod, input.DefaultValue, input.BasicPay, input.Taxable, input.GLAccountNo, input.Blocked);
    }

    public override async Task<PayrollEarningDto> UpdateAsync(Guid id, CreateUpdatePayrollEarningDto input)
    {
        var existing = await Repository.GetAsync(id);
        if (existing.Code != CodeTableEntity.NormalizeCode(input.Code))
        {
            await EnsureNotInUseAsync(PayItemType.Earning, "Payroll Earning", existing.Code);
        }

        return await base.UpdateAsync(id, input);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var earning = await Repository.GetAsync(id);
        await EnsureNotInUseAsync(PayItemType.Earning, "Payroll Earning", earning.Code);

        await Repository.DeleteAsync(id, autoSave: true);
    }
}

public class PayrollDeductionAppService : PayrollCodeTableAppService<PayrollDeduction, PayrollDeductionDto, CreateUpdatePayrollDeductionDto>, IPayrollDeductionAppService
{
    public PayrollDeductionAppService(IRepository<PayrollDeduction, Guid> repository)
        : base(repository) { }

    protected override PayrollDeduction NewEntity(Guid id, CreateUpdatePayrollDeductionDto input) => new(id, input.Code, input.Description);

    protected override async Task ApplyAsync(PayrollDeduction entity, CreateUpdatePayrollDeductionDto input)
    {
        await Relations.EnsureGLAccountsExistAsync(input.GLAccountNo, input.EmployerExpenseAccountNo);
        entity.Set(
            input.CalculationMethod,
            input.DefaultValue,
            input.MaximumAmount,
            input.TaxDeductible,
            input.EmployerContributionPct,
            input.GLAccountNo,
            input.EmployerExpenseAccountNo,
            input.Blocked
        );
        entity.SetStatutory(input.Statutory);
    }

    public override async Task<PayrollDeductionDto> UpdateAsync(Guid id, CreateUpdatePayrollDeductionDto input)
    {
        var existing = await Repository.GetAsync(id);
        if (existing.Code != CodeTableEntity.NormalizeCode(input.Code))
        {
            await EnsureNotInUseAsync(PayItemType.Deduction, "Payroll Deduction", existing.Code);
        }

        return await base.UpdateAsync(id, input);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var deduction = await Repository.GetAsync(id);
        await EnsureNotInUseAsync(PayItemType.Deduction, "Payroll Deduction", deduction.Code);

        await Repository.DeleteAsync(id, autoSave: true);
    }
}

/// <summary>The monthly income tax bands.</summary>
public class PayrollTaxBandAppService
    : ErpTableAppService<PayrollTaxBand, PayrollTaxBandDto, GetPayrollTaxBandListInput, CreateUpdatePayrollTaxBandDto>,
        IPayrollTaxBandAppService
{
    public PayrollTaxBandAppService(IRepository<PayrollTaxBand, Guid> repository)
        : base(repository, ErpPermissions.PayrollSetup.Default) { }

    public override async Task<PayrollTaxBandDto> CreateAsync(CreateUpdatePayrollTaxBandDto input)
    {
        await CheckCreatePolicyAsync();

        var band = new PayrollTaxBand(GuidGenerator.Create(), input.LowerLimit);
        band.Set(input.UpperLimit, input.RatePct);
        await EnsureNoOverlapAsync(band);

        await Repository.InsertAsync(band, autoSave: true);
        return await MapToGetOutputDtoAsync(band);
    }

    public override async Task<PayrollTaxBandDto> UpdateAsync(Guid id, CreateUpdatePayrollTaxBandDto input)
    {
        await CheckUpdatePolicyAsync();

        var band = await GetEntityByIdAsync(id);
        band.SetLowerLimit(input.LowerLimit);
        band.Set(input.UpperLimit, input.RatePct);
        await EnsureNoOverlapAsync(band);

        await Repository.UpdateAsync(band, autoSave: true);
        return await MapToGetOutputDtoAsync(band);
    }

    protected override IQueryable<PayrollTaxBand> ApplyDefaultSorting(IQueryable<PayrollTaxBand> query) => query.OrderBy(x => x.LowerLimit);

    /// <summary>A pound of pay must be taxed by one band only.</summary>
    private async Task EnsureNoOverlapAsync(PayrollTaxBand band)
    {
        var others = await Repository.GetListAsync(b => b.Id != band.Id);
        var upper = band.UpperLimit == 0m ? decimal.MaxValue : band.UpperLimit;

        if (others.Any(o => o.LowerLimit < upper && (o.UpperLimit == 0m ? decimal.MaxValue : o.UpperLimit) > band.LowerLimit))
        {
            throw new BusinessException(ErpErrorCodes.Payroll.InvalidTaxBand).WithData("lower", band.LowerLimit);
        }
    }
}

/// <summary>Employees' recurring earnings and deductions.</summary>
public class EmployeePayItemAppService
    : ErpTableAppService<EmployeePayItem, EmployeePayItemDto, GetEmployeePayItemListInput, CreateUpdateEmployeePayItemDto>,
        IEmployeePayItemAppService
{
    public EmployeePayItemAppService(IRepository<EmployeePayItem, Guid> repository)
        : base(repository, ErpPermissions.Payroll.Default) { }

    public override async Task<EmployeePayItemDto> CreateAsync(CreateUpdateEmployeePayItemDto input)
    {
        await CheckCreatePolicyAsync();

        var item = new EmployeePayItem(GuidGenerator.Create(), input.EmployeeNo, input.ItemType, input.Code);
        await ApplyAsync(item, input);

        await Repository.InsertAsync(item, autoSave: true);
        return await MapToGetOutputDtoAsync(item);
    }

    public override async Task<EmployeePayItemDto> UpdateAsync(Guid id, CreateUpdateEmployeePayItemDto input)
    {
        await CheckUpdatePolicyAsync();

        var item = await GetEntityByIdAsync(id);
        item.SetKey(input.EmployeeNo, input.ItemType, input.Code);
        await ApplyAsync(item, input);

        await Repository.UpdateAsync(item, autoSave: true);
        return await MapToGetOutputDtoAsync(item);
    }

    protected override async Task<IQueryable<EmployeePayItem>> CreateFilteredQueryAsync(GetEmployeePayItemListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var employee = input.EmployeeNo?.Trim();

        return query
            .WhereIf(!employee.IsNullOrEmpty(), x => x.EmployeeNo == employee)
            .WhereIf(!filter.IsNullOrEmpty(), x => x.EmployeeNo.ToLower().Contains(filter) || x.Code.ToLower().Contains(filter));
    }

    protected override IQueryable<EmployeePayItem> ApplyDefaultSorting(IQueryable<EmployeePayItem> query) =>
        query.OrderBy(x => x.EmployeeNo).ThenBy(x => x.ItemType).ThenBy(x => x.Code);

    private async Task ApplyAsync(EmployeePayItem item, CreateUpdateEmployeePayItemDto input)
    {
        await Relations.EnsureNoExistsAsync<Employee>(item.EmployeeNo);
        if (item.ItemType == PayItemType.Earning)
        {
            await CodeTableChecker.EnsureExistsAsync<PayrollEarning>(item.Code);
        }
        else
        {
            await CodeTableChecker.EnsureExistsAsync<PayrollDeduction>(item.Code);
        }

        item.Set(input.Amount, input.StartDate, input.EndDate);

        // Two items of the same code for the same months would pay or deduct it twice.
        var (employee, type, code, id) = (item.EmployeeNo, item.ItemType, item.Code, item.Id);
        var others = await Repository.GetListAsync(x => x.Id != id && x.EmployeeNo == employee && x.ItemType == type && x.Code == code);
        if (others.Any(o => (o.StartDate ?? DateTime.MinValue) <= (item.EndDate ?? DateTime.MaxValue) && (o.EndDate ?? DateTime.MaxValue) >= (item.StartDate ?? DateTime.MinValue)))
        {
            throw new BusinessException(ErpErrorCodes.Payroll.PayItemDuplicated).WithData("employeeNo", employee).WithData("code", code);
        }
    }
}

/// <summary>Payroll runs: calculated into payslips, posted, and paid through a payment voucher.</summary>
public class PayrollRunAppService
    : ErpTableAppService<PayrollRun, PayrollRunDto, GetPayrollRunListInput, CreateUpdatePayrollRunDto>,
        IPayrollRunAppService
{
    private readonly PayrollEngine _engine;
    private readonly PayrollSetupManager _setupManager;
    private readonly NoSeriesManager _noSeriesManager;
    private readonly IRepository<Payslip, Guid> _payslips;
    private readonly IRepository<PayslipLine, Guid> _lines;

    public PayrollRunAppService(
        IRepository<PayrollRun, Guid> repository,
        PayrollEngine engine,
        PayrollSetupManager setupManager,
        NoSeriesManager noSeriesManager,
        IRepository<Payslip, Guid> payslips,
        IRepository<PayslipLine, Guid> lines
    )
        : base(repository, ErpPermissions.Payroll.Default)
    {
        _engine = engine;
        _setupManager = setupManager;
        _noSeriesManager = noSeriesManager;
        _payslips = payslips;
        _lines = lines;
    }

    public override async Task<PayrollRunDto> CreateAsync(CreateUpdatePayrollRunDto input)
    {
        await CheckCreatePolicyAsync();

        var postingDate = input.PostingDate == default ? Clock.Now.Date : input.PostingDate;
        var no = (await _noSeriesManager.ResolveNoAsync((await _setupManager.GetAsync()).PayrollRunNos, input.No, postingDate)).ToUpperInvariant();
        if (await Repository.AnyAsync(x => x.No == no))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Payroll Run").WithData("key", no);
        }

        var run = new PayrollRun(GuidGenerator.Create(), no, input.PayPeriod == default ? postingDate : input.PayPeriod, postingDate);
        run.Set(run.PayPeriod, postingDate, input.Description);

        await Repository.InsertAsync(run, autoSave: true);
        return await MapToGetOutputDtoAsync(run);
    }

    public override async Task<PayrollRunDto> UpdateAsync(Guid id, CreateUpdatePayrollRunDto input)
    {
        await CheckUpdatePolicyAsync();

        var run = await GetEntityByIdAsync(id);
        run.Set(input.PayPeriod == default ? run.PayPeriod : input.PayPeriod, input.PostingDate == default ? run.PostingDate : input.PostingDate, input.Description);

        await Repository.UpdateAsync(run, autoSave: true);
        return await MapToGetOutputDtoAsync(run);
    }

    /// <summary>A posted run is the source of ledger entries and stays; an unposted one goes with its payslips.</summary>
    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var run = await GetEntityByIdAsync(id);
        run.EnsureNotPosted();

        await _lines.DeleteAsync(l => l.PayrollRunNo == run.No, autoSave: true);
        await _payslips.DeleteAsync(p => p.PayrollRunNo == run.No, autoSave: true);
        await Repository.DeleteAsync(id, autoSave: true);
    }

    [Authorize(ErpPermissions.Payroll.Update)]
    public async Task<PayrollRunDto> CalculateAsync(Guid id)
    {
        var run = await GetEntityByIdAsync(id);
        await _engine.CalculateAsync(run);
        return await MapToGetOutputDtoAsync(run);
    }

    [Authorize(ErpPermissions.Payroll.Post)]
    public async Task<PayrollRunDto> RunPostingAsync(Guid id)
    {
        var run = await GetEntityByIdAsync(id);
        await _engine.PostAsync(run);
        return await MapToGetOutputDtoAsync(run);
    }

    [Authorize(ErpPermissions.Payroll.Post)]
    public async Task<PayrollRunDto> RaisePaymentVoucherAsync(Guid id)
    {
        var run = await GetEntityByIdAsync(id);
        await _engine.RaisePaymentVoucherAsync(run, run.PostingDate);
        return await MapToGetOutputDtoAsync(run);
    }

    protected override async Task<IQueryable<PayrollRun>> CreateFilteredQueryAsync(GetPayrollRunListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();

        return query
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status.Value)
            .WhereIf(!filter.IsNullOrEmpty(), x => x.No.ToLower().Contains(filter) || (x.Description != null && x.Description.ToLower().Contains(filter)));
    }

    protected override IQueryable<PayrollRun> ApplyDefaultSorting(IQueryable<PayrollRun> query) => query.OrderByDescending(x => x.PayPeriod).ThenByDescending(x => x.No);
}

/// <summary>Payslips: made by calculating a payroll run, so they can only be read.</summary>
public class PayslipAppService : ErpReadOnlyAppService<Payslip, PayslipDto, Guid, GetPayslipListInput>, IPayslipAppService
{
    public PayslipAppService(IRepository<Payslip, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Payroll.Default;
        GetListPolicyName = ErpPermissions.Payroll.Default;
    }

    protected override async Task<IQueryable<Payslip>> CreateFilteredQueryAsync(GetPayslipListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var run = input.PayrollRunNo?.Trim().ToUpperInvariant();
        var employee = input.EmployeeNo?.Trim();

        return query
            .WhereIf(!run.IsNullOrEmpty(), x => x.PayrollRunNo == run)
            .WhereIf(!employee.IsNullOrEmpty(), x => x.EmployeeNo == employee)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.PayrollRunNo.ToLower().Contains(filter) || x.EmployeeNo.ToLower().Contains(filter) || (x.EmployeeName != null && x.EmployeeName.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<Payslip> ApplyDefaultSorting(IQueryable<Payslip> query) => query.OrderByDescending(x => x.PayPeriod).ThenBy(x => x.EmployeeNo);
}

/// <summary>The earnings, deductions and employer contributions of payslips.</summary>
public class PayslipLineAppService : ErpReadOnlyAppService<PayslipLine, PayslipLineDto, Guid, GetPayslipListInput>, IPayslipLineAppService
{
    public PayslipLineAppService(IRepository<PayslipLine, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Payroll.Default;
        GetListPolicyName = ErpPermissions.Payroll.Default;
    }

    protected override async Task<IQueryable<PayslipLine>> CreateFilteredQueryAsync(GetPayslipListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var run = input.PayrollRunNo?.Trim().ToUpperInvariant();
        var employee = input.EmployeeNo?.Trim();

        return query
            .WhereIf(!run.IsNullOrEmpty(), x => x.PayrollRunNo == run)
            .WhereIf(!employee.IsNullOrEmpty(), x => x.EmployeeNo == employee)
            .WhereIf(!filter.IsNullOrEmpty(), x => x.PayrollRunNo.ToLower().Contains(filter) || x.EmployeeNo.ToLower().Contains(filter) || x.Code.ToLower().Contains(filter));
    }

    protected override IQueryable<PayslipLine> ApplyDefaultSorting(IQueryable<PayslipLine> query) =>
        query.OrderBy(x => x.PayrollRunNo).ThenBy(x => x.EmployeeNo).ThenBy(x => x.LineType).ThenBy(x => x.Code);
}
