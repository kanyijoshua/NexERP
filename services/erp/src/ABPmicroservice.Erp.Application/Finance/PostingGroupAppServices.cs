using System;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Permissions;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sales;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Finance;

/// <summary>A posting group table under the Posting Setup permissions.</summary>
[Authorize(ErpPermissions.PostingSetup.Default)]
public abstract class PostingGroupAppServiceBase<TEntity, TDto, TInput> : CodeTableAppServiceBase<TEntity, TDto, TInput>
    where TEntity : PostingGroupBase
    where TDto : PostingGroupDto
    where TInput : CreateUpdatePostingGroupDto
{
    protected PostingSetupManager PostingSetupManager => LazyServiceProvider.LazyGetRequiredService<PostingSetupManager>();

    protected PostingGroupAppServiceBase(IRepository<TEntity, Guid> repository)
        : base(repository, ErpPermissions.PostingSetup.Default) { }
}

/// <summary>Gen. Business Posting Groups (BC page 312).</summary>
public class GenBusinessPostingGroupAppService
    : PostingGroupAppServiceBase<GenBusinessPostingGroup, PostingGroupDto, CreateUpdatePostingGroupDto>,
        IGenBusinessPostingGroupAppService
{
    public GenBusinessPostingGroupAppService(IRepository<GenBusinessPostingGroup, Guid> repository)
        : base(repository) { }

    protected override GenBusinessPostingGroup NewEntity(Guid id, CreateUpdatePostingGroupDto input) =>
        new(id, input.Code, input.Description);
}

/// <summary>Gen. Product Posting Groups (BC page 313).</summary>
public class GenProductPostingGroupAppService
    : PostingGroupAppServiceBase<GenProductPostingGroup, PostingGroupDto, CreateUpdatePostingGroupDto>,
        IGenProductPostingGroupAppService
{
    public GenProductPostingGroupAppService(IRepository<GenProductPostingGroup, Guid> repository)
        : base(repository) { }

    protected override GenProductPostingGroup NewEntity(Guid id, CreateUpdatePostingGroupDto input) =>
        new(id, input.Code, input.Description);
}

/// <summary>VAT Business Posting Groups (BC page 470).</summary>
public class VatBusinessPostingGroupAppService
    : PostingGroupAppServiceBase<VatBusinessPostingGroup, PostingGroupDto, CreateUpdatePostingGroupDto>,
        IVatBusinessPostingGroupAppService
{
    public VatBusinessPostingGroupAppService(IRepository<VatBusinessPostingGroup, Guid> repository)
        : base(repository) { }

    protected override VatBusinessPostingGroup NewEntity(Guid id, CreateUpdatePostingGroupDto input) =>
        new(id, input.Code, input.Description);
}

/// <summary>VAT Product Posting Groups (BC page 471).</summary>
public class VatProductPostingGroupAppService
    : PostingGroupAppServiceBase<VatProductPostingGroup, PostingGroupDto, CreateUpdatePostingGroupDto>,
        IVatProductPostingGroupAppService
{
    public VatProductPostingGroupAppService(IRepository<VatProductPostingGroup, Guid> repository)
        : base(repository) { }

    protected override VatProductPostingGroup NewEntity(Guid id, CreateUpdatePostingGroupDto input) =>
        new(id, input.Code, input.Description);
}

/// <summary>Inventory Posting Groups (BC page 112).</summary>
public class InventoryPostingGroupAppService
    : PostingGroupAppServiceBase<InventoryPostingGroup, PostingGroupDto, CreateUpdatePostingGroupDto>,
        IInventoryPostingGroupAppService
{
    public InventoryPostingGroupAppService(IRepository<InventoryPostingGroup, Guid> repository)
        : base(repository) { }

    protected override InventoryPostingGroup NewEntity(Guid id, CreateUpdatePostingGroupDto input) =>
        new(id, input.Code, input.Description);
}

/// <summary>Customer Posting Groups (BC page 110).</summary>
public class CustomerPostingGroupAppService
    : PostingGroupAppServiceBase<CustomerPostingGroup, CustomerPostingGroupDto, CreateUpdateCustomerPostingGroupDto>,
        ICustomerPostingGroupAppService
{
    public CustomerPostingGroupAppService(IRepository<CustomerPostingGroup, Guid> repository)
        : base(repository) { }

    protected override CustomerPostingGroup NewEntity(Guid id, CreateUpdateCustomerPostingGroupDto input) =>
        new(id, input.Code, input.ReceivablesAccountNo, input.Description);

    protected override async Task ApplyAsync(CustomerPostingGroup entity, CreateUpdateCustomerPostingGroupDto input)
    {
        await PostingSetupManager.EnsureGLAccountsExistAsync(input.ReceivablesAccountNo);
        entity.SetReceivablesAccount(input.ReceivablesAccountNo);
    }
}

/// <summary>Vendor Posting Groups (BC page 111).</summary>
public class VendorPostingGroupAppService
    : PostingGroupAppServiceBase<VendorPostingGroup, VendorPostingGroupDto, CreateUpdateVendorPostingGroupDto>,
        IVendorPostingGroupAppService
{
    public VendorPostingGroupAppService(IRepository<VendorPostingGroup, Guid> repository)
        : base(repository) { }

    protected override VendorPostingGroup NewEntity(Guid id, CreateUpdateVendorPostingGroupDto input) =>
        new(id, input.Code, input.PayablesAccountNo, input.Description);

    protected override async Task ApplyAsync(VendorPostingGroup entity, CreateUpdateVendorPostingGroupDto input)
    {
        await PostingSetupManager.EnsureGLAccountsExistAsync(input.PayablesAccountNo);
        entity.SetPayablesAccount(input.PayablesAccountNo);
    }
}
