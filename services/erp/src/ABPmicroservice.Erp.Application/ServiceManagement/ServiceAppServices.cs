using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Permissions;
using ABPmicroservice.Erp.Projects;
using ABPmicroservice.Erp.Sales;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.ServiceManagement;

[Authorize(ErpPermissions.ServiceSetup.Default)]
public class ServiceMgtSetupAppService : ErpAppService, IServiceMgtSetupAppService
{
    private readonly IRepository<ServiceMgtSetup, Guid> _repository;

    public ServiceMgtSetupAppService(IRepository<ServiceMgtSetup, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<ServiceMgtSetupDto> GetAsync() => Map(await GetOrCreateAsync());

    [Authorize(ErpPermissions.ServiceSetup.Update)]
    public async Task<ServiceMgtSetupDto> UpdateAsync(ServiceMgtSetupDto input)
    {
        await LazyServiceProvider.LazyGetRequiredService<NoSeriesCodeValidator>().EnsureExistAsync(input.ServiceOrderNos, input.ServiceItemNos, input.ServiceContractNos);
        await LazyServiceProvider.LazyGetRequiredService<PostingSetupManager>().EnsureGLAccountsExistAsync(input.ServiceRevenueAccountNo, input.ContractRevenueAccountNo);
        var setup = await GetOrCreateAsync();
        setup.Set(input.ServiceOrderNos, input.ServiceItemNos, input.ServiceContractNos, input.ServiceRevenueAccountNo, input.ContractRevenueAccountNo, input.DefaultWarrantyMonths);
        await _repository.UpdateAsync(setup, autoSave: true);
        return Map(setup);
    }

    private async Task<ServiceMgtSetup> GetOrCreateAsync() =>
        await _repository.FirstOrDefaultAsync() ?? await _repository.InsertAsync(new ServiceMgtSetup(GuidGenerator.Create()), autoSave: true);

    private static ServiceMgtSetupDto Map(ServiceMgtSetup s) => new()
    {
        ServiceOrderNos = s.ServiceOrderNos,
        ServiceItemNos = s.ServiceItemNos,
        ServiceContractNos = s.ServiceContractNos,
        ServiceRevenueAccountNo = s.ServiceRevenueAccountNo,
        ContractRevenueAccountNo = s.ContractRevenueAccountNo,
        DefaultWarrantyMonths = s.DefaultWarrantyMonths,
    };
}

public class ServiceOrderTypeAppService : CodeTableAppServiceBase<ServiceOrderType, ServiceOrderTypeDto, CreateUpdateServiceOrderTypeDto>, IServiceOrderTypeAppService
{
    public ServiceOrderTypeAppService(IRepository<ServiceOrderType, Guid> repository)
        : base(repository, ErpPermissions.ServiceSetup.Default) { }

    protected override ServiceOrderType NewEntity(Guid id, CreateUpdateServiceOrderTypeDto input) => new(id, input.Code, input.Description);
}

public class FaultCodeAppService : CodeTableAppServiceBase<FaultCode, FaultCodeDto, CreateUpdateFaultCodeDto>, IFaultCodeAppService
{
    public FaultCodeAppService(IRepository<FaultCode, Guid> repository)
        : base(repository, ErpPermissions.ServiceSetup.Default) { }

    protected override FaultCode NewEntity(Guid id, CreateUpdateFaultCodeDto input) => new(id, input.Code, input.Description);
}

public class ResolutionCodeAppService : CodeTableAppServiceBase<ResolutionCode, ResolutionCodeDto, CreateUpdateResolutionCodeDto>, IResolutionCodeAppService
{
    public ResolutionCodeAppService(IRepository<ResolutionCode, Guid> repository)
        : base(repository, ErpPermissions.ServiceSetup.Default) { }

    protected override ResolutionCode NewEntity(Guid id, CreateUpdateResolutionCodeDto input) => new(id, input.Code, input.Description);
}

/// <summary>Shared lookups of the service app services: the customer, the setup and the next number.</summary>
internal static class ServiceLookups
{
    public static async Task<Customer> CustomerAsync(IRepository<Customer, Guid> customers, string customerNo)
    {
        var no = customerNo?.Trim();
        return await customers.FirstOrDefaultAsync(c => c.No == no)
            ?? throw new BusinessException(ErpErrorCodes.Customers.CustomerNotFound).WithData("accountNo", no ?? "");
    }

