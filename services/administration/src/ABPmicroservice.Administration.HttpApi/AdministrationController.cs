using ABPmicroservice.Administration.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace ABPmicroservice.Administration;

public abstract class AdministrationController : AbpControllerBase
{
    protected AdministrationController()
    {
        LocalizationResource = typeof(AdministrationResource);
    }
}
