using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Modules;

/// <summary>One module as the Modules page shows it.</summary>
public class ErpModuleDto
{
    public string Code { get; set; }

    /// <summary>Already localized, e.g. "Sales".</summary>
    public string DisplayName { get; set; }

    public string Description { get; set; }

    public string Group { get; set; }

    public string Icon { get; set; }

    /// <summary>Where the module's tile goes, or null when it has no screen of its own yet.</summary>
    public string Route { get; set; }

    public bool Enabled { get; set; }

    /// <summary>A module the system cannot run without: always on, and the switch is locked.</summary>
    public bool IsCore { get; set; }

    public List<string> DependsOn { get; set; } = [];

    /// <summary>Modules that are on and would have to go off first. Empty when it can be switched off.</summary>
    public List<string> BlockedBy { get; set; } = [];
}

public class SetModuleEnabledInput
{
    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string Code { get; set; }

    public bool Enabled { get; set; }
}

/// <summary>
/// Installing and uninstalling the system's own apps. Mirrors Odoo's Apps page and Business
/// Central's per-company feature management.
/// </summary>
public interface IModuleAppService : IApplicationService
{
    Task<ListResultDto<ErpModuleDto>> GetListAsync();

    /// <summary>Routed as POST /api/erp/module/set-enabled.</summary>
    Task<ErpModuleDto> SetEnabledAsync(SetModuleEnabledInput input);
}
