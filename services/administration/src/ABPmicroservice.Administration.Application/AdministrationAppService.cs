using ABPmicroservice.Administration.Localization;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Administration;

public abstract class AdministrationAppService : ApplicationService
{
    protected AdministrationAppService()
    {
        LocalizationResource = typeof(AdministrationResource);
        ObjectMapperContext = typeof(AdministrationApplicationModule);
    }
}
