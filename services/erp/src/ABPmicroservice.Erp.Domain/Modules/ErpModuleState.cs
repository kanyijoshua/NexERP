using System;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;

namespace ABPmicroservice.Erp.Modules;

/// <summary>
/// Whether one module is switched on in one company.
/// <para>
/// Only modules that have been switched away from their default are stored, so a company that has
/// never touched this has no rows and every module stands at its default.
/// </para>
/// </summary>
public class ErpModuleState : CompanyEntity
{
    public string ModuleCode { get; private set; }

    public bool Enabled { get; private set; }

    protected ErpModuleState() { }

    public ErpModuleState(Guid id, string moduleCode, bool enabled)
        : base(id)
    {
        ModuleCode = Check.NotNullOrWhiteSpace(moduleCode, nameof(moduleCode), ErpDomainConsts.MaxCodeLength);
        Enabled = enabled;
    }

    public void SetEnabled(bool enabled)
    {
        Enabled = enabled;
    }
}