    public static async Task<string> NextNoAsync(NoSeriesManager series, string seriesCode, string typedNo, DateTime date) =>
        (await series.ResolveNoAsync(seriesCode, typedNo, date)).Trim().ToUpperInvariant();
}

[Authorize(ErpPermissions.Service.Default)]
public class ServiceItemAppService
    : ErpCrudAppService<ServiceItem, ServiceItemDto, Guid, GetCodeTableListInput, CreateUpdateServiceItemDto, CreateUpdateServiceItemDto>,
        IServiceItemAppService
{
    private ServicePostingEngine Engine => LazyServiceProvider.LazyGetRequiredService<ServicePostingEngine>();

    public ServiceItemAppService(IRepository<ServiceItem, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Service.Default;
        GetListPolicyName = ErpPermissions.Service.Default;
        CreatePolicyName = ErpPermissions.Service.Create;
        UpdatePolicyName = ErpPermissions.Service.Update;
        DeletePolicyName = ErpPermissions.Service.Delete;
    }

    public override async Task<ServiceItemDto> CreateAsync(CreateUpdateServiceItemDto input)
    {
        await CheckCreatePolicyAsync();
        var setup = await Engine.GetSetupAsync();
        var no = await ServiceLookups.NextNoAsync(LazyServiceProvider.LazyGetRequiredService<NoSeriesManager>(), setup.ServiceItemNos, input.No, Clock.Now.Date);
        if (await Repository.AnyAsync(s => s.No == no))
        {
            throw new BusinessException(ErpErrorCodes.Service.AlreadyExists).WithData("no", no);
        }

        var serviceItem = new ServiceItem(GuidGenerator.Create(), no);
        await ApplyAsync(serviceItem, input, setup);
        await Repository.InsertAsync(serviceItem, autoSave: true);
        return await MapToGetOutputDtoAsync(serviceItem);
    }

    public override async Task<ServiceItemDto> UpdateAsync(Guid id, CreateUpdateServiceItemDto input)
    {
        await CheckUpdatePolicyAsync();
        var serviceItem = await GetEntityByIdAsync(id);
        await ApplyAsync(serviceItem, input, await Engine.GetSetupAsync());
        await Repository.UpdateAsync(serviceItem, autoSave: true);
        return await MapToGetOutputDtoAsync(serviceItem);
    }

    private async Task ApplyAsync(ServiceItem serviceItem, CreateUpdateServiceItemDto input, ServiceMgtSetup setup)
    {
        var customer = await ServiceLookups.CustomerAsync(LazyServiceProvider.LazyGetRequiredService<IRepository<Customer, Guid>>(), input.CustomerNo);
        string description = input.Description;
        var itemNo = input.ItemNo?.Trim();
        if (!itemNo.IsNullOrEmpty())
        {
            var item = await LazyServiceProvider.LazyGetRequiredService<IRepository<Item, Guid>>().FirstOrDefaultAsync(i => i.No == itemNo)
                ?? throw new BusinessException(ErpErrorCodes.Items.ItemNotFound).WithData("no", itemNo);
            description ??= item.Description;
        }

        // A serial number belongs to one piece of equipment: no two service items of an item share it.
        var serialNo = input.SerialNo?.Trim();
        if (!serialNo.IsNullOrEmpty() && await Repository.AnyAsync(s => s.Id != serviceItem.Id && s.ItemNo == itemNo && s.SerialNo == serialNo))
        {
            throw new BusinessException(ErpErrorCodes.Service.SerialNoInUse).WithData("serialNo", serialNo).WithData("itemNo", itemNo ?? "");
        }

        serviceItem.Set(description, customer.Id, customer.No, customer.Name, itemNo, serialNo, input.InstallationDate, input.Blocked);
        var start = input.WarrantyStartingDate ?? input.InstallationDate;
        var end = input.WarrantyEndingDate ?? (start.HasValue && setup.DefaultWarrantyMonths > 0 ? start.Value.AddMonths(setup.DefaultWarrantyMonths).AddDays(-1) : null);
        serviceItem.SetWarranty(start, end);
    }

    protected override async Task<IQueryable<ServiceItem>> CreateFilteredQueryAsync(GetCodeTableListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        var filter = input.Filter?.Trim().ToLower();
        return query.WhereIf(
            !filter.IsNullOrEmpty(),
            s => s.No.ToLower().Contains(filter)
                || (s.Description != null && s.Description.ToLower().Contains(filter))
                || (s.SerialNo != null && s.SerialNo.ToLower().Contains(filter))
                || (s.CustomerName != null && s.CustomerName.ToLower().Contains(filter))
        );
    }

    protected override IQueryable<ServiceItem> ApplyDefaultSorting(IQueryable<ServiceItem> query) => query.OrderBy(s => s.No);
}

[Authorize(ErpPermissions.Service.Default)]
public class ServiceContractAppService
    : ErpCrudAppService<ServiceContract, ServiceContractDto, Guid, GetCodeTableListInput, CreateUpdateServiceContractDto, CreateUpdateServiceContractDto>,
        IServiceContractAppService
{
    private ServicePostingEngine Engine => LazyServiceProvider.LazyGetRequiredService<ServicePostingEngine>();

    public ServiceContractAppService(IRepository<ServiceContract, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Service.Default;
        GetListPolicyName = ErpPermissions.Service.Default;
        CreatePolicyName = ErpPermissions.Service.Create;
        UpdatePolicyName = ErpPermissions.Service.Update;
        DeletePolicyName = ErpPermissions.Service.Delete;
    }

    protected override async Task<ServiceContract> GetEntityByIdAsync(Guid id) => await Repository.GetAsync(id, includeDetails: true);

    public override async Task<ServiceContractDto> CreateAsync(CreateUpdateServiceContractDto input)
    {
        await CheckCreatePolicyAsync();
        var setup = await Engine.GetSetupAsync();
        var date = input.StartingDate == default ? Clock.Now.Date : input.StartingDate;
        var no = await ServiceLookups.NextNoAsync(LazyServiceProvider.LazyGetRequiredService<NoSeriesManager>(), setup.ServiceContractNos, input.No, date);
        if (await Repository.AnyAsync(c => c.No == no))
        {
            throw new BusinessException(ErpErrorCodes.Service.AlreadyExists).WithData("no", no);
        }

        var contract = new ServiceContract(GuidGenerator.Create(), no);
        await ApplyAsync(contract, input);
        await Repository.InsertAsync(contract, autoSave: true);
        return await MapToGetOutputDtoAsync(contract);
    }

    public override async Task<ServiceContractDto> UpdateAsync(Guid id, CreateUpdateServiceContractDto input)
    {
        await CheckUpdatePolicyAsync();
        var contract = await GetEntityByIdAsync(id);
        await ApplyAsync(contract, input);
        await Repository.UpdateAsync(contract, autoSave: true);
        return await MapToGetOutputDtoAsync(contract);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();
        var contract = await GetEntityByIdAsync(id);
        contract.EnsureDraft();
        await Repository.DeleteAsync(contract);
    }

    [Authorize(ErpPermissions.Service.Update)]
    public async Task<ServiceContractDto> SignAsync(Guid id)
    {
        var contract = await GetEntityByIdAsync(id);
        contract.Sign();
        await Repository.UpdateAsync(contract, autoSave: true);
        return await MapToGetOutputDtoAsync(contract);
    }

    [Authorize(ErpPermissions.Service.Update)]
    public async Task<ServiceContractDto> CancelAsync(Guid id)
    {
        var contract = await GetEntityByIdAsync(id);
        contract.Cancel();
        await Repository.UpdateAsync(contract, autoSave: true);
        return await MapToGetOutputDtoAsync(contract);
    }

    [Authorize(ErpPermissions.Service.Post)]
    public async Task<ServiceContractInvoicingResultDto> InvoiceDueAsync(Guid id, InvoiceServiceContractsInput input) =>
        new() { InvoicedPeriods = await Engine.InvoiceContractsAsync(input.AsOfDate == default ? Clock.Now.Date : input.AsOfDate, id) };

    [Authorize(ErpPermissions.Service.Post)]
    public async Task<ServiceContractInvoicingResultDto> InvoiceAllDueAsync(InvoiceServiceContractsInput input) =>
        new() { InvoicedPeriods = await Engine.InvoiceContractsAsync(input.AsOfDate == default ? Clock.Now.Date : input.AsOfDate) };

    private async Task ApplyAsync(ServiceContract contract, CreateUpdateServiceContractDto input)
    {
        var customer = await ServiceLookups.CustomerAsync(LazyServiceProvider.LazyGetRequiredService<IRepository<Customer, Guid>>(), input.CustomerNo);
        contract.Set(input.Description, customer.Id, customer.No, customer.Name, input.StartingDate == default ? Clock.Now.Date : input.StartingDate, input.ExpirationDate, input.InvoicePeriod);
    }

    protected override async Task<IQueryable<ServiceContract>> CreateFilteredQueryAsync(GetCodeTableListInput input)
    {
        var query = await Repository.WithDetailsAsync(c => c.Lines);
        var filter = input.Filter?.Trim().ToLower();
        return query.WhereIf(
            !filter.IsNullOrEmpty(),
            c => c.No.ToLower().Contains(filter) || (c.Description != null && c.Description.ToLower().Contains(filter)) || (c.CustomerName != null && c.CustomerName.ToLower().Contains(filter))
        );
    }

    protected override IQueryable<ServiceContract> ApplyDefaultSorting(IQueryable<ServiceContract> query) => query.OrderBy(c => c.No);
}

[Authorize(ErpPermissions.Service.Default)]
public class ServiceContractLineAppService
    : ErpCrudAppService<ServiceContractLine, ServiceContractLineDto, Guid, GetServiceContractLineListInput, CreateUpdateServiceContractLineDto, CreateUpdateServiceContractLineDto>,
        IServiceContractLineAppService
{
    private readonly IRepository<ServiceContract, Guid> _contracts;

    public ServiceContractLineAppService(IRepository<ServiceContractLine, Guid> repository, IRepository<ServiceContract, Guid> contracts)
        : base(repository)
    {
        _contracts = contracts;
        GetPolicyName = ErpPermissions.Service.Default;
        GetListPolicyName = ErpPermissions.Service.Default;
        CreatePolicyName = ErpPermissions.Service.Update;
        UpdatePolicyName = ErpPermissions.Service.Update;
        DeletePolicyName = ErpPermissions.Service.Update;
    }

    public override async Task<ServiceContractLineDto> CreateAsync(CreateUpdateServiceContractLineDto input)
    {
        await CheckCreatePolicyAsync();
        var contract = await _contracts.GetAsync(input.ServiceContractId, includeDetails: true);
        var description = await DescriptionAsync(contract, input);
        var line = contract.AddLine(GuidGenerator.Create(), input.ServiceItemNo, description, input.LineValue);
        await _contracts.UpdateAsync(contract, autoSave: true);
        return await MapToGetOutputDtoAsync(line);
    }

    public override async Task<ServiceContractLineDto> UpdateAsync(Guid id, CreateUpdateServiceContractLineDto input)
    {
        await CheckUpdatePolicyAsync();
        var line = await GetEntityByIdAsync(id);
        var contract = await _contracts.GetAsync(line.ServiceContractId);
        contract.EnsureDraft();
        line.Set(input.ServiceItemNo, await DescriptionAsync(contract, input), input.LineValue);
        await Repository.UpdateAsync(line, autoSave: true);
        return await MapToGetOutputDtoAsync(line);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();
        var line = await GetEntityByIdAsync(id);
        (await _contracts.GetAsync(line.ServiceContractId)).EnsureDraft();
        await Repository.DeleteAsync(line);
    }

    private async Task<string> DescriptionAsync(ServiceContract contract, CreateUpdateServiceContractLineDto input)
    {
        if (input.ServiceItemNo.IsNullOrWhiteSpace())
        {
            return input.Description;
        }

        var no = input.ServiceItemNo.Trim().ToUpperInvariant();
        var serviceItem = await LazyServiceProvider.LazyGetRequiredService<IRepository<ServiceItem, Guid>>().FirstOrDefaultAsync(s => s.No == no)
            ?? throw new BusinessException(ErpErrorCodes.Service.ServiceItemNotFound).WithData("no", no);
        if (serviceItem.CustomerId != contract.CustomerId)
        {
            throw new BusinessException(ErpErrorCodes.Service.ServiceItemOfOtherCustomer).WithData("no", no).WithData("customerNo", contract.CustomerNo ?? "");
        }

        return input.Description ?? serviceItem.Description;
    }

    protected override async Task<IQueryable<ServiceContractLine>> CreateFilteredQueryAsync(GetServiceContractLineListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        return query.WhereIf(input.ServiceContractId.HasValue, l => l.ServiceContractId == input.ServiceContractId.Value);
    }

    protected override IQueryable<ServiceContractLine> ApplyDefaultSorting(IQueryable<ServiceContractLine> query) => query.OrderBy(l => l.LineNo);
}

[Authorize(ErpPermissions.Service.Default)]
public class ServiceOrderAppService
    : ErpCrudAppService<ServiceOrder, ServiceOrderDto, Guid, GetServiceOrderListInput, CreateUpdateServiceOrderDto, CreateUpdateServiceOrderDto>,
        IServiceOrderAppService
{
    private ServicePostingEngine Engine => LazyServiceProvider.LazyGetRequiredService<ServicePostingEngine>();

    public ServiceOrderAppService(IRepository<ServiceOrder, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Service.Default;
        GetListPolicyName = ErpPermissions.Service.Default;
        CreatePolicyName = ErpPermissions.Service.Create;
        UpdatePolicyName = ErpPermissions.Service.Update;
        DeletePolicyName = ErpPermissions.Service.Delete;
    }

    protected override async Task<ServiceOrder> GetEntityByIdAsync(Guid id) => await Repository.GetAsync(id, includeDetails: true);

    public override async Task<ServiceOrderDto> CreateAsync(CreateUpdateServiceOrderDto input)
    {
        await CheckCreatePolicyAsync();
        var setup = await Engine.GetSetupAsync();
        var date = input.OrderDate == default ? Clock.Now.Date : input.OrderDate;
        var no = await ServiceLookups.NextNoAsync(LazyServiceProvider.LazyGetRequiredService<NoSeriesManager>(), setup.ServiceOrderNos, input.No, date);
        if (await Repository.AnyAsync(o => o.No == no))
        {
            throw new BusinessException(ErpErrorCodes.Service.AlreadyExists).WithData("no", no);
        }

        var order = new ServiceOrder(GuidGenerator.Create(), no);
        await ApplyAsync(order, input);
        await Repository.InsertAsync(order, autoSave: true);
        return await MapToGetOutputDtoAsync(order);
    }

    public override async Task<ServiceOrderDto> UpdateAsync(Guid id, CreateUpdateServiceOrderDto input)
    {
        await CheckUpdatePolicyAsync();
        var order = await GetEntityByIdAsync(id);
        if (order.Lines.Any(l => l.QuantityPosted != 0m) && !string.Equals(order.CustomerNo, input.CustomerNo?.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException(ErpErrorCodes.Service.LinePosted).WithData("no", order.No);
        }

        await ApplyAsync(order, input);
        await Repository.UpdateAsync(order, autoSave: true);
        return await MapToGetOutputDtoAsync(order);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();
        var order = await GetEntityByIdAsync(id);
        if (order.Lines.Any(l => l.QuantityPosted != 0m))
        {
            throw new BusinessException(ErpErrorCodes.Service.LinePosted).WithData("no", order.No);
        }

        await Repository.DeleteAsync(order);
    }

    [Authorize(ErpPermissions.Service.Update)]
    public async Task<ServiceOrderDto> ChangeStatusAsync(Guid id, ChangeServiceOrderStatusInput input)
    {
        var order = await GetEntityByIdAsync(id);
        order.SetStatus(input.Status);
        await Repository.UpdateAsync(order, autoSave: true);
        return await MapToGetOutputDtoAsync(order);
    }

    [Authorize(ErpPermissions.Service.Post)]
    public async Task<ServicePostingResultDto> RunPostingAsync(Guid id, ServicePostingInput input)
    {
        var result = await Engine.PostOrderAsync(id, input.PostingDate == default ? Clock.Now.Date : input.PostingDate);
        return new ServicePostingResultDto { InvoiceNo = result.InvoiceNo, LineCount = result.LineCount, Amount = result.Amount, WarrantyAmount = result.WarrantyAmount };
    }

    private async Task ApplyAsync(ServiceOrder order, CreateUpdateServiceOrderDto input)
    {
        var customer = await ServiceLookups.CustomerAsync(LazyServiceProvider.LazyGetRequiredService<IRepository<Customer, Guid>>(), input.CustomerNo);
        await LazyServiceProvider.LazyGetRequiredService<CodeTableChecker>().EnsureExistsAsync<ServiceOrderType>(input.ServiceOrderType);

        var resourceNo = input.ResourceNo?.Trim().ToUpperInvariant();
        if (!resourceNo.IsNullOrEmpty() && !await LazyServiceProvider.LazyGetRequiredService<IRepository<Resource, Guid>>().AnyAsync(r => r.No == resourceNo))
        {
            throw new BusinessException(ErpErrorCodes.Service.ResourceNotFound).WithData("no", resourceNo);
        }

        var contractNo = input.ContractNo?.Trim().ToUpperInvariant();
        if (!contractNo.IsNullOrEmpty())
        {
            var contract = await LazyServiceProvider.LazyGetRequiredService<IRepository<ServiceContract, Guid>>().FirstOrDefaultAsync(c => c.No == contractNo)
                ?? throw new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(ServiceContract), contractNo);
            if (contract.CustomerId != customer.Id)
            {
                throw new BusinessException(ErpErrorCodes.Service.ServiceItemOfOtherCustomer).WithData("no", contractNo).WithData("customerNo", customer.No);
            }
        }

        order.Set(input.Description, customer.Id, customer.No, customer.Name, input.ServiceOrderType, input.Priority, input.OrderDate == default ? Clock.Now.Date : input.OrderDate, resourceNo, contractNo);
    }

    protected override async Task<ServiceOrderDto> MapToGetOutputDtoAsync(ServiceOrder entity)
    {
        var dto = await base.MapToGetOutputDtoAsync(entity);
        Totals(dto, entity);
        return dto;
    }

    protected override async Task<List<ServiceOrderDto>> MapToGetListOutputDtosAsync(List<ServiceOrder> entities)
    {
        var dtos = await base.MapToGetListOutputDtosAsync(entities);
        for (var i = 0; i < dtos.Count; i++)
        {
            Totals(dtos[i], entities[i]);
        }

        return dtos;
    }

    private static void Totals(ServiceOrderDto dto, ServiceOrder order)
    {
        dto.Amount = order.Lines.Where(l => !l.Warranty).Sum(l => l.LineAmount);
        dto.WarrantyAmount = order.Lines.Where(l => l.Warranty).Sum(l => l.LineAmount);
        dto.ItemLines = dto.ItemLines.OrderBy(l => l.LineNo).ToList();
        dto.Lines = dto.Lines.OrderBy(l => l.LineNo).ToList();
    }

    protected override async Task<IQueryable<ServiceOrder>> CreateFilteredQueryAsync(GetServiceOrderListInput input)
    {
        var query = await Repository.WithDetailsAsync();
        var filter = input.Filter?.Trim().ToLower();
        var resourceNo = input.ResourceNo?.Trim().ToUpper();
        return query
            .WhereIf(input.Status.HasValue, o => o.Status == input.Status.Value)
            .WhereIf(!resourceNo.IsNullOrEmpty(), o => o.ResourceNo == resourceNo)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                o => o.No.ToLower().Contains(filter) || (o.Description != null && o.Description.ToLower().Contains(filter)) || (o.CustomerName != null && o.CustomerName.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<ServiceOrder> ApplyDefaultSorting(IQueryable<ServiceOrder> query) => query.OrderByDescending(o => o.CreationTime);
}

[Authorize(ErpPermissions.Service.Default)]
public class ServiceItemLineAppService
    : ErpCrudAppService<ServiceItemLine, ServiceItemLineDto, Guid, GetServiceOrderLineListInput, CreateUpdateServiceItemLineDto, CreateUpdateServiceItemLineDto>,
        IServiceItemLineAppService
{
    private readonly IRepository<ServiceOrder, Guid> _orders;

    public ServiceItemLineAppService(IRepository<ServiceItemLine, Guid> repository, IRepository<ServiceOrder, Guid> orders)
        : base(repository)
    {
        _orders = orders;
        GetPolicyName = ErpPermissions.Service.Default;
        GetListPolicyName = ErpPermissions.Service.Default;
        CreatePolicyName = ErpPermissions.Service.Update;
        UpdatePolicyName = ErpPermissions.Service.Update;
        DeletePolicyName = ErpPermissions.Service.Update;
    }

    public override async Task<ServiceItemLineDto> CreateAsync(CreateUpdateServiceItemLineDto input)
    {
        await CheckCreatePolicyAsync();
        var order = await _orders.GetAsync(input.ServiceOrderId, includeDetails: true);
        var no = input.ServiceItemNo?.Trim().ToUpperInvariant();
        var serviceItem = await LazyServiceProvider.LazyGetRequiredService<IRepository<ServiceItem, Guid>>().FirstOrDefaultAsync(s => s.No == no)
            ?? throw new BusinessException(ErpErrorCodes.Service.ServiceItemNotFound).WithData("no", no ?? "");
        if (serviceItem.CustomerId != order.CustomerId)
        {
            throw new BusinessException(ErpErrorCodes.Service.ServiceItemOfOtherCustomer).WithData("no", no).WithData("customerNo", order.CustomerNo ?? "");
        }

        var line = order.AddItemLine(GuidGenerator.Create(), serviceItem);
        await ApplyAsync(line, input);
        await _orders.UpdateAsync(order, autoSave: true);
        return await MapToGetOutputDtoAsync(line);
    }

    public override async Task<ServiceItemLineDto> UpdateAsync(Guid id, CreateUpdateServiceItemLineDto input)
    {
        await CheckUpdatePolicyAsync();
        var line = await GetEntityByIdAsync(id);
        (await _orders.GetAsync(line.ServiceOrderId, includeDetails: false)).EnsureNotFinished();
        await ApplyAsync(line, input);
        await Repository.UpdateAsync(line, autoSave: true);
        return await MapToGetOutputDtoAsync(line);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();
        var line = await GetEntityByIdAsync(id);
        (await _orders.GetAsync(line.ServiceOrderId, includeDetails: false)).EnsureNotFinished();
        await Repository.DeleteAsync(line);
    }

    private async Task ApplyAsync(ServiceItemLine line, CreateUpdateServiceItemLineDto input)
    {
        var codes = LazyServiceProvider.LazyGetRequiredService<CodeTableChecker>();
        await codes.EnsureExistsAsync<FaultCode>(input.FaultCode);
        await codes.EnsureExistsAsync<ResolutionCode>(input.ResolutionCode);
        line.Set(input.FaultCode, input.ResolutionCode, input.FaultComment, input.Warranty ?? line.Warranty);
    }

    protected override async Task<IQueryable<ServiceItemLine>> CreateFilteredQueryAsync(GetServiceOrderLineListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        return query.WhereIf(input.ServiceOrderId.HasValue, l => l.ServiceOrderId == input.ServiceOrderId.Value);
    }

    protected override IQueryable<ServiceItemLine> ApplyDefaultSorting(IQueryable<ServiceItemLine> query) => query.OrderBy(l => l.LineNo);
}

[Authorize(ErpPermissions.Service.Default)]
public class ServiceLineAppService
    : ErpCrudAppService<ServiceLine, ServiceLineDto, Guid, GetServiceOrderLineListInput, CreateUpdateServiceLineDto, CreateUpdateServiceLineDto>,
        IServiceLineAppService
{
    private readonly IRepository<ServiceOrder, Guid> _orders;

    public ServiceLineAppService(IRepository<ServiceLine, Guid> repository, IRepository<ServiceOrder, Guid> orders)
        : base(repository)
    {
        _orders = orders;
        GetPolicyName = ErpPermissions.Service.Default;
        GetListPolicyName = ErpPermissions.Service.Default;
        CreatePolicyName = ErpPermissions.Service.Update;
        UpdatePolicyName = ErpPermissions.Service.Update;
        DeletePolicyName = ErpPermissions.Service.Update;
    }

    public override async Task<ServiceLineDto> CreateAsync(CreateUpdateServiceLineDto input)
    {
        await CheckCreatePolicyAsync();
        var order = await _orders.GetAsync(input.ServiceOrderId, includeDetails: true);
        var line = order.AddLine(GuidGenerator.Create());
        await ApplyAsync(order, line, input);
        await _orders.UpdateAsync(order, autoSave: true);
        return await MapToGetOutputDtoAsync(line);
    }

    public override async Task<ServiceLineDto> UpdateAsync(Guid id, CreateUpdateServiceLineDto input)
    {
        await CheckUpdatePolicyAsync();
        var line = await GetEntityByIdAsync(id);
        var order = await _orders.GetAsync(line.ServiceOrderId, includeDetails: true);
        order.EnsureNotFinished();
        await ApplyAsync(order, line, input);
        await Repository.UpdateAsync(line, autoSave: true);
        return await MapToGetOutputDtoAsync(line);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();
        var line = await GetEntityByIdAsync(id);
        if (line.QuantityPosted != 0m)
        {
            throw new BusinessException(ErpErrorCodes.Service.LinePosted).WithData("no", line.No ?? "");
        }

        (await _orders.GetAsync(line.ServiceOrderId, includeDetails: false)).EnsureNotFinished();
        await Repository.DeleteAsync(line);
    }

    /// <summary>Fills in the price, cost and description of the item, resource or account, and the warranty of the service item.</summary>
    private async Task ApplyAsync(ServiceOrder order, ServiceLine line, CreateUpdateServiceLineDto input)
    {
        var no = input.No?.Trim();
        string description = input.Description;
        decimal price = 0m, cost = 0m;
        switch (input.Type)
        {
            case ServiceLineType.Item:
                var item = await LazyServiceProvider.LazyGetRequiredService<IRepository<Item, Guid>>().FirstOrDefaultAsync(i => i.No == no)
                    ?? throw new BusinessException(ErpErrorCodes.Items.ItemNotFound).WithData("no", no ?? "");
                await LazyServiceProvider.LazyGetRequiredService<CodeTableChecker>().EnsureExistsAsync<Location>(input.LocationCode);
                (description, price, cost) = (description ?? item.Description, item.UnitPrice, item.UnitCost);
                break;
            case ServiceLineType.Resource:
                no = no?.ToUpperInvariant();
                var resource = await LazyServiceProvider.LazyGetRequiredService<IRepository<Resource, Guid>>().FirstOrDefaultAsync(r => r.No == no)
                    ?? throw new BusinessException(ErpErrorCodes.Service.ResourceNotFound).WithData("no", no ?? "");
                (description, price, cost) = (description ?? resource.Name, resource.UnitPrice, resource.UnitCost);
                break;
            default:
                await LazyServiceProvider.LazyGetRequiredService<PostingSetupManager>().EnsureGLAccountsExistAsync(no);
                description ??= (await LazyServiceProvider.LazyGetRequiredService<IRepository<GLAccount, Guid>>().FirstOrDefaultAsync(a => a.No == no))?.Name;
                break;
        }

        var serviceItemNo = input.ServiceItemNo?.Trim().ToUpperInvariant();
        var itemLine = serviceItemNo.IsNullOrEmpty() ? null : order.ItemLines.FirstOrDefault(l => l.ServiceItemNo == serviceItemNo);
        if (!serviceItemNo.IsNullOrEmpty() && itemLine == null)
        {
            throw new BusinessException(ErpErrorCodes.Service.ServiceItemNotFound).WithData("no", serviceItemNo);
        }

        line.Set(
            serviceItemNo,
            input.Type,
            no,
            description,
            input.Type == ServiceLineType.Item ? input.LocationCode : null,
            input.Quantity,
            input.UnitPrice ?? price,
            input.UnitCost ?? cost,
            input.LineDiscountPct,
            input.Warranty ?? itemLine?.Warranty ?? false
        );
    }

    protected override async Task<IQueryable<ServiceLine>> CreateFilteredQueryAsync(GetServiceOrderLineListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        return query.WhereIf(input.ServiceOrderId.HasValue, l => l.ServiceOrderId == input.ServiceOrderId.Value);
    }

    protected override IQueryable<ServiceLine> ApplyDefaultSorting(IQueryable<ServiceLine> query) => query.OrderBy(l => l.LineNo);
}

[Authorize(ErpPermissions.Service.Default)]
public class ServiceLedgerEntryAppService : ErpReadOnlyAppService<ServiceLedgerEntry, ServiceLedgerEntryDto, Guid, GetServiceLedgerListInput>, IServiceLedgerEntryAppService
{
    public ServiceLedgerEntryAppService(IRepository<ServiceLedgerEntry, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Service.Default;
        GetListPolicyName = ErpPermissions.Service.Default;
    }

    protected override async Task<IQueryable<ServiceLedgerEntry>> CreateFilteredQueryAsync(GetServiceLedgerListInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();
        return query
            .WhereIf(!input.ServiceOrderNo.IsNullOrWhiteSpace(), e => e.ServiceOrderNo == input.ServiceOrderNo)
            .WhereIf(!input.ServiceItemNo.IsNullOrWhiteSpace(), e => e.ServiceItemNo == input.ServiceItemNo);
    }

    protected override IQueryable<ServiceLedgerEntry> ApplyDefaultSorting(IQueryable<ServiceLedgerEntry> query) => query.OrderByDescending(e => e.EntryNo);
}

[Authorize(ErpPermissions.Service.Default)]
public class WarrantyLedgerEntryAppService : ErpReadOnlyAppService<WarrantyLedgerEntry, WarrantyLedgerEntryDto, Guid, GetServiceLedgerListInput>, IWarrantyLedgerEntryAppService
{
    public WarrantyLedgerEntryAppService(IRepository<WarrantyLedgerEntry, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Service.Default;
        GetListPolicyName = ErpPermissions.Service.Default;
    }

    protected override async Task<IQueryable<WarrantyLedgerEntry>> CreateFilteredQueryAsync(GetServiceLedgerListInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();
        return query
            .WhereIf(!input.ServiceOrderNo.IsNullOrWhiteSpace(), e => e.ServiceOrderNo == input.ServiceOrderNo)
            .WhereIf(!input.ServiceItemNo.IsNullOrWhiteSpace(), e => e.ServiceItemNo == input.ServiceItemNo);
    }

    protected override IQueryable<WarrantyLedgerEntry> ApplyDefaultSorting(IQueryable<WarrantyLedgerEntry> query) => query.OrderByDescending(e => e.EntryNo);
}
