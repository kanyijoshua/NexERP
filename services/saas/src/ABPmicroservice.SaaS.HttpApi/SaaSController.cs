using ABPmicroservice.SaaS.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace ABPmicroservice.SaaS;

public abstract class SaaSController : AbpControllerBase
{
    protected SaaSController()
    {
        LocalizationResource = typeof(SaaSResource);
    }
}
