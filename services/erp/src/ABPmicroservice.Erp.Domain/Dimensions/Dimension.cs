using ABPmicroservice.Erp.Companies;
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace ABPmicroservice.Erp.Dimensions;

/// <summary>
/// Financial Dimension. Mirrors Business Central table 348 "Dimension".
/// </summary>
public class Dimension : CompanyAggregateRoot
{
    public string Code { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public bool Blocked { get; private set; }

    protected Dimension() { }

    public Dimension(Guid id, string code, string name, string description = null)
        : base(id)
    {
        SetCode(code);
        SetName(name);
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        Blocked = false;
    }

    public void SetCode(string code)
    {
        Code = Check.NotNullOrWhiteSpace(code, nameof(code), ErpDomainConsts.MaxDimensionCodeLength);
    }

    public void SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), ErpDomainConsts.MaxNameLength);
    }

    public void Block() => Blocked = true;
    public void Unblock() => Blocked = false;
}
