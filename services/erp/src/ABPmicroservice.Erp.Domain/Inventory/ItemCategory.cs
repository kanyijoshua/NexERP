using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace ABPmicroservice.Erp.Inventory;

/// <summary>
/// Item Category. Mirrors Business Central table 5722 "Item Category".
/// </summary>
public class ItemCategory : FullAuditedAggregateRoot<Guid>
{
    public string Code { get; private set; }

    public string Description { get; private set; }

    public Guid? ParentCategoryId { get; private set; }

    public string ParentCategoryCode { get; private set; }

    protected ItemCategory() { }

    public ItemCategory(
        Guid id,
        string code,
        string description,
        Guid? parentCategoryId = null,
        string parentCategoryCode = null
    )
        : base(id)
    {
        SetCode(code);
        SetDescription(description);
        ParentCategoryId = parentCategoryId;
        ParentCategoryCode = parentCategoryCode;
    }

    public void SetCode(string code) =>
        Code = Check.NotNullOrWhiteSpace(code, nameof(code), ErpDomainConsts.MaxCodeLength);

    public void SetDescription(string description) =>
        Description = Check.Length(
            description,
            nameof(description),
            ErpDomainConsts.MaxDescriptionLength
        );

    public void SetParent(Guid? parentCategoryId, string parentCategoryCode)
    {
        ParentCategoryId = parentCategoryId;
        ParentCategoryCode = parentCategoryCode;
    }
}
