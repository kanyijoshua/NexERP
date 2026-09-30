using System;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.HumanResources;

/// <summary>
/// Human Resources Setup. Mirrors Business Central table 5218: one row per company with the
/// employee number series and the unit absences are counted in.
/// </summary>
public class HumanResourcesSetup : CompanyEntity
{
    public string EmployeeNos { get; private set; }

    /// <summary>A Human Resource Unit of Measure code, e.g. "DAY".</summary>
    public string BaseUnitOfMeasure { get; private set; }

    protected HumanResourcesSetup() { }

    public HumanResourcesSetup(Guid id)
        : base(id) { }

    public void Set(string employeeNos, string baseUnitOfMeasure)
    {
        EmployeeNos = employeeNos.IsNullOrWhiteSpace()
            ? null
            : Check.Length(employeeNos.Trim(), nameof(employeeNos), ErpDomainConsts.MaxNoSeriesCodeLength);
        BaseUnitOfMeasure = CodeTableEntity.NormalizeCode(Check.Length(baseUnitOfMeasure, nameof(baseUnitOfMeasure), ErpDomainConsts.MaxUnitOfMeasureCodeLength));
    }
}

public class HumanResourcesSetupManager : DomainService
{
    private readonly IRepository<HumanResourcesSetup, Guid> _repository;

    public HumanResourcesSetupManager(IRepository<HumanResourcesSetup, Guid> repository)
    {
        _repository = repository;
    }

    /// <summary>The current company's setup, created blank on first use.</summary>
    public async Task<HumanResourcesSetup> GetAsync()
    {
        return await _repository.FirstOrDefaultAsync()
            ?? await _repository.InsertAsync(new HumanResourcesSetup(GuidGenerator.Create()), autoSave: true);
    }
}

/// <summary>Human Resource Unit of Measure. Mirrors BC table 5220: DAY, HOUR, and how many base units each is.</summary>
public class HumanResourceUnitOfMeasure : CodeTableEntity
{
    protected override int MaxCodeLength => ErpDomainConsts.MaxUnitOfMeasureCodeLength;

    public decimal QtyPerUnitOfMeasure { get; private set; } = 1m;

    protected HumanResourceUnitOfMeasure() { }

    public HumanResourceUnitOfMeasure(Guid id, string code, string description, decimal qtyPerUnitOfMeasure = 1m)
        : base(id, code, description)
    {
        SetQtyPerUnitOfMeasure(qtyPerUnitOfMeasure);
    }

    public void SetQtyPerUnitOfMeasure(decimal qty)
    {
        QtyPerUnitOfMeasure = qty > 0 ? qty : throw new BusinessException(ErpErrorCodes.PostingSetup.InvalidPercent).WithData("value", qty);
    }
}

/// <summary>
/// Employee Posting Group. Mirrors BC table 5221: the payables account employee expenses owed to
/// employees are carried on.
/// </summary>
public class EmployeePostingGroup : PostingGroupBase
{
    public string PayablesAccountNo { get; private set; }

    protected EmployeePostingGroup() { }

    public EmployeePostingGroup(Guid id, string code, string payablesAccountNo, string description = null)
        : base(id, code, description)
    {
        SetPayablesAccount(payablesAccountNo);
    }

    public void SetPayablesAccount(string payablesAccountNo)
    {
        PayablesAccountNo = Check.NotNullOrWhiteSpace(payablesAccountNo, nameof(payablesAccountNo), ErpDomainConsts.MaxNoLength).Trim();
    }
}

/// <summary>Cause of Absence. Mirrors BC table 5206: sickness, holiday, training, and the unit it is counted in.</summary>
public class CauseOfAbsence : CodeTableEntity
{
    public string UnitOfMeasureCode { get; private set; }

    protected CauseOfAbsence() { }

    public CauseOfAbsence(Guid id, string code, string description, string unitOfMeasureCode = null)
        : base(id, code, description)
    {
        SetUnitOfMeasure(unitOfMeasureCode);
    }

    public void SetUnitOfMeasure(string unitOfMeasureCode) =>
        UnitOfMeasureCode = NormalizeCode(Check.Length(unitOfMeasureCode, nameof(unitOfMeasureCode), ErpDomainConsts.MaxUnitOfMeasureCodeLength));
}

/// <summary>Qualification. Mirrors BC table 5202: a degree, certificate or skill an employee can hold.</summary>
public class Qualification : CodeTableEntity
{
    protected Qualification() { }

    public Qualification(Guid id, string code, string description)
        : base(id, code, description) { }
}

/// <summary>Union. Mirrors BC table 5209: a trade union employees may belong to. The description is its name.</summary>
public class Union : CodeTableEntity
{
    protected Union() { }

    public Union(Guid id, string code, string name)
        : base(id, code, name) { }
}

/// <summary>Employment Contract. Mirrors BC table 5211: permanent, fixed-term, casual.</summary>
public class EmploymentContract : CodeTableEntity
{
    protected EmploymentContract() { }

    public EmploymentContract(Guid id, string code, string description)
        : base(id, code, description) { }
}

/// <summary>Grounds for Termination. Mirrors BC table 5217: why employment ended.</summary>
public class GroundsForTermination : CodeTableEntity
{
    protected GroundsForTermination() { }

    public GroundsForTermination(Guid id, string code, string description)
        : base(id, code, description) { }
}
