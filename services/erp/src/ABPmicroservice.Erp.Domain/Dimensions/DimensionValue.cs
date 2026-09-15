using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace ABPmicroservice.Erp.Dimensions;

/// <summary>
/// Dimension Value. Mirrors Business Central table 349 "Dimension Value".
/// </summary>
public class DimensionValue : FullAuditedEntity<Guid>
{
    public Guid DimensionId { get; private set; }
    public string DimensionCode { get; private set; }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public bool Blocked { get; private set; }

    protected DimensionValue() { }

    public DimensionValue(Guid id, Guid dimensionId, string dimensionCode, string code, string name)
        : base(id)
    {
        DimensionId = dimensionId;
        DimensionCode = Check.NotNullOrWhiteSpace(dimensionCode, nameof(dimensionCode), ErpDomainConsts.MaxDimensionCodeLength);
        SetCode(code);
        SetName(name);
        Blocked = false;
    }

    public void SetCode(string code)
    {
        Code = Check.NotNullOrWhiteSpace(code, nameof(code), ErpDomainConsts.MaxDimensionValueCodeLength);
    }

    public void SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), ErpDomainConsts.MaxNameLength);
    }

    public void Block() => Blocked = true;
    public void Unblock() => Blocked = false;
}
