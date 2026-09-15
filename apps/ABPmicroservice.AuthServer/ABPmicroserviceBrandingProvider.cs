using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace ABPmicroservice;

[Dependency(ReplaceServices = true)]
public class ABPmicroserviceBrandingProvider : DefaultBrandingProvider
{
    public override string AppName => "ABPmicroservice";
}
