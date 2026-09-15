using ABPmicroservice.SaaS.Localization;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.SaaS;

public abstract class SaaSAppService : ApplicationService
{
    protected SaaSAppService()
    {
        LocalizationResource = typeof(SaaSResource);
        ObjectMapperContext = typeof(SaaSApplicationModule);
    }
}
