using ABPmicroservice.IdentityService.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace ABPmicroservice.IdentityService;

public abstract class IdentityServiceController : AbpControllerBase
{
    protected IdentityServiceController()
    {
        LocalizationResource = typeof(IdentityServiceResource);
    }
}
