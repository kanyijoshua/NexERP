using ABPmicroservice.Erp.Localization;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp;

public abstract class ErpAppService : ApplicationService
{
    protected ErpAppService()
    {
        LocalizationResource = typeof(ErpResource);
        ObjectMapperContext = typeof(ErpApplicationModule);
    }
}
