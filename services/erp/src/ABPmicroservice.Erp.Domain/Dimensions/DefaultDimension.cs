using ABPmicroservice.Erp.Companies;
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace ABPmicroservice.Erp.Dimensions;

/// <summary>
/// Default Dimension. Mirrors Business Central table 352 "Default Dimension".
/// Links entities (Customer, Vendor, Item, GLAccount) to default dimensions.
/// </summary>
public class DefaultDimension : CompanyBasicEntity
{
    public string TableName { get; private set; }
    public Guid EntityId { get; private set; }
    public string EntityNo { get; private set; }
    public string DimensionCode { get; private set; }
    public string DimensionValueCode { get; private set; }
    public string ValuePosting { get; private set; } // e.g. "Code Mandatory", "Same Code", "No Code"

    protected DefaultDimension() { }

    public DefaultDimension(
        Guid id,
        string tableName,
        Guid entityId,
        string entityNo,
        string dimensionCode,
        string dimensionValueCode,
        string valuePosting = "Code Mandatory"
    )
        : base(id)
    {
        TableName = Check.NotNullOrWhiteSpace(tableName, nameof(tableName));
        EntityId = entityId;
        EntityNo = Check.NotNullOrWhiteSpace(entityNo, nameof(entityNo), ErpDomainConsts.MaxNoLength);
        DimensionCode = Check.NotNullOrWhiteSpace(dimensionCode, nameof(dimensionCode), ErpDomainConsts.MaxDimensionCodeLength);
        DimensionValueCode = Check.NotNullOrWhiteSpace(dimensionValueCode, nameof(dimensionValueCode), ErpDomainConsts.MaxDimensionValueCodeLength);
        ValuePosting = valuePosting;
    }
}
