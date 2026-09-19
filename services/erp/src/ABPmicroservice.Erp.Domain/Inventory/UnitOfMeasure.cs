using ABPmicroservice.Erp.Companies;
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace ABPmicroservice.Erp.Inventory;

/// <summary>
/// Unit of Measure. Mirrors Business Central table 204 "Unit of Measure".
/// </summary>
public class UnitOfMeasure : CompanyAggregateRoot
{
    public string Code { get; private set; }

    public string Description { get; private set; }

    protected UnitOfMeasure() { }

    public UnitOfMeasure(Guid id, string code, string description)
        : base(id)
    {
        SetCode(code);
        SetDescription(description);
    }

    public void SetCode(string code) =>
        Code = Check.NotNullOrWhiteSpace(
            code,
            nameof(code),
            ErpDomainConsts.MaxUnitOfMeasureCodeLength
        );

    public void SetDescription(string description) =>
        Description = Check.Length(
            description,
            nameof(description),
            ErpDomainConsts.MaxDescriptionLength
        );
}
