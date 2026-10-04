using System;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Inventory;

/// <summary>Locations.</summary>
[Authorize(ErpPermissions.Locations.Default)]
public class LocationAppService : CodeTableAppServiceBase<Location, LocationDto, CreateUpdateLocationDto>, ILocationAppService
{
    public LocationAppService(IRepository<Location, Guid> repository)
        : base(repository, ErpPermissions.Locations.Default) { }

    protected override Location NewEntity(Guid id, CreateUpdateLocationDto input) =>
        new(id, input.Code, input.Description);

    protected override Task ApplyAsync(Location entity, CreateUpdateLocationDto input)
    {
        entity.SetAddress(input.Address, input.City, input.PostCode, input.CountryRegionCode);
        entity.SetContact(input.Contact, input.PhoneNo);
        return Task.CompletedTask;
    }
}

/// <summary>Inventory Setup: one record per company, created on first read.</summary>
[Authorize(ErpPermissions.InventorySetup.Default)]
public class InventorySetupAppService : ErpAppService, IInventorySetupAppService
{
    private readonly InventorySetupManager _setupManager;
    private readonly IRepository<InventorySetup, Guid> _repository;
    private readonly NoSeriesCodeValidator _codeValidator;

    public InventorySetupAppService(
        InventorySetupManager setupManager,
        IRepository<InventorySetup, Guid> repository,
        NoSeriesCodeValidator codeValidator
    )
    {
        _setupManager = setupManager;
        _repository = repository;
        _codeValidator = codeValidator;
    }

    public async Task<InventorySetupDto> GetAsync()
    {
        return ObjectMapper.Map<InventorySetup, InventorySetupDto>(await _setupManager.GetAsync());
    }

    [Authorize(ErpPermissions.InventorySetup.Update)]
    public async Task<InventorySetupDto> UpdateAsync(InventorySetupDto input)
    {
        await _codeValidator.EnsureExistAsync(input.ItemNos);

        var setup = await _setupManager.GetAsync();
        setup.SetNumbering(input.ItemNos);
        setup.SetRules(input.LocationMandatory, input.PreventNegativeInventory, input.AutomaticCostPosting);

        await _repository.UpdateAsync(setup, autoSave: true);
        return ObjectMapper.Map<InventorySetup, InventorySetupDto>(setup);
    }
}
