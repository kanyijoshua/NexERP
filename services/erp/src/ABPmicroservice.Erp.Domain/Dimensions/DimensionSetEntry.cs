using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace ABPmicroservice.Erp.Dimensions;

/// <summary>
/// Dimension Set Entry. Mirrors Business Central table 480 "Dimension Set Entry".
/// Immutable combination of dimension codes & value codes grouped by a unique DimensionSetId.
/// </summary>
public class DimensionSetEntry : Entity<Guid>
{
    public Guid DimensionSetId { get; private set; }
    public string DimensionCode { get; private set; }
    public string DimensionValueCode { get; private set; }

    protected DimensionSetEntry() { }

    public DimensionSetEntry(Guid id, Guid dimensionSetId, string dimensionCode, string dimensionValueCode)
        : base(id)
    {
        DimensionSetId = dimensionSetId;
        DimensionCode = Check.NotNullOrWhiteSpace(dimensionCode, nameof(dimensionCode), ErpDomainConsts.MaxDimensionCodeLength);
        DimensionValueCode = Check.NotNullOrWhiteSpace(dimensionValueCode, nameof(dimensionValueCode), ErpDomainConsts.MaxDimensionValueCodeLength);
    }
}
