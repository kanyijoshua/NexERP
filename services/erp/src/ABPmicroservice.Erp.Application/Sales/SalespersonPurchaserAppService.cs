using System;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Sales;

/// <summary>Salespeople/Purchasers (BC page 14).</summary>
[Authorize(ErpPermissions.SalespeoplePurchasers.Default)]
public class SalespersonPurchaserAppService
    : CodeTableAppServiceBase<SalespersonPurchaser, SalespersonPurchaserDto, CreateUpdateSalespersonPurchaserDto>,
        ISalespersonPurchaserAppService
{
    public SalespersonPurchaserAppService(IRepository<SalespersonPurchaser, Guid> repository)
        : base(repository, ErpPermissions.SalespeoplePurchasers.Default) { }

    protected override SalespersonPurchaser NewEntity(Guid id, CreateUpdateSalespersonPurchaserDto input) =>
        new(id, input.Code, input.Description);

    protected override Task ApplyAsync(SalespersonPurchaser entity, CreateUpdateSalespersonPurchaserDto input)
    {
        entity.SetContact(input.Email, input.PhoneNo, input.JobTitle);
        entity.SetCommission(input.CommissionPercent);
        entity.SetBlocked(input.Blocked);
        return Task.CompletedTask;
    }
}
