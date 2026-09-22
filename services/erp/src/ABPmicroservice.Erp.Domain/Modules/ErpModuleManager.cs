using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Guids;

namespace ABPmicroservice.Erp.Modules;

/// <summary>
/// Which modules this company runs.
/// <para>
/// The rules are the ones Odoo enforces when you install or uninstall an app: a module cannot be
/// switched on while something it needs is off, and cannot be switched off while something that
/// needs it is on. Core modules are never switchable.
/// </para>
/// </summary>
public class ErpModuleManager : DomainService
{
    private readonly IRepository<ErpModuleState, Guid> _stateRepository;
    private readonly ICurrentCompany _currentCompany;
    private readonly IGuidGenerator _guidGenerator;

    public ErpModuleManager(
        IRepository<ErpModuleState, Guid> stateRepository,
        ICurrentCompany currentCompany,
        IGuidGenerator guidGenerator
    )
    {
        _stateRepository = stateRepository;
        _currentCompany = currentCompany;
        _guidGenerator = guidGenerator;
    }

    /// <summary>Every module with the state it stands at in this company.</summary>
    public async Task<IReadOnlyDictionary<string, bool>> GetStatesAsync()
    {
        // With no company in scope nothing is company data yet, so everything reads as on rather
        // than the whole system reading as off.
        if (_currentCompany.Id == null)
        {
            return ErpModuleRegistry.All.ToDictionary(m => m.Code, _ => true, StringComparer.OrdinalIgnoreCase);
        }

        var stored = await _stateRepository.GetListAsync();
        var byCode = stored.ToDictionary(s => s.ModuleCode, s => s.Enabled, StringComparer.OrdinalIgnoreCase);

        return ErpModuleRegistry.All.ToDictionary(
            m => m.Code,
            m => m.IsCore || (byCode.TryGetValue(m.Code, out var enabled) ? enabled : true),
            StringComparer.OrdinalIgnoreCase
        );
    }

    public async Task<bool> IsEnabledAsync(string moduleCode)
    {
        var module = ErpModuleRegistry.Find(moduleCode);
        if (module == null)
        {
            return true;
        }

        if (module.IsCore)
        {
            return true;
        }

        var states = await GetStatesAsync();
        return states.TryGetValue(module.Code, out var enabled) && enabled;
    }

    public async Task EnsureEnabledAsync(string moduleCode)
    {
        if (!await IsEnabledAsync(moduleCode))
        {
            throw new BusinessException(ErpErrorCodes.Modules.ModuleIsDisabled).WithData("module", moduleCode);
        }
    }

    public async Task SetEnabledAsync(string moduleCode, bool enabled)
    {
        var module = ErpModuleRegistry.Get(moduleCode);

        if (module.IsCore)
        {
            throw new BusinessException(ErpErrorCodes.Modules.CoreModuleCannotBeDisabled)
                .WithData("module", module.Code);
        }

        var states = await GetStatesAsync();

        if (enabled)
        {
            EnsureDependenciesAreOn(module, states);
        }
        else
        {
            EnsureNothingStillNeedsIt(module, states);
        }

        var existing = await _stateRepository.FirstOrDefaultAsync(s => s.ModuleCode == module.Code);
        if (existing == null)
        {
            await _stateRepository.InsertAsync(
                new ErpModuleState(_guidGenerator.Create(), module.Code, enabled),
                autoSave: true
            );

            return;
        }

        existing.SetEnabled(enabled);
        await _stateRepository.UpdateAsync(existing, autoSave: true);
    }

    private static void EnsureDependenciesAreOn(ErpModuleDefinition module, IReadOnlyDictionary<string, bool> states)
    {
        foreach (var dependency in module.DependsOn)
        {
            if (!states.TryGetValue(dependency, out var on) || !on)
            {
                throw new BusinessException(ErpErrorCodes.Modules.DependencyIsDisabled)
                    .WithData("module", module.Code)
                    .WithData("dependency", dependency);
            }
        }
    }

    private static void EnsureNothingStillNeedsIt(
        ErpModuleDefinition module,
        IReadOnlyDictionary<string, bool> states
    )
    {
        var blocking = ErpModuleRegistry
            .DependantsOf(module.Code)
            .Where(d => states.TryGetValue(d.Code, out var on) && on)
            .Select(d => d.Code)
            .ToList();

        if (blocking.Count > 0)
        {
            throw new BusinessException(ErpErrorCodes.Modules.RequiredByAnotherModule)
                .WithData("module", module.Code)
                .WithData("dependants", string.Join(", ", blocking));
        }
    }
}
