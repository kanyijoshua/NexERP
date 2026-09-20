using ABPmicroservice.Erp.Companies;
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

    /// <summary>The company this request is working in, resolved from the X-Company-Id header.</summary>
    protected ICurrentCompany CurrentCompany => LazyServiceProvider.LazyGetRequiredService<ICurrentCompany>();
}
