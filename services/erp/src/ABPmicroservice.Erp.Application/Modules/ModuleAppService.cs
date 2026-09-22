using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;

namespace ABPmicroservice.Erp.Modules;

/// <summary>
/// The system's own apps: which are on in this company, and turning them on and off.
/// Mirrors Odoo's Apps page and Business Central's per-company feature management.
/// </summary>
[Authorize(ErpPermissions.Modules.Default)]
public class ModuleAppService : ErpAppService, IModuleAppService
{
    private readonly ErpModuleManager _moduleManager;

    public ModuleAppService(ErpModuleManager moduleManager)
    {
        _moduleManager = moduleManager;
    }

    public async Task<ListResultDto<ErpModuleDto>> GetListAsync()
    {
        var states = await _moduleManager.GetStatesAsync();

        return new ListResultDto<ErpModuleDto>(ErpModuleRegistry.All.Select(m => Map(m, states)).ToList());
    }

    [Authorize(ErpPermissions.Modules.Manage)]
    public async Task<ErpModuleDto> SetEnabledAsync(SetModuleEnabledInput input)
    {
        await _moduleManager.SetEnabledAsync(input.Code, input.Enabled);

        var states = await _moduleManager.GetStatesAsync();

        return Map(ErpModuleRegistry.Get(input.Code), states);
    }

    private ErpModuleDto Map(ErpModuleDefinition module, IReadOnlyDictionary<string, bool> states)
    {
        var enabled = states.TryGetValue(module.Code, out var on) && on;

        return new ErpModuleDto
        {
            Code = module.Code,
            DisplayName = L[$"Module:{module.Code}"],
            Description = L[$"ModuleDescription:{module.Code}"],
            Group = L[$"ModuleGroup:{module.Group}"],
            Icon = module.Icon,
            Route = module.Route,
            Enabled = enabled,
            IsCore = module.IsCore,
            DependsOn = module.DependsOn.ToList(),

            // What the page needs to explain a switch it has to leave locked.
            BlockedBy = ErpModuleRegistry
                .DependantsOf(module.Code)
                .Where(d => states.TryGetValue(d.Code, out var dependantOn) && dependantOn)
                .Select(d => L[$"Module:{d.Code}"].Value)
                .ToList(),
        };
    }
}
